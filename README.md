# SpeedrunningUtils

Simple speedrunning utils for VA Proxy.

Also needs [VAProxy.MainMenuSettings](https://github.com/tairasoul/VAProxy.MainMenuSettings) to integrate into the actual game UI for settings.

If you want some pre-made configs, go to [SpeedrunConfigs repo](https://github.com/tairasoul/VAProxy.SpeedrunConfigs)

Splitting functionality is provided by [tairasoul.unity.common/speedrunning](https://github.com/tairasoul/tairasoul.unity.common/tree/main/speedrunning/)

## Mod overview

SpeedrunningUtils is just a bunch of utils to make speedrunners' lives easier.

This includes automatic splitting, OBS integration and quick resetting by keybind.

Automatic splitting is enabled with no option to disable, as it is central to the mod.

OBS integration is disabled by default, enable in BepInEx/config/tairasoul.vaproxy.speedrunning.cfg

Quick reset keybind is "P" by default, loads you into the main menu and clears the data of whatever slot you were playing on.

## Changelog

Changelog is at [SpeedrunningUtils/changelog.md](https://github.com/tairasoul/VAProxy.SpeedrunningUtils/blob/master/changelog.md)

## Split setup

Splits use a custom DSL for split creation, conditions and ordering.

Game-agnostic syntax reference is located at [tairasoul.unity.common/speedrunning/dsl/syntax.md](https://github.com/tairasoul/tairasoul.unity.common/blob/main/speedrunning/dsl/syntax.md), and runtime-specific syntax reference is as follows:

### Events

ItemPickup [name, amount, total]

Occurs on item pickup.

SceneChange [id]

Fired whenever scene is changed.

Index 1 is the menu, index 2 is the main game.