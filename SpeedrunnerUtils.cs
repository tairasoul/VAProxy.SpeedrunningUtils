﻿using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using System.Collections;
using BepInEx;
using System.Reflection;
using SettingsAPI;
using System.Collections.Generic;

namespace SpeedrunningUtils
{
    internal static class ObjectExtensions
    {
        internal static object GetFieldOrPropertyValue(this object obj, string fieldNameOrPropertyName)
        {
            if (obj == null)
            {
                return null;
            }
            try
            {
                Type objType = obj.GetType();
                // Attempt to get the field
                FieldInfo[] fields = objType.GetFields();
                foreach (FieldInfo field in fields)
                {
                    if (field.Name == fieldNameOrPropertyName)
                    {
                        return field.GetValue(obj);
                    }
                }

                // Attempt to get the property
                PropertyInfo[] properties = objType.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.Name == fieldNameOrPropertyName)
                    {
                        return property.GetValue(obj);
                    }
                }
                Plugin.Log.LogError($"Field or Property '{fieldNameOrPropertyName}' not found in {objType.FullName}.");
            }
            catch (Exception ex)
            {
                // If neither field nor property found
                Plugin.Log.LogError($"Error retrieving {fieldNameOrPropertyName}: {ex.Message}.");
                return null; // or throw the exception further
            }
            return null;
        }
    }
    internal class SpeedrunnerUtils : MonoBehaviour
    {
        private string CurrentScene = "Intro";
        private Vector3 ColliderStart;
        internal static CustomSplit[] splits = [];
        private int SplitIndex = 0;
        internal static Condition[] Conditions = [];
        internal static string SplitPath = Path.Combine(Paths.ConfigPath, "Splits.json");

        private void Awake()
        {
            SceneManager.activeSceneChanged += OnSceneChanged;
            Livesplit.StartSocket();
            SplitLoader.LoadSplits();
        }

        private void OnSceneChanged(Scene old, Scene newS)
        {
            CurrentScene = newS.name;
            if (CurrentScene != "Menu" && CurrentScene != "Intro")
            {
                Livesplit.SendCommand("reset\r\n");
            }
        }

        private void Update()
        {
            if (CurrentScene != "Intro" && CurrentScene != "Menu")
            {
                CustomSplit split = splits[SplitIndex];
                if (split.condition != null)
                {
                    bool fulfilled = split.condition.Fulfilled();
                    if (split.bounds != null)
                    {
                        if (fulfilled && split.bounds.Value.Contains(GameObject.Find("S-105").transform.position)) {
                            if (split.shouldSplitHere || split.finalSplit)
                            {
                                Plugin.Log.LogInfo("Splitting at split " + split.SplitName);
                                Livesplit.SendCommand("startorsplit\r\n");
                            }
                            SplitIndex++;
                        }
                    }
                    else
                    {
                        if (fulfilled)
                        {
                            if (split.shouldSplitHere || split.finalSplit)
                            {
                                Plugin.Log.LogInfo("Splitting at split " + split.SplitName);
                                Livesplit.SendCommand("startorsplit\r\n");
                            }
                            SplitIndex++;
                        }
                    }
                }
                else
                {
                    if (split.bounds.Value.Contains(GameObject.Find("S-105").transform.position))
                    {
                        if (split.shouldSplitHere || split.finalSplit)
                        {
                            Plugin.Log.LogInfo("Splitting at split " + split.SplitName);
                            Livesplit.SendCommand("startorsplit\r\n");
                        }
                        SplitIndex++;
                    }
                }
            }
        }
    }

    internal class Condition
    {
        internal string Name;
        internal string Path;
        internal string Property;
        internal string Value;
        internal string ValueType;
        internal string Comparison;
        internal string? Component;
        private T ParseValue<T>(object value)
        {
            // Parse based on the type T
            if (typeof(T) == typeof(int))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (typeof(T) == typeof(float))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (typeof(T) == typeof(bool))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else
            {
                throw new NotSupportedException($"Type {typeof(T)} not supported for parsing.");
            }
        }

        internal bool Fulfilled()
        {
            if (Component != null)
            {
                try
                {
                    string[] splits = Path.Split("/".ToCharArray());
                    GameObject obj = GameObject.Find(splits[0]);
                    for (int i = 1; i < splits.Length; i++)
                    {
                        obj = obj.Find(splits[i]);
                    }
                    Component component = obj.GetComponent(Component);
                    object parsed = "";
                    Type type = typeof(int);
                    switch (ValueType)
                    {
                        case "float":
                            parsed = ParseValue<float>(Value);
                            type = typeof(float);
                            break;
                        case "int":
                            parsed = ParseValue<int>(Value);
                            type = typeof(int);
                            break;
                        case "bool":
                            parsed = ParseValue<bool>(Value);
                            type = typeof(bool);
                            break;
                        case "string":
                            parsed = ParseValue<string>(Value);
                            type = typeof(string);
                            break;

                    }
                    object fieldValue = null;
                    try
                    {
                        Type objType = component.GetType();
                        // Attempt to get the field
                        FieldInfo[] fields = objType.GetFields();
                        foreach (FieldInfo field in fields)
                        {
                            if (field.Name == Property)
                            {
                                fieldValue = field.GetValue(obj);
                            }
                        }

                        // Attempt to get the property
                        PropertyInfo[] properties = objType.GetProperties();
                        foreach (PropertyInfo property in properties)
                        {
                            if (property.Name == Property)
                            {
                                fieldValue = property.GetValue(obj);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.LogError(ex);
                    }

                    if (fieldValue != null && fieldValue.GetType() == type)
                    {
                        object convertedParsed = Convert.ChangeType(parsed, type);

#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                        return Comparison switch
                        {
                            "==" => fieldValue.Equals(convertedParsed),
                            "<" => Comparer.Default.Compare(fieldValue, convertedParsed) < 0,
                            ">" => Comparer.Default.Compare(fieldValue, convertedParsed) > 0,
                            ">=" => Comparer.Default.Compare(fieldValue, convertedParsed) >= 0,
                            "<=" => Comparer.Default.Compare(fieldValue, convertedParsed) <= 0
                        };
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                    }
                }
                catch { }
                return false;
            }
            else
            {
                try
                {
                    string[] splits = Path.Split("/".ToCharArray());
                    GameObject obj = GameObject.Find(splits[0]);
                    for (int i = 1; i < splits.Length; i++)
                    {
                        obj = obj.Find(splits[i]);
                    }
                    object parsed = "";
                    Type type = typeof(int);
                    switch (ValueType)
                    {
                        case "float":
                            parsed = ParseValue<float>(Value);
                            type = typeof(float);
                            break;
                        case "int":
                            parsed = ParseValue<int>(Value);
                            type = typeof(int);
                            break;
                        case "bool":
                            parsed = ParseValue<bool>(Value);
                            type = typeof(bool);
                            break;
                        case "string":
                            parsed = ParseValue<string>(Value);
                            type = typeof(string);
                            break;
                    }
                    //PropertyInfo info = obj.GetType().GetProperty(Property);
                    //object fieldValue = info.GetValue(obj);
                    object fieldValue = null;
                    try
                    {
                        Type objType = obj.GetType();
                        // Attempt to get the field
                        FieldInfo[] fields = objType.GetFields();
                        foreach (FieldInfo field in fields)
                        {
                            if (field.Name == Property)
                            {
                                fieldValue = field.GetValue(obj);
                            }
                        }

                        // Attempt to get the property
                        PropertyInfo[] properties = objType.GetProperties();
                        foreach (PropertyInfo property in properties)
                        {
                            if (property.Name == Property)
                            {
                                fieldValue = property.GetValue(obj);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.LogError(ex);
                    }
                    if (fieldValue != null && fieldValue.GetType() == type)
                    {
                        object convertedParsed = Convert.ChangeType(parsed, type);

#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                        return Comparison switch
                        {
                            "==" => fieldValue.Equals(convertedParsed),
                            "<" => Comparer.Default.Compare(fieldValue, convertedParsed) < 0,
                            ">" => Comparer.Default.Compare(fieldValue, convertedParsed) > 0,
                            ">=" => Comparer.Default.Compare(fieldValue, convertedParsed) >= 0,
                            "<=" => Comparer.Default.Compare(fieldValue, convertedParsed) <= 0
                        };
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                    }
                }
                catch { }
                return false;
            }
        }
    }

    internal class SplitLoader
    {
        internal static Condition ParseCondition(JToken condition)
        {
            if (condition == null) return null;
            string comp = null;
            try
            {
                comp = (string)condition["Component"];
            }
            catch { }
            Condition cond = new()
            {
                Name = (string)condition["Name"],
                Path = (string)condition["Path"],
                Component = comp,
                Property = (string)condition["Property"],
                Value = (string)condition["Value"],
                ValueType = (string)condition["ValueType"],
                Comparison = (string)condition["Comparison"]
            };
            return cond;
        }

        internal static Condition[] ParseConditionArray(JArray Conditions)
        {
            Condition[] conditions = [];
            foreach (JToken cond in Conditions)
            {
                conditions = [.. conditions, ParseCondition(cond)];
            }
            return conditions;
        }

        internal static Vector3 ParseVector(string vector)
        {
            string[] parts = vector.Split(char.Parse(" "));
            return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
        }

        internal static Bounds? ParseBounds(JToken collider)
        {
            if (collider == null) return null;
            Bounds coll = new(ParseVector((string)collider["center"]), ParseVector((string)collider["size"]));
            Plugin.Visualiser.AddBounds(coll);
            return coll;
        }

        internal static string ParseBoundsToJson(Bounds bounds)
        {
            return $"{{ \"size\": \"{bounds.size.x} {bounds.size.y} {bounds.size.z}\", \"center\": \"{bounds.center.x} {bounds.center.y} {bounds.center.z}\"}}";
        }
        
        internal static string ParseConditionToJson(Condition condition)
        {
            return $"{{ \"Name\": \"{condition.Name}\", \"Path\": \"{condition.Path}\", \"Property\": \"{condition.Property}\", \"Value\": \"{condition.Value}\", \"ValueType\": \"{condition.ValueType}\", \"Comparison\": \"{condition.Comparison}\"";
        }
        internal static void LoadSplits()
        {
            if (!File.Exists(SpeedrunnerUtils.SplitPath))
            {
                string baseData = @"[
                {
                    ""SplitName"": ""PreStart Split"",
                    ""condition"": {
                        ""Name"": ""WasCutsceneActive"",
                        ""Path"": ""Director/Cutscene4"",
                        ""Property"": ""activeSelf"",
                        ""Value"": ""true"",
                        ""ValueType"": ""bool"",
                        ""Comparison"": ""==""
                    },
                    ""shouldSplitHere"": ""false""
                },
                {
                    ""SplitName"": ""Spawn"",
                    ""condition"": {
                        ""Name"": ""CutsceneNotActive"",
                        ""Path"": ""Director/Cutscene4"",
                        ""Property"": ""activeSelf"",
                        ""Value"": ""false"",
                        ""ValueType"": ""bool"",
                        ""Comparison"": ""==""
                    },
                    ""shouldSplitHere"": ""true""
                },
                {
                    ""SplitName"": ""DropCutscene-Pre"",
                    ""condition"": {
                        ""Name"": ""WasCutsceneActive"",
                        ""Path"": ""Director/Cutscene6"",
                        ""Property"": ""activeSelf"",
                        ""Value"": ""true"",
                        ""ValueType"": ""bool"",
                        ""Comparison"": ""==""
                    },
                    ""shouldSplitHere"": ""false""
                },
                {
                    ""SplitName"": ""Drop"",
                    ""condition"": {
                        ""Name"": ""CutsceneNotActive"",
                        ""Path"": ""Director/Cutscene6"",
                        ""Property"": ""activeSelf"",
                        ""Value"": ""false"",
                        ""ValueType"": ""bool"",
                        ""Comparison"": ""==""
                    },
                    ""shouldSplitHere"": ""true""
                },
                {
                    ""SplitName"": ""Scrap Pits"",
                    ""bounds"": {
                        ""center"": ""4049.9 10501.425 374.2478"",
                        ""size"": ""25.748 2 21.6042""
                    },
                    ""shouldSplitHere"": ""true""
                }
            ]";

                // Create or overwrite the file with the base data
                using StreamWriter streamWriter = new(SpeedrunnerUtils.SplitPath);
                streamWriter.Write(baseData);
            }

            string json = File.ReadAllText(SpeedrunnerUtils.SplitPath);

            JArray splits = JArray.Parse(json);
            int actualSplitIndex = 0;
            for (int i = 0; i < splits.Count; i++)
            {
                try
                {
                    var split = splits[i];
                    Plugin.Log.LogInfo($"Loading split {(string)split["SplitName"]}");
                    bool isFinalSplit = false;
                    try
                    {
                        isFinalSplit = (string)split["isFinalSplit"] == "true";
                    }
                    catch { }
                    CustomSplit spl = new()
                    {
                        SplitName = (string)split["SplitName"],
                        condition = ParseCondition(split["condition"]),
                        bounds = ParseBounds(split["bounds"]),
                        shouldSplitHere = (string)split["shouldSplitHere"] == "true",
                        finalSplit = isFinalSplit
                    };
                    SpeedrunnerUtils.splits = [.. SpeedrunnerUtils.splits, spl];
                    if (spl.shouldSplitHere && !isFinalSplit)
                    {
                        Livesplit.SendCommand($"setsplitname {actualSplitIndex} {spl.SplitName}\r\n");
                        actualSplitIndex++;
                    }
                    Plugin.Log.LogInfo($"Loaded split {(string)split["SplitName"]}");
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError(e);
                }
            }
        }

        internal static void SaveSplits()
        {
            string[] data = [];
            foreach (var split in SpeedrunnerUtils.splits)
            {
                string parsed = $"{{\"SplitName\": \"${split.SplitName}\", \"condition\": {ParseConditionToJson(split.condition)}, \"bounds\": {ParseBoundsToJson(split.bounds.Value)}}}";
                data = [.. data, parsed];
            }
            using StreamWriter streamWriter = new(SpeedrunnerUtils.SplitPath);
            streamWriter.Write(data);
        }
    }

    internal class CustomSplit
    {
        internal Bounds? bounds;
        internal Condition? condition;
        internal string SplitName;
        internal bool shouldSplitHere;
        internal bool finalSplit;
    }
}