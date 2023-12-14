using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using System.Collections;

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
        private BoxCollider CurrentCollider;
        public static CustomSplit[] splits = [];
        private bool CreatingCollider = false;
        private int SplitIndex = 0;
        private bool Cutscene6Split = false;
        private bool MenuLoadedBefore = false;
        public static Condition[] Conditions = Array.Empty<Condition>();
        public static string ConditionPath = Path.Combine(BepInEx.Paths.ConfigPath, "SplitConditions.json");
        public static string SplitPath = Path.Combine(BepInEx.Paths.ConfigPath, "Splits.json");

        private void Awake()
        {
            SceneManager.activeSceneChanged += OnSceneChanged;
            Livesplit.StartSocket();
            SplitLoader.LoadSplits();
        }

        private void OnSceneChanged(Scene old, Scene newS)
        {
            CurrentScene = newS.name;
            if (CurrentScene == "Menu")
            {
                if (MenuLoadedBefore) Livesplit.SendCommand("pause\r\n");
                MenuLoadedBefore = true;
            }
            if (CurrentScene != "Menu" && CurrentScene != "Intro")
            {
                Livesplit.SendCommand("reset\r\n");
            }
        }

        private void StartAddingCollider()
        {
            CreatingCollider = true;
            GameObject Sen = GameObject.Find("S-105");
            ColliderStart = Sen.transform.position;
            GameObject Collider = new($"Collider {Colliders.Length + 1}");
            Collider.AddComponent<VisualiserComponent>();
        }

        private void Update()
        {
            if (CurrentScene != "Intro" && CurrentScene != "Menu")
            {
                GameObject Director = GameObject.Find("Director");
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
                }
                /*foreach (CustomSplit split in splits)
                {
                    if (split.collider != null)
                    {
                        if (split.condition != null)
                        {
                            if (split.collider.bounds.Contains(GameObject.Find("S-105").transform.position) && split.condition.Fulfilled())
                            {
                                
                            }
                        }
                    }
                }*/
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
                object fieldValue = @object.GetType().GetField(Property).GetValue(@object);

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
        }
    }

    internal class SplitLoader
    {
        public static Condition ParseCondition(JToken condition)
        {
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

        public static Vector3 ParseVector(string vector)
        {
            string[] parts = vector.Split(char.Parse(" "));
            return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
        }

        public static BoxCollider ParseCollider(JToken collider)
        {
            BoxCollider coll = new()
            {
                size = ParseVector((string)collider["size"]),
                name = (string)collider["name"]
            };
            return coll;
        }

        public static string ParseColliderToJson(BoxCollider collider)
        {
            return $"{{ \"size\": \"{collider.size.x} {collider.size.y} {collider.size.z}\" }}";
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

            string json = File.ReadAllText(SpeedrunnerUtils.ConditionPath);

            JObject conditionsObject = JObject.Parse(json);
            JArray splits = (JArray)conditionsObject["conditions"];

            foreach(var split in splits)
            {
                CustomSplit spl = new()
                {
                    SplitName = (string)split["SplitName"],
                    condition = ParseCondition(split["condition"]),
                    collider = ParseCollider(split["collider"]),
                    name = (string)split["name"]
                };
                SpeedrunnerUtils.splits = [ .. SpeedrunnerUtils.splits, spl ];
            }
        }

        public static void SaveSplits()
        {
            string[] data = [];
            foreach (var split in SpeedrunnerUtils.splits)
            {
                string parsed = $"{{\"SplitName\": \"${split.SplitName}\", \"name\": \"{split.name}\", \"condition\": {ParseConditionToJson(split.condition)}, \"collider\": {ParseColliderToJson(split.collider)}}}";
                data = [.. data, parsed];
            }
            using StreamWriter streamWriter = new(SpeedrunnerUtils.SplitPath);
            streamWriter.Write(data);
        }
    }
    internal class ConditionLoader
    {
        public static void LoadConditions()
        {
            if (!File.Exists(SpeedrunnerUtils.ConditionPath))
            {
                string baseData = @"
        {
            ""conditions"": []
        }";

                // Create or overwrite the file with the base data
                using StreamWriter streamWriter = new(SpeedrunnerUtils.ConditionPath);
                streamWriter.Write(baseData);
            }
            string json = File.ReadAllText(SpeedrunnerUtils.ConditionPath);

            JObject conditionsObject = JObject.Parse(json);
            JArray conditions = (JArray)conditionsObject["conditions"];

            foreach (var condition in conditions)
            {
                Condition cond = new()
                {
                    Name = (string)condition["Name"],
                    Path = (string)condition["Path"],
                    Value = (string)condition["Value"],
                    Comparison = (string)condition["Comparison"]
                };
                SpeedrunnerUtils.Conditions = [.. SpeedrunnerUtils.Conditions, cond];
            }
        }
    }

    public class CustomSplit
    {
        public BoxCollider? collider;
        public string name;
        public Condition? condition;
        public string SplitName;
    }
}