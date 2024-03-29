# SpeedrunningUtils

Simple speedrunning utils for VA Proxy.

~~Needs [LiveSplit.Server.Modified](https://github.com/tairasoul/LiveSplit.Server.Modified) to create splits if they aren't there already.~~
No longer needs LiveSplit.Serever.Modified to function, but it is needed if you have ANY elements with addToLayout set to true.

Also needs [SettingsAPI.VAProxy](https://github.com/tairasoul/SettingsAPI.VAProxy) to integrate into the actual game UI for settings. It is included by default though.

If you want some pre-made configs, go to [SpeedrunConfigs repo](https://github.com/tairasoul/VAProxy.SpeedrunConfigs)

## Changelog

Changelog is at [SpeedrunningUtils/changelog.md](https://github.com/tairasoul/VAProxy.SpeedrunningUtils/blob/master/changelog.md)

## Setup for splits

Split files are located at BepInEx/plugins/SpeedrunningUtils.Splits

Each split is formatted like this:

```json5
{
  "SplitName": "name of split",
  "splitCondition": condition,
  "skipCondition": condition,
  "splitBounds": bounds,
  "skipBounds": bounds,
  "splitHere": "true", // or "false"
  "addToLayout": "true", // or "false"
}
```

skipBounds and skipCondition are optional, however you need either splitCondition or splitBounds.

skipBounds and skipCondition, once fulfilled, will skip that split if splitHere is true.

splitBounds and splitCondition, once fulfilled, will split.

splitHere means this split will end up sending startorsplit once requirements are met.

addToLayout means it will be added to your LiveSplit layout. This will replace whatever you have at that index with this split name.

addToLayout defaults to false, and splitHere defaults to true if you omit them.

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
