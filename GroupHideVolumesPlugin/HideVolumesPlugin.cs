using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BepInEx;
using Bounce.ManagedCollections;
using Bounce.Unmanaged;
using LordAshes;
using RadialUI;
using Bounce.Singletons;
using HarmonyLib;

namespace GroupHideVolumes
{

    [BepInPlugin(Guid, "HolloFoxes' Group Hide Volumes Plug-In", Version)]
    [BepInDependency(RadialUIPlugin.Guid)]
    [BepInDependency(AssetDataPlugin.Guid)]
    // [BepInDependency(HVG.HideVolumeLabelsPlugin.Guid)]
    public partial class HideVolumesPlugin : BaseUnityPlugin
    {
        // constants
        public const string Guid = "org.hollofox.plugins.GroupHideVolumesPlugin";
        private const string Version = "2.0.0.0";

        // Key = HideVolume, value = name of group, Going to need complete re-write
        private static Dictionary<NGuid,string> groups = new Dictionary<NGuid, string>();
        internal static BList<HideVolumeItem> hideVolumes;

        private Sprite collectionSprite;
        private Sprite showSprite;
        
        public static HideVolumeItem currentHideVolume;
        private static string currentGroup;
        
        /// <summary>
        /// Awake plugin
        /// </summary>
        void Awake()
        {
            Logger.LogInfo("In Awake for HideVolumes");
            var harmony = new Harmony(Guid);
            harmony.PatchAll();
            ModdingTales.ModdingUtils.Initialize(this, Logger);

            showSprite = Icons.GetIconSprite("toggle_hide");
            collectionSprite = null; // FileAccessPlugin.Image.LoadSprite("/Images/Icons/collection.png");

            // Register Group Menus in a branch
            RadialSubmenu.EnsureMainMenuItem(RadialUIPlugin.Guid + ".HideVolume.Groups",
                RadialSubmenu.MenuType.HideVolume,
                "Grouping",
                collectionSprite
            );

            // Grouping branch
            RadialSubmenu.CreateSubMenuItem(RadialUIPlugin.Guid + ".HideVolume.Groups",
                new MapMenu.ItemArgs { Title = "Create Group", CloseMenuOnActivate = true, Action = CreateGroup, Icon = collectionSprite }
            );

            RadialSubmenu.CreateSubMenuItem(RadialUIPlugin.Guid + ".HideVolume.Groups",
                    new MapMenu.ItemArgs { Title = "Set Group", CloseMenuOnActivate = true, Action = SetGroup }
                    , null, GroupSelected);
            RadialSubmenu.CreateSubMenuItem(RadialUIPlugin.Guid + ".HideVolume.Groups",
                new MapMenu.ItemArgs { Title = "Remove from Group", CloseMenuOnActivate = true, Action = RemoveFromGroup, Icon = Icons.GetIconSprite("remove") }
                , null,CanRemove);
            RadialSubmenu.CreateSubMenuItem(RadialUIPlugin.Guid + ".HideVolume.Groups",
                new MapMenu.ItemArgs { Title = "Hide Group", CloseMenuOnActivate = true, Action = HideGroup, Icon = showSprite }
                , null,InGroup);
            RadialSubmenu.CreateSubMenuItem(RadialUIPlugin.Guid + ".HideVolume.Groups",
                new MapMenu.ItemArgs { Title = "Show Group", CloseMenuOnActivate = true, Action = ShowGroup, Icon = showSprite }
                , null, InGroup);
            RadialSubmenu.CreateSubMenuItem(RadialUIPlugin.Guid + ".HideVolume.Groups",
                new MapMenu.ItemArgs { Title = "Use this Group", CloseMenuOnActivate = true, Action = CurrentGroup }
                , null, InGroup);

            AssetDataPlugin.Subscribe($"{Guid}",DataChangeCallback);
        }


        private void DataChangeCallback(AssetDataPlugin.DatumChange obj)
        {
            try
            {
                var volume = new NGuid(obj.source);
                groups[volume] = (string) obj.value;
                if (string.IsNullOrWhiteSpace(currentGroup)) return;
                HideAll();
                currentHideVolume = RadialUIPlugin.GetLastRadialHideVolume();
                var group = groups[currentHideVolume.HideVolume.Id];
                var hiding = groups.Where(e => e.Value == @group).Select(e => e.Key);
                foreach (var volumeId in hiding)
                {
                    HideFace(findItem(volumeId), false);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        private static HideVolumeItem findItem(NGuid id)
        {
            HideVolumeItem item = null;
            var volumes = hideVolumes;
            foreach(var volume in volumes)
            {
                    if (volume.HideVolume.Id == id) item = volume;
            }
            return item;
        }

        public static void ToggleTiles(NGuid volumeComponent)
            => ToggleTiles(findItem(volumeComponent));

        public static bool IsVisible(NGuid volumeComponent)
            => IsVisible(findItem(volumeComponent));


        public static void ToggleTiles(HideVolumeItem volumeComponent)
        {
            volumeComponent.ChangeIsActive(!volumeComponent.HideVolume.IsActive);
            SimpleSingletonBehaviour<HideVolumeManager>.Instance.SetHideVolumeState(volumeComponent.HideVolume);
        }

        public static bool IsVisible(HideVolumeItem volumeComponent)
        {
            return !volumeComponent.HideVolume.IsActive;
        }
    }
}
