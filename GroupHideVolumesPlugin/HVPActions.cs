using System.Collections.Generic;
using BepInEx;
using RadialUI;

namespace GroupHideVolumes
{

    public partial class HideVolumesPlugin : BaseUnityPlugin
    {

        // Menu Actions
        private static void CreateGroup(MapMenuItem item, object o)
        {
            currentHideVolume = RadialUIPlugin.GetLastRadialHideVolume();
            RemoveFromGroup(item, o);
            groups.Add((new List<HideVolumeItem> { currentHideVolume }, true));
            groupIndex = groups.Count - 1;
            SetGroup(item, o);
            SaveGroups();
        }
        private static void SetGroup(MapMenuItem item, object o)
        {
            Remove();
            if (groupIndex >= 0 && !groups[groupIndex].Item1.Contains(currentHideVolume))
            {
                groups[groupIndex].Item1.Add(currentHideVolume);
            }

            Cleanup();
            groupIndex = GetIndex();
            SaveGroups();
        }
        private static void RemoveFromGroup(MapMenuItem item, object o)
        {
            Remove();
            Cleanup();
            HideFace(RadialUIPlugin.GetLastRadialHideVolume(), true);
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
            groupIndex = GetIndex();
            SaveGroups();
        }

        // Methods
        private static void Cleanup()
        {
            for (var i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                if (group.Item1.Count == 0)
                {
                    groups.RemoveAt(i);
                    i -= 1;
                }
            }
            SaveGroups();
        }

        private static void Remove()
        {
            foreach (var group in groups)
            {
                if (group.Item1.Contains(currentHideVolume))
                {
                    group.Item1.Remove(currentHideVolume);
                }
            }
        }
    }
}
