# SpeedrunningUtils

Simple speedrunning utils for VA Proxy.

Needs [LiveSplit.Server.Modded](https://github.com/tairasoul/LiveSplit.Server.Modified) to create splits if they aren't there already.

Also needs [SettingsAPI.VAProxy](https://github.com/tairasoul/SettingsAPI.VAProxy) to integrate into the actual game UI for settings. It is included by default though.

I'd recommend clearing all your splits when you load up a new config or modify existing config, as it renames splits based on the splits marked as shouldSplitHere in Splits.json

If you want some pre-made configs, go to [SpeedrunConfigs repo](https://github.com/tairasoul/VAProxy.SpeedrunConfigs)

Functionality planned to be finished:

A quick-restart keybind that starts you back at the very beginning

## Setup for splits

Splits are located at BepInEx/config/Splits.json

Each split is formatted like this:

```json5
{
  "SplitName": "name of split",
  "condition": condition,
  "bounds": bounds,
  "shouldSplitHere": "true", // or "false"
  "isFinalSplit": "true", // or "false"
}
```

Condition and bounds is optional, but atleast one of them is required.

shouldSplitHere adds it to the splits you have, and will split once the conditions are met or the player is in the bounds.

isFinalSplit means it won't be added but will split once conditions are met or player is in bounds. This is intended to be used for your last split.

### Condition formatting

A condition is formatted like this:

```json5
{
  "Name": "name of condition",
  "Path": "path",
  "Component": "ComponentName", // optional
  "Property": "property", // property to check
  "Value": "value" // value to compare property to, must always be double quoted.
  "ValueType": "bool", // type of value. can be string, int, bool or float.
  "Comparison": "==" // valid comparisons are == < > >= and <=
}
```

### Bounds formatting

Bounds are formatted like this:

```json5
{
  "center": "x y z", // Something like "4049.9 10501.425 374.2478", the center of the bounds.
  "size": "x y z" // Something like "25.748 4 21.6042", the size of the bounds.
}
```

### Default splits.json

The default splits.json is
```json5
[
    {
        "SplitName": "PreStart Split",
        "condition": {
            "Name": "WasCutsceneActive",
            "Path": "Director/Cutscene4",
            "Property": "activeSelf",
            "Value": "true",
            "ValueType": "bool",
            "Comparison": "=="
        },
        "shouldSplitHere": "false"
    },
    {
        "SplitName": "Spawn",
        "condition": {
            "Name": "CutsceneNotActive",
            "Path": "Director/Cutscene4",
            "Property": "activeSelf",
            "Value": "false",
            "ValueType": "bool",
            "Comparison": "=="
        },
        "shouldSplitHere": "true"
    },
    {
        "SplitName": "DropCutscene-Pre",
        "condition": {
            "Name": "WasCutsceneActive",
            "Path": "Director/Cutscene6",
            "Property": "activeSelf",
            "Value": "true",
            "ValueType": "bool",
            "Comparison": "=="
        },
        "shouldSplitHere": "false"
    },
    {
        "SplitName": "Drop",
        "condition": {
            "Name": "CutsceneNotActive",
            "Path": "Director/Cutscene6",
            "Property": "activeSelf",
            "Value": "false",
            "ValueType": "bool",
            "Comparison": "=="
        },
        "shouldSplitHere": "true"
    },
    {
        "SplitName": "Scrap Pits",
        "bounds": {
            "center": "4049.9 10501.425 374.2478",
            "size": "25.748 4 21.6042"
        },
        "shouldSplitHere": "true"
    }
]
```
