using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using UnityEngine;
using BepInEx;
using Newtonsoft.Json;
using RadialUI;

namespace GroupHideVolumes
{

    [BepInPlugin(Guid, "HideVolumesPlugin", Version)]
    [BepInDependency(RadialUIPlugin.Guid)]
    public class HideVolumesPlugin : BaseUnityPlugin
    {
        // constants
        private const string Guid = "org.hollofox.plugins.HideVolumesPlugin";
        private const string Version = "1.1.1.0";

        private static List<(List<HideVolumeItem>,bool)> groups =
            new List<(List<HideVolumeItem>, bool)>();
        
        public static HideVolumeItem currentHideVolume;
        public static int groupIndex = -1;
        private bool last = false;

        /// <summary>
        /// Awake plugin
        /// </summary>
        void Awake()
        {
            Logger.LogInfo("In Awake for HideVolumes");

            Debug.Log("HideVolumes Plug-in loaded");
            
            ModdingTales.ModdingUtils.Initialize(this, Logger);

            // Register Group Menus in a branch
            RadialUIPlugin.AddOnHideVolume(
                    Guid + "Grouping",
                    new MapMenu.ItemArgs
                    {
                        Title = "Grouping",
                        Action = ShowGroupingSubmenu,
                        Icon = sprite("collection.png")
                    }, StoreCurrentHideVolume
            );
        }

        private static void ShowGroupingSubmenu(MapMenuItem item, object obj)
        {
            MapMenu mapMenu = MapMenuManager.OpenMenu(item, MapMenu.MenuType.BRANCH);
            mapMenu.AddItem(new MapMenu.ItemArgs { Title = "Create Group", CloseMenuOnActivate = true, Action = CreateGroup, Icon = Icons.GetIconSprite("dungeonmaster") });
            if (GroupSelected()) mapMenu.AddItem(new MapMenu.ItemArgs {Title = "Set Group", CloseMenuOnActivate = true, Action = SetGroup});
            if (CanRemove(currentHideVolume)) mapMenu.AddItem(new MapMenu.ItemArgs {Title = "Remove from Group", CloseMenuOnActivate = true, Action = RemoveFromGroup, Icon = Icons.GetIconSprite("remove") });
            if (CanHide(currentHideVolume)) mapMenu.AddItem(new MapMenu.ItemArgs {Title = "Hide Group", CloseMenuOnActivate = true, Action = HideGroup, Icon = sprite("show.png") });
            if (CanShow(currentHideVolume)) mapMenu.AddItem(new MapMenu.ItemArgs {Title = "Show Group", CloseMenuOnActivate = true, Action = ShowGroup, Icon = sprite("show.png") });
            if (InGroup(currentHideVolume)) mapMenu.AddItem(new MapMenu.ItemArgs {Title = "Use this Group", CloseMenuOnActivate = true, Action = CurrentGroup});
        }

        private static Sprite sprite(string FileName)
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Texture2D tex = new Texture2D(32, 32);
            tex.LoadImage(System.IO.File.ReadAllBytes(dir + "\\" + FileName));
            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }

        // Menu Actions
        private static void CreateGroup(MapMenuItem item, object o)
        {
            RemoveFromGroup(item, o);
            groups.Add((new List<HideVolumeItem> { currentHideVolume }, true));
            groupIndex = groups.Count - 1;
            SetGroup(item, o);
            SaveGroups();
        }
        private static void SetGroup(MapMenuItem item, object o)
        {
            RemoveFromGroup(item, o);
            if (groupIndex >= 0 && !groups[groupIndex].Item1.Contains(currentHideVolume))
            {
                groups[groupIndex].Item1.Add(currentHideVolume);
            }
            SaveGroups();
        }
        private static void RemoveFromGroup(MapMenuItem item, object o)
        {
            foreach (var group in groups)
            {
                if (group.Item1.Contains(currentHideVolume))
                {
                    group.Item1.Remove(currentHideVolume);
                }
            }

            groups.RemoveAll(g => g.Item1.Count == 0);
            SaveGroups();
        }
        private static void HideGroup(MapMenuItem item, object o)
        {
            var tempIndex = GetIndex();

            foreach (var volume in groups[tempIndex].Item1)
            {
                if (IsVisible(volume)) ToggleTiles(volume);
            }
            groups[tempIndex] = (groups[tempIndex].Item1, false);
            SaveGroups();
        }
        private static void ShowGroup(MapMenuItem item, object o)
        {
            var tempIndex = GetIndex();

            foreach (var volume in groups[tempIndex].Item1)
            {
                if (!IsVisible(volume)) ToggleTiles(volume);
            }

            groups[tempIndex] = (groups[tempIndex].Item1, true);
            SaveGroups();
        }
        private static void CurrentGroup(MapMenuItem item, object o)
        {
            foreach (var group in groups)
            {
                if (group.Item1.Contains(currentHideVolume))
                {
                    groupIndex = groups.IndexOf(group);
                }
            }
            SaveGroups();
        }

        // Checks
        private static bool StoreCurrentHideVolume(HideVolumeItem item)
        {
            currentHideVolume = item;
            //JsonConvert.SerializeObject(groups);

            var a = HideVolumeManager.Instance;
            var hideVolumes = a.transform.GetChild(1).Children();
            for (int i = 0; i < hideVolumes.LongCount(); i++)
            {
                var volume = a.transform.GetChild(1).GetChild(i);
                var volumeComponent = volume.GetComponent<HideVolumeItem>();

            }

            return true;
        }
        private static void HideAll()
        {
            var a = HideVolumeManager.Instance;
            var hideVolumes = a.transform.GetChild(1).Children();
            for (int i = 0; i < hideVolumes.LongCount(); i++)
            {
                var volume = a.transform.GetChild(1).GetChild(i);
                var volumeComponent = volume.GetComponent<HideVolumeItem>();
                HideFace(volumeComponent,true);
            }
        }
        private static bool CanHide(HideVolumeItem item)
        {
            var tempIndex = GetIndex();
            if (tempIndex == -1) return false;
            return groups[tempIndex].Item2;
        }
        private static bool CanShow(HideVolumeItem item)
        {
            if (GetIndex() == -1) return false;
            return !CanHide(item);
        }
        private static int GetIndex()
        {
            var tempIndex = -1;

            foreach (var group in groups)
            {
                if (group.Item1.Contains(currentHideVolume))
                {
                    tempIndex = groups.IndexOf(group);
                }
            }

            return tempIndex;
        }
        private static void HideFace(HideVolumeItem item, bool condition)
        {
            var volume = item.gameObject.transform;
            // Face
            var TaleVolume = volume.GetChild(0);
            TaleVolume.gameObject.SetActive(!condition);
            // Mesh mesh = tmeshFilter.mesh;
            // mesh.SetColors(new []{Color.red});
            // var tmeshRenderer = TaleVolume.GetComponent<MeshRenderer>();

            // Edges
            // var painter = volume.GetChild(7);
            // var pmeshFilter = painter.GetComponent<MeshFilter>();
            // var pmeshRenderer = painter.GetComponent<MeshRenderer>();
        }
        private static bool InGroup(HideVolumeItem item)
        {
            return groups.Any(g => g.Item1.Contains(item));
        }
        private static bool CanRemove(HideVolumeItem item) => InGroup(item);
        private static bool GroupSelected()
        {
            return groupIndex != -1;
        }

        private bool OnBoard()
        {
            return (CameraController.HasInstance &&
                    BoardSessionManager.HasInstance &&
                    BoardSessionManager.HasBoardAndIsInNominalState &&
                    !BoardSessionManager.IsLoading);
        }

        


        // Persistence
        private static void SaveGroups()
        {
            // Get All Hide Volumes and index them
            var a = HideVolumeManager.Instance;
            var hideVolumes = a.transform.GetChild(1).Children();
            var allVolumes = new List<HideVolumeItem>();
            for (int i = 0; i < hideVolumes.LongCount(); i++)
            {
                var volume = a.transform.GetChild(1).GetChild(i);
                var volumeComponent = volume.GetComponent<HideVolumeItem>();
                allVolumes.Add(volumeComponent);
            }

            // Convert HideVolumeItem to Index
            var x = groups.Select(g => g.Item1);
            List<List<int>> volumeToInt = new List<List<int>>();
            foreach (var group in x)
            {
                var indexes = new List<int>();
                foreach (var volume in group)
                {
                    indexes.Add(allVolumes.IndexOf(volume));
                }
                volumeToInt.Add(
                    indexes
                    );
            }
            
            var y = groups.Select(g => g.Item2).ToList();

            var tupled = new List<(List<int>, bool)>();
            for (int i = 0; i < volumeToInt.Count; i++)
            {
                tupled.Add((volumeToInt[i],y[i]));
            }

            HolloFoxes.BoardPersistence.SetInfo(Guid, JsonConvert.SerializeObject(tupled));
        }

        private static void LoadGroups()
        {
            // Get All Hide Volumes and index them
            var a = HideVolumeManager.Instance;
            var hideVolumes = a.transform.GetChild(1).Children();
            var allVolumes = new List<HideVolumeItem>();
            for (int i = 0; i < hideVolumes.LongCount(); i++)
            {
                var volume = a.transform.GetChild(1).GetChild(i);
                var volumeComponent = volume.GetComponent<HideVolumeItem>();
                allVolumes.Add(volumeComponent);
            }

            var result = HolloFoxes.BoardPersistence.ReadInfo(Guid);
            if (result == "") return;
            var actual = JsonConvert.DeserializeObject<List<(List<int>, bool)>>(result);

            // Convert Index to HideVolumes
            var x = actual.Select(g => g.Item1);
            List<List<HideVolumeItem>> intToVolume = new List<List<HideVolumeItem>>();
            foreach (var group in x)
            {
                var indexes = new List<HideVolumeItem>();
                foreach (var volume in group)
                {
                    Debug.Log($"Volume: {volume}");
                    indexes.Add(allVolumes[volume]);
                }
                intToVolume.Add(
                    indexes
                );
            }

            var y = actual.Select(g => g.Item2).ToList();

            var tupled = new List<(List<HideVolumeItem>, bool)>();
            for (int i = 0; i < intToVolume.Count; i++)
            {
                Debug.Log($"index {i}");
                tupled.Add((intToVolume[i], y[i]));
            }

            groups = tupled;
        }

        /// <summary>
        /// Looping method run by plugin
        /// </summary>
        void Update()
        {
            if (OnBoard())
            {
                if (!last)
                {
                    LoadGroups();
                }
                if (groupIndex != -1)
                {
                    // Hide all
                    HideAll();

                    // Un-hide set group
                    if (groupIndex != -1)
                        foreach (var volume in groups[groupIndex].Item1)
                            HideFace(volume, false);
                }
                last = true;
            }
            else
            {
                if (last)
                {
                    // Clear Groups
                    groupIndex = -1;
                    groups.Clear();
                }

                last = false;
            }
        }

        public static void ToggleTiles(HideVolumeItem volumeComponent)
        {
            var tool = new GMHideVolumeMenuBoardTool();
            tool.SetSelectedVolume(volumeComponent, new Vector3(0, 0, 0));

            Type classType = tool.GetType();

            MethodInfo mi = classType.GetMethod("ToggleTiles",
                BindingFlags.Instance | BindingFlags.NonPublic);

            mi.Invoke(tool, new object[] {null, null});
        }

        public static bool IsVisible(HideVolumeItem volumeComponent)
        {
            return volumeComponent.HideVolume.State == (byte)0 ? true : false;
        }
    }
}
