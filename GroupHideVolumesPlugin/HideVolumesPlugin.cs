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

    [BepInPlugin(Guid, "HolloFoxes' Group Hide Volumes Plug-In", Version)]
    [BepInDependency(RadialUIPlugin.Guid)]
    [BepInDependency(HolloFoxes.BoardPersistence.Guid)]
    [BepInDependency(HideVolumeLabelsPlugin.HideVolumeLabelsPlugin.Guid)]
    public class HideVolumesPlugin : BaseUnityPlugin
    {
        // constants
        public const string Guid = "org.hollofox.plugins.GroupHideVolumesPlugin";
        public const string G = "G";
        private const string Version = "1.2.0.0";

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

            Debug.Log("GroupHideVolumes Plug-in loaded");
            
            ModdingTales.ModdingUtils.Initialize(this, Logger);

            // Register Group Menus in a branch
            RadialSubmenu.EnsureMainMenuItem(RadialUIPlugin.Guid + ".HideVolume.Groups",
                RadialSubmenu.MenuType.HideVolume,
                "Grouping",
                sprite("collection.png")
                // StoreCurrentHideVolume
            );

            RadialSubmenu.EnsureMainMenuItem(RadialUIPlugin.Guid + ".HideVolume.Labels",
                RadialSubmenu.MenuType.HideVolume,
                "Grouping",
                sprite("collection.png")
                // StoreCurrentHideVolume
            );


            // Grouping branch
            RadialSubmenu.CreateSubMenuItem(RadialUIPlugin.Guid + ".HideVolume.Groups",
                new MapMenu.ItemArgs { Title = "Create Group", CloseMenuOnActivate = true, Action = CreateGroup, Icon = Icons.GetIconSprite("dungeonmaster") }
            );
            RadialSubmenu.CreateSubMenuItem(RadialUIPlugin.Guid + ".HideVolume.Groups",
                    new MapMenu.ItemArgs { Title = "Set Group", CloseMenuOnActivate = true, Action = SetGroup }
                    , null, GroupSelected);
            RadialSubmenu.CreateSubMenuItem(RadialUIPlugin.Guid + ".HideVolume.Groups",
                new MapMenu.ItemArgs { Title = "Remove from Group", CloseMenuOnActivate = true, Action = RemoveFromGroup, Icon = Icons.GetIconSprite("remove") }
                , null,CanRemove);
            RadialSubmenu.CreateSubMenuItem(RadialUIPlugin.Guid + ".HideVolume.Groups",
                new MapMenu.ItemArgs { Title = "Hide Group", CloseMenuOnActivate = true, Action = HideGroup, Icon = sprite("show.png") }
                , null,CanHide);
            RadialSubmenu.CreateSubMenuItem(RadialUIPlugin.Guid + ".HideVolume.Groups",
                new MapMenu.ItemArgs { Title = "Show Group", CloseMenuOnActivate = true, Action = ShowGroup, Icon = sprite("show.png") }
                , null,CanShow);
            RadialSubmenu.CreateSubMenuItem(RadialUIPlugin.Guid + ".HideVolume.Groups",
                new MapMenu.ItemArgs { Title = "Use this Group", CloseMenuOnActivate = true, Action = CurrentGroup }
                , null,InGroup);

            // Labeling Branch
            RadialSubmenu.CreateSubMenuItem(RadialUIPlugin.Guid + ".HideVolume.Labels",
                "Set Group Name",
                sprite("Edit.png"),
                SetLabelName
            );
        }

        private static string Labelholder;
        private static bool processing;

        private void SetLabelName(HideVolumeItem hideVolume, string guid, MapMenuItem item)
        {
            SystemMessage.AskForTextInput("Set Hide Volume Label", "", "Set Label", SetLabel, ProcessLabel);
        }
        private void SetLabel(string input)
        {
            Labelholder = input;
            processing = true;
            ProcessLabel();
        }

        private void ProcessLabel()
        {
            // Foreach label in group, set label
            if (processing)
            {
                var lastVolume = RadialUIPlugin.GetLastRadialHideVolume();
                HideVolumeLabelsPlugin.HideVolumeLabelsPlugin.SetLabel(lastVolume, G, Labelholder);
                processing = false;
            }
        }

        private static Sprite sprite(string FileName)
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = dir + "\\" + FileName;
            return RadialSubmenu.GetIconFromFile(path);
        }

        // Menu Actions
        private static void CreateGroup(MapMenuItem item, object o)
        {
            currentHideVolume = RadialUIPlugin.GetLastRadialHideVolume();
            RemoveFromGroup(item, o);
            groups.Add((new List<HideVolumeItem> { currentHideVolume }, true));
            groupIndex = groups.Count - 1;
            SetGroup(item,o);
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

            groupIndex = GetIndex();
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

        // checks
        private static void HideAll()
        {
            currentHideVolume = RadialUIPlugin.GetLastRadialHideVolume();
            var a = HideVolumeManager.Instance;
            var hideVolumes = a.transform.GetChild(1).Children();
            for (int i = 0; i < hideVolumes.LongCount(); i++)
            {
                var volume = a.transform.GetChild(1).GetChild(i);
                var volumeComponent = volume.GetComponent<HideVolumeItem>();
                HideFace(volumeComponent,true);
            }
        }
        private static bool CanHide()
        {
            var tempIndex = GetIndex();
            if (tempIndex == -1) return false;
            return groups[tempIndex].Item2;
        }
        private static bool CanShow()
        {
            if (GetIndex() == -1) return false;
            return !CanHide();
        }
        private static int GetIndex()
        {
            currentHideVolume = RadialUIPlugin.GetLastRadialHideVolume();
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
            currentHideVolume = RadialUIPlugin.GetLastRadialHideVolume();
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
        private static bool InGroup()
        {
            currentHideVolume = RadialUIPlugin.GetLastRadialHideVolume();
            return groups.Any(g => g.Item1.Contains(RadialUIPlugin.GetLastRadialHideVolume()));
        }
        private static bool CanRemove() => InGroup();
        private static bool GroupSelected()
        {
            currentHideVolume = RadialUIPlugin.GetLastRadialHideVolume();
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

            var tupled = new List<dto>();
            for (int i = 0; i < volumeToInt.Count; i++)
            {
                var bo = y[i] ? 1: 0;
                tupled.Add(new dto{I = volumeToInt[i], B = bo});
            }

            HolloFoxes.BoardPersistence.SetInfo(G, JsonConvert.SerializeObject(tupled));
        }

        private static bool LoadGroups()
        {
            try
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

                var result = HolloFoxes.BoardPersistence.ReadInfo(G);
                if (result == "") return true;
                var actual = JsonConvert.DeserializeObject<List<dto>>(result);

                // Convert Index to HideVolumes
                var x = actual.Select(g => g.I);
                List<List<HideVolumeItem>> intToVolume = new List<List<HideVolumeItem>>();
                foreach (var group in x)
                {
                    var indexes = new List<HideVolumeItem>();
                    foreach (var volume in group)
                    {
                        indexes.Add(allVolumes[volume]);
                    }

                    intToVolume.Add(
                        indexes
                    );
                }

                var y = actual.Select(g => g.B).ToList();

                var tupled = new List<(List<HideVolumeItem>, bool )>();
                for (int i = 0; i < intToVolume.Count; i++)
                {
                    var bo = y[i] == 1 ? true : false;
                    tupled.Add((intToVolume[i], bo));
                }

                groups = tupled;
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
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
                    if (!LoadGroups()) return;
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
