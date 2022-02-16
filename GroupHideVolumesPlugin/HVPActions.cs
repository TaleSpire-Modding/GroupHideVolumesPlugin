using System.Linq;
using BepInEx;
using LordAshes;
using RadialUI;

namespace GroupHideVolumes
{

    public partial class HideVolumesPlugin : BaseUnityPlugin
    {
        private static void CreateGroup(MapMenuItem item, object o)
        {
            currentHideVolume = RadialUIPlugin.GetLastRadialHideVolume();

            SystemMessage.AskForTextInput("Group Name", "Enter the name of the group",
                "OK", delegate (string groupName)
                {
                    AssetDataPlugin.SetInfo(currentHideVolume.HideVolume.Id.ToString(),Guid,groupName);
                }, delegate { }, "Cancel", null);
            SetGroup(item,o);
        }

        private static void SetGroup(MapMenuItem item, object o)
        {
            currentHideVolume = RadialUIPlugin.GetLastRadialHideVolume();
            if (currentGroup != null)
            {
                AssetDataPlugin.SetInfo(currentHideVolume.HideVolume.Id.ToString(), Guid, currentGroup);
            }
        }
        private static void RemoveFromGroup(MapMenuItem item, object o)
        {
            currentHideVolume = RadialUIPlugin.GetLastRadialHideVolume();
            AssetDataPlugin.SetInfo(currentHideVolume.HideVolume.Id.ToString(), Guid, string.Empty);
            HideFace(currentHideVolume, true);
        }

        private static void CurrentGroup(MapMenuItem arg1, object arg2)
        {
            currentHideVolume = RadialUIPlugin.GetLastRadialHideVolume();
            currentGroup = groups[currentHideVolume.HideVolume.Id];
            HideAll();
            currentHideVolume = RadialUIPlugin.GetLastRadialHideVolume();
            var group = groups[currentHideVolume.HideVolume.Id];
            var hiding = groups.Where(e => e.Value == group).Select(e => e.Key);
            foreach (var volumeId in hiding)
            {
                HideFace(findItem(volumeId),false);
            }
        }

        private static void HideGroup(MapMenuItem item, object o)
        {
            currentHideVolume = RadialUIPlugin.GetLastRadialHideVolume();
            var group = groups[currentHideVolume.HideVolume.Id];
            var hiding = groups.Where(e => e.Value == group).Select(e => e.Key);
            foreach (var volumeId in hiding)
            {
                if (IsVisible(volumeId)) ToggleTiles(volumeId);
            }
        }
        private static void ShowGroup(MapMenuItem item, object o)
        {
            currentHideVolume = RadialUIPlugin.GetLastRadialHideVolume();
            var group = groups[currentHideVolume.HideVolume.Id];
            var hiding = groups.Where(e => e.Value == group).Select(e => e.Key);
            foreach (var volumeId in hiding)
            {
                if (!IsVisible(volumeId)) ToggleTiles(volumeId);
            }
        }

    }
}
