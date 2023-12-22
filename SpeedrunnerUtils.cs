using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using System.Collections;
using BepInEx;
using System.Reflection;

namespace SpeedrunningUtils
{
    internal class SpeedrunnerUtils : MonoBehaviour
    {
        private string CurrentScene = "Intro";
        private bool Cutscene4WasActive = false;
        private bool TimerStarted = false;
        private bool Cutscene6WasActive = false;
        private BoxCollider[] Colliders = [];
        private Vector3 ColliderStart;
        public static CustomSplit[] splits = [];
        private int SplitIndex = 0;
        private bool Cutscene6Split = false;
        public static Condition[] Conditions = Array.Empty<Condition>();
        public static string SplitPath = Path.Combine(Paths.ConfigPath, "Splits.json");

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

        private void StartAddingCollider()
        {
            GameObject Sen = GameObject.Find("S-105");
            ColliderStart = Sen.transform.position;
            GameObject Collider = new($"Collider {Colliders.Length + 1}");
            Collider.AddComponent<VisualiserComponent>();
            Collider.AddComponent<BoxCollider>();
        }

        private IEnumerator ColliderUpdate(GameObject coll)
        {
            BoxCollider collider = coll.GetComponent<BoxCollider>();
            while (true)
            {
                if (UnityInput.Current.GetKeyDown(KeyCode.F8)) break;
                yield return null;
            }
        }

        private void Update()
        {
            if (CurrentScene != "Intro" && CurrentScene != "Menu")
            {
                /*GameObject Director = GameObject.Find("Director");
                GameObject Cutscene4 = Director?.transform.Find("Cutscene4")?.gameObject;
                if (Cutscene4 != null)
                {
                    if (Cutscene4.activeSelf) Cutscene4WasActive = true;
                    if (!Cutscene4.activeSelf && Cutscene4WasActive && !TimerStarted)
                    {
                        Plugin.Log.LogInfo("Starting LiveSplit timer.");
                        TimerStarted = true;
                        Livesplit.SendCommand("starttimer\r\n");
                    }
                }
                GameObject Cutscene6 = Director?.transform.Find("Cutscene6")?.gameObject;
                if (Cutscene6 != null)
                {
                    if (Cutscene6.activeSelf)
                    {
                        Cutscene6WasActive = true;
                    }
                    if (!Cutscene6.activeSelf && Cutscene6WasActive && !Cutscene6Split)
                    {
                        Cutscene6Split = true;
                        Livesplit.SendCommand("split\r\n");
                        SplitIndex++;
                    }
                }*/
                CustomSplit split = splits[SplitIndex];
                if (split.condition != null)
                {
                    bool fulfilled = split.condition.Fulfilled();
                    if (split.collider != null)
                    {
                        if (fulfilled && split.collider.bounds.Contains(GameObject.Find("S-105").transform.position)) {
                            if (split.shouldSplitHere)
                            {
                                Plugin.Log.LogInfo("Splitting at split " + split.SplitName);
                                Livesplit.SendCommand("split\r\n");
                            }
                            SplitIndex++;
                        }
                    }
                    else
                    {
                        if (fulfilled)
                        {
                            if (split.shouldSplitHere)
                            {
                                Plugin.Log.LogInfo("Splitting at split " + split.SplitName);
                                Livesplit.SendCommand("split\r\n");
                            }
                            SplitIndex++;
                        }
                    }
                }
                else
                {
                    if (split.collider.bounds.Contains(GameObject.Find("S-105").transform.position))
                    {
                        if (split.shouldSplitHere)
                        {
                            Plugin.Log.LogInfo("Splitting at split " + split.SplitName);
                            Livesplit.SendCommand("split\r\n");
                        }
                        SplitIndex++;
                    }
                }
            }
        }
    }

    public class Condition
    {
        public string Name;
        public string Path;
        public string Property;
        public string Value;
        public string ValueType;
        public string Comparison;
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

        public bool Fulfilled()
        {
            if (Path.Contains("."))
            {
                GameObject @object = GameObject.Find(Path.Split(char.Parse("."))[0]);
                Component component = @object.GetComponent(Path.Split(char.Parse("."))[1]);
                object parsed = "";
                Type type = typeof(int);
                switch(ValueType)
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
                object fieldValue = component.GetType().GetField(Property).GetValue(component);

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
                return false;
            }
            else
            {
                try
                {
                    GameObject @object = GameObject.Find(Path);
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
                    Plugin.Log.LogInfo($"Getting {Property} on {@object}");
                    FieldInfo info = @object.GetType().GetField(Property, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    Plugin.Log.LogInfo($"FieldInfo gotten, {info}. Getting value.");
                    object fieldValue = info.GetValue(@object);
                    Plugin.Log.LogInfo($"Got {Property} on {@object}");

                    if (fieldValue != null && fieldValue.GetType() == type)
                    {
                        object convertedParsed = Convert.ChangeType(parsed, type);

                        bool returnVal = Comparison switch
                        {
                            "==" => fieldValue.Equals(convertedParsed),
                            "<" => Comparer.Default.Compare(fieldValue, convertedParsed) < 0,
                            ">" => Comparer.Default.Compare(fieldValue, convertedParsed) > 0,
                            ">=" => Comparer.Default.Compare(fieldValue, convertedParsed) >= 0,
                            "<=" => Comparer.Default.Compare(fieldValue, convertedParsed) <= 0
                        };

                        Plugin.Log.LogInfo($"return value for comparison {Property} {Comparison} {Value} is {returnVal}");

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
                catch (Exception e)
                {
                    Plugin.Log.LogError(e);
                }
                return false;
            }
        }
    }

    internal class SplitLoader
    {
        public static Condition ParseCondition(JToken condition)
        {
            if (condition == null) return null;
            Condition cond = new()
            {
                Name = (string)condition["Name"],
                Path = (string)condition["Path"],
                Property = (string)condition["Property"],
                Value = (string)condition["Value"],
                ValueType = (string)condition["ValueType"],
                Comparison = (string)condition["Comparison"]
            };
            return cond;
        }

        public static Condition[] ParseConditionArray(JArray Conditions)
        {
            Condition[] conditions = [];
            foreach (JToken cond in Conditions)
            {
                conditions = [.. conditions, ParseCondition(cond)];
            }
            return conditions;
        }

        public static Vector3 ParseVector(string vector)
        {
            string[] parts = vector.Split(char.Parse(" "));
            return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
        }

        public static BoxCollider ParseCollider(JToken collider)
        {
            if (collider == null) return null;
            GameObject colliderObject = new((string)collider["name"]);
            BoxCollider coll = colliderObject.AddComponent<BoxCollider>();
            coll.transform.SetParent(Plugin.ColliderStorage.transform);
            coll.size = ParseVector((string)collider["size"]);
            coll.transform.position = ParseVector((string)collider["transform"]);
            return coll;
        }

        public static string ParseColliderToJson(BoxCollider collider)
        {
            return $"{{ \"size\": \"{collider.size.x} {collider.size.y} {collider.size.z}\", \"transform\": \"{collider.transform.position.x} {collider.transform.position.y} {collider.transform.position.z}\"}}";
        }
        
        public static string ParseConditionToJson(Condition condition)
        {
            return $"{{ \"Name\": \"{condition.Name}\", \"Path\": \"{condition.Path}\", \"Property\": \"{condition.Property}\", \"Value\": \"{condition.Value}\", \"ValueType\": \"{condition.ValueType}\", \"Comparison\": \"{condition.Comparison}\"";
        }
        public static void LoadSplits()
        {
            if (!File.Exists(SpeedrunnerUtils.SplitPath))
            {
                string baseData = @"[]";

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
                    CustomSplit spl = new()
                    {
                        SplitName = (string)split["SplitName"],
                        condition = ParseCondition(split["condition"]),
                        collider = ParseCollider(split["collider"]),
                        shouldSplitHere = (string)split["shouldSplitHere"] == "true"
                    };
                    SpeedrunnerUtils.splits = [.. SpeedrunnerUtils.splits, spl];
                    if (spl.shouldSplitHere)
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

        public static void SaveSplits()
        {
            string[] data = [];
            foreach (var split in SpeedrunnerUtils.splits)
            {
                string parsed = $"{{\"SplitName\": \"${split.SplitName}\", \"condition\": {ParseConditionToJson(split.condition)}, \"collider\": {ParseColliderToJson(split.collider)}}}";
                data = [.. data, parsed];
            }
            using StreamWriter streamWriter = new(SpeedrunnerUtils.SplitPath);
            streamWriter.Write(data);
        }
    }

    public class CustomSplit
    {
        public BoxCollider? collider;
        public Condition? condition;
        public string SplitName;
        public bool shouldSplitHere;
    }
}