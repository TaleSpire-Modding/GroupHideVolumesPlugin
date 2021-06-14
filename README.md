# Group Hide Volumes Plugin

This is a plugin for TaleSpire using BepInEx.

## Install

Currently you need to either follow the build guide down below or use the R2ModMan.

## Usage
This plugin allows a user to group Hide Volumes and Toggle the visibility in mass.
Currently there's no way to save the groups. Right clicking on a hide volume will provide the extra options:
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
```

Build the project.

Browse to the newly created ```bin/Debug``` or ```bin/Release``` folders and copy the ```GroupHideVolumesPlugin.dll``` to ```Steam\steamapps\common\TaleSpire\BepInEx\plugins```

## Shoutouts
Shoutout to my Patreons on https://www.patreon.com/HolloFox recognising your
mighty contribution to my caffeine addiciton:
- John Fuller

## Bounty
This plugin is to complete the Group Hide Volumes Bounty outlined by Demongund.