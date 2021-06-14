using System;
using System.Collections.Generic;
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
        private const string Version = "1.0.0.0";

        private static List<(List<HideVolumeItem>,bool)> groups =
            new List<(List<HideVolumeItem>, bool)>();
        
        public static HideVolumeItem currentHideVolume;
        public static int groupIndex = -1;

        /// <summary>
        /// Awake plugin
        /// </summary>
        void Awake()
        {
            Logger.LogInfo("In Awake for HideVolumes");

            Debug.Log("HideVolumes Plug-in loaded");
            // Load PUP
            ModdingTales.ModdingUtils.Initialize(this, Logger);

            RadialUIPlugin.AddOnHideVolume(
                Guid + "SetGroup",
                new MapMenu.ItemArgs
                {
                    Title = "Set Group",
                    CloseMenuOnActivate = true,
                    Action = SetGroup
                }, SetCurrentHideVolume
                );

            RadialUIPlugin.AddOnHideVolume(
                Guid + "RemoveFromGroup",
                new MapMenu.ItemArgs
                {
                    Title = "Remove from Group",
                    CloseMenuOnActivate = true,
                    Action = RemoveFromGroup
                }, CanRemove
            );

            RadialUIPlugin.AddOnHideVolume(
                Guid + "HideGroup",
                new MapMenu.ItemArgs
                {
                    Title = "Hide Group",
                    CloseMenuOnActivate = true,
                    Action = HideGroup
                }, CanHide
            );

            RadialUIPlugin.AddOnHideVolume(
                Guid + "ShowGroup",
                new MapMenu.ItemArgs
                {
                    Title = "Show Group",
                    CloseMenuOnActivate = true,
                    Action = ShowGroup
                }, CanShow
            );

            RadialUIPlugin.AddOnHideVolume(
                Guid + "CreateGroup",
                new MapMenu.ItemArgs
                {
                    Title = "Create Group",
                    CloseMenuOnActivate = true,
                    Action = CreateGroup
                }
            );

            RadialUIPlugin.AddOnHideVolume(
                Guid + "CurrentGroup",
                new MapMenu.ItemArgs
                {
                    Title = "Use this Group",
                    CloseMenuOnActivate = true,
                    Action = CurrentGroup
                }, InGroup
            );
        }

        private static void CreateGroup(MapMenuItem item, object o)
        {
            RemoveFromGroup(item,o);
            groups.Add( (new List<HideVolumeItem>{currentHideVolume}, true));
            groupIndex = groups.Count - 1;
            SetGroup(item,o);
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
        }


        private static void SetGroup(MapMenuItem item, object o)
        {
            if (groupIndex >= 0 && !groups[groupIndex].Item1.Contains(currentHideVolume))
            {
                groups[groupIndex].Item1.Add(currentHideVolume);
            }
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
        }


        private static void HideGroup(MapMenuItem item, object o)
        {
            var tempIndex = GetIndex();

            foreach (var volume in groups[tempIndex].Item1)
            {
                if (IsVisible(volume)) ToggleTiles(volume);
            }
            groups[tempIndex] = (groups[tempIndex].Item1, false);
        }

        private static void ShowGroup(MapMenuItem item, object o)
        {
            var tempIndex = GetIndex();

            foreach (var volume in groups[tempIndex].Item1)
            {
                if (!IsVisible(volume)) ToggleTiles(volume);
            }

            groups[tempIndex] = (groups[tempIndex].Item1, true);
        }

        private static bool CanHide(HideVolumeItem item)
        {
            var tempIndex = GetIndex();
            if (tempIndex == -1) return false;
            return groups[tempIndex].Item2;
        }

        private static bool CanShow(HideVolumeItem item)
        {
            var tempIndex = GetIndex();
            if (tempIndex == -1) return false;
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


        private static bool SetCurrentHideVolume(HideVolumeItem item)
        {
            currentHideVolume = item;
            return groupIndex != -1;
        }

        private bool OnBoard()
        {
            return (CameraController.HasInstance &&
                    BoardSessionManager.HasInstance &&
                    BoardSessionManager.HasBoardAndIsInNominalState &&
                    !BoardSessionManager.IsLoading);
        }

        private bool last = false;

        /// <summary>
        /// Looping method run by plugin
        /// </summary>
        void Update()
        {
            if (OnBoard())
            {
                if (!last)
                {
                    // Load Groups
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
                }
                last = false;
            }
        }

        private void Probe()
        {
            /*CampaignSessionManager.SetCreatureStatNames(new[]
            {
                "012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789",
                "1",
                "2",
                "3"
            });*/
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
