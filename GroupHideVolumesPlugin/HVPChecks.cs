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

        private static bool GroupSelected()
        {
            currentHideVolume = RadialUIPlugin.GetLastRadialHideVolume();
            return groupIndex != -1;
        }

        private static bool CanRemove() => InGroup();
    }
}
