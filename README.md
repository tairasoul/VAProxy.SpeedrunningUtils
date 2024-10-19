# SpeedrunningUtils

Simple speedrunning utils for VA Proxy.

~~Needs [LiveSplit.Server.Modified](https://github.com/tairasoul/LiveSplit.Server.Modified) to create splits if they aren't there already.~~
Unnecessary functionality, removed.

Also needs [VAProxy.PageFramework](https://github.com/tairasoul/VAProxy.PageFramework) and [VAProxy.MainMenuSettings](https://github.com/tairasoul/VAProxy.MainMenuSettings) to integrate into the actual game UI for settings.

If you want some pre-made configs, go to [SpeedrunConfigs repo](https://github.com/tairasoul/VAProxy.SpeedrunConfigs)

## Mod overview

SpeedrunningUtils is just a bunch of utils to make speedrunners' lives easier.

This includes automatic splitting, OBS integration and quick resetting by keybind.

Automatic splitting is enabled with no option to disable, as it is central to the mod.

OBS integration is disabled by default, enable in BepInEx/config/tairasoul.vaproxy.speedrunning.cfg

Quick reset keybind is "P" by default, loads you into the main menu and clears the data of whatever slot you were playing on.

## Changelog

Changelog is at [SpeedrunningUtils/changelog.md](https://github.com/tairasoul/VAProxy.SpeedrunningUtils/blob/master/changelog.md)

## Setup for splits

Split files are located at BepInEx/plugins/SpeedrunningUtils.Splits

Each split is formatted like this:

```json5
{
  "SplitName": "name of split",
  "splitCondition": condition,
  "splitBounds": bounds,
  "Command": "startorsplit"
}
```

splitBounds and splitCondition, once fulfilled, will send the command (or move on to the next split's conditions if not decalred).

Command is the command to send. Do not put this key-value pair in if you don't want to send anything.

The commands you can send are found [here](https://github.com/LiveSplit/LiveSplit/blob/master/src/LiveSplit.Core/Server/CommandServer.cs#L155).

Each `case "string":` line represents a command. The text in quotes is the command.

Command defaults to not being anything.

### Condition formatting

A condition is formatted like this:

```json5
{
  "Name": "name of condition",
  "Path": "path",
  "Component": "ComponentName", // optional
  "Property": "property", // property to check
  "Value": "value" // value to compare property to, must always be double quoted.
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
