using System.Linq;
using BepInEx;
using RadialUI;

namespace GroupHideVolumes
{

    public partial class HideVolumesPlugin : BaseUnityPlugin
    {
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
                HideFace(volumeComponent, true);
            }
        }
        
        private static void HideFace(HideVolumeItem item, bool condition)
        {
            var volume = item.gameObject.transform;
            // Face
            var TaleVolume = volume.GetChild(0);
            TaleVolume.gameObject.SetActive(!condition);
        }
        
        private static bool InGroup()
        {
            currentHideVolume = RadialUIPlugin.GetLastRadialHideVolume();
            if (groups.ContainsKey(currentHideVolume.HideVolume.Id))
                return !string.IsNullOrWhiteSpace(groups[currentHideVolume.HideVolume.Id]);
            return false;
        }

        private static bool CanRemove()
            => InGroup();

        private static bool GroupSelected()
            => !string.IsNullOrWhiteSpace(currentGroup);
    }
}
