# Group Hide Volumes Plugin

This is a plugin for TaleSpire using BepInEx.

## Install

Currently you need to either follow the build guide down below or use the R2ModMan.

## Usage
This plugin allows a user to group Hide Volumes and Toggle the visibility in mass.
Right clicking on a hide volume will provide the extra options:
- Create Group
- Set Group (adds hide volume to group)
- Remove from Group (removes volume from its group)
- Show/Hide Group (mass show/hide)
- Use this group (change to the group of selected hide volume)

## How to Compile / Modify

Open ```GroupHideVolumesPlugin.sln``` in Visual Studio.

You will need to add references to:

```
* BepInEx.dll  (Download from the BepInEx project.)
* Bouncyrock.TaleSpire.Runtime (found in Steam\steamapps\common\TaleSpire\TaleSpire_Data\Managed)
* UnityEngine.dll
* UnityEngine.CoreModule.dll
* UnityEngine.InputLegacyModule.dll 
* UnityEngine.UI
* Unity.TextMeshPro
* RadialUI.dll
* HideVolumeLabelsPlugin.dll
```

Build the project.

Browse to the newly created ```bin/Debug``` or ```bin/Release``` folders and copy the ```GroupHideVolumesPlugin.dll``` to ```Steam\steamapps\common\TaleSpire\BepInEx\plugins```

## Changelog
1.2.1: Fix ReadMe
1.2.0: Update HVG and fix bugs from update. Group labels now work (careful there is a data limit for now)
1.1.4: Update method to fetch sprite for robustness
1.1.3: Remove debug logging not needed
1.1.2: Fix bepin dependency on BoardPersistence Plugin
1.1.1: Bug Fix
1.1.0: Added Persistence to the mod to allow groups to be saved and shared.
1.0.1: Added Icons and Submenu to Radial Menu
1.0.0: Initial release

## Shoutouts
Shoutout to my Patreons on https://www.patreon.com/HolloFox recognising your
mighty contribution to my caffeine addiciton:
- John Fuller
- [Tales Tavern](https://talestavern.com/) - MadWizard

## Bounty
This plugin is to complete the Group Hide Volumes Bounty outlined by Demongund.