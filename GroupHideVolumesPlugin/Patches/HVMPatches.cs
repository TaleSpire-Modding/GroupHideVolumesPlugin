using Bounce.ManagedCollections;
using DataModel;
using HarmonyLib;

namespace GroupHideVolumes.Patches
{
    [HarmonyPatch(typeof(HideVolumeManager), "OnHideVolumeAdded")]
    internal class HVMAddPatch
    {
        public static void Postfix(ref HideVolume volume, ref BList<HideVolumeItem> ____hideVolumeItems)
        {
            HideVolumesPlugin.hideVolumes = ____hideVolumeItems;
        }
    }

    [HarmonyPatch(typeof(HideVolumeManager), "OnHideVolumeRemoved")]
    internal class HVMRemovePatch
    {
        public static void Postfix(ref HideVolume volume, ref BList<HideVolumeItem> ____hideVolumeItems)
        {
            HideVolumesPlugin.hideVolumes = ____hideVolumeItems;
        }
    }
}
