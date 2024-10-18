using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using System.Collections;
using BepInEx;
using System.Reflection;
using UIWindowPageFramework;

namespace SpeedrunningUtils
{
	internal static class JExtensions
	{
		internal static bool HasKey(this JToken token, string key)
		{
			if (token.Type == JTokenType.Object)
			{
				JObject obj = (JObject)token;
				if (obj.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out JToken propValue))
				{
					return true;
				}
			}
			return false;
		}
	}
	internal static class ObjectExtensions
	{
		// man i should really use this at some point
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
				FieldInfo[] fields = objType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
				foreach (FieldInfo field in fields)
				{
					if (field.Name == fieldNameOrPropertyName)
					{
						return field.GetValue(obj);
					}
				}

				// Attempt to get the property
				PropertyInfo[] properties = objType.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
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
	public class SpeedrunnerUtils : MonoBehaviour
	{
		private string CurrentScene = "Intro";
		internal static CustomSplit[] splits = [];
		private int SplitIndex = 0;
		private bool RestartKeyDown = false;

		internal void Clear()
		{
			//Livesplit.SendCommand("clearsplits");
			splits = [];
		}

		private void Awake()
		{
			Plugin.Log.LogInfo("SpeedrunnerUtils component awake.");
			SceneManager.activeSceneChanged += OnSceneChanged;
			if (Plugin.LastLoadedConfig.Value != "")
				SplitLoader.LoadSplits($"{Paths.PluginPath}/SpeedrunningUtils.Splits/{Plugin.LastLoadedConfig.Value}");
			OBS.ObsWebsocket.RecordingStopped += RecordingStopped;
		}

		private void OnSceneChanged(Scene old, Scene newS)
		{
			CurrentScene = newS.name;
			if (CurrentScene != "Menu" && CurrentScene != "Intro")
			{
				Livesplit.SendCommand("reset");
			}
			if (CurrentScene == "Menu")
			{
				SplitIndex = 0;
			}
		}

		private IEnumerator ResetTimer() {
			while (GameObject.FindFirstObjectByType<SaveSlotSelect>() == null)
				yield return new WaitForEndOfFrame();
			ClearSlotData(Plugin.CurrentSaveSlot);
		}

		private static void ClearSlotData(int ID)
		{
			PlayerPrefs.SetInt("fresh" + ID, 0);
		}

		private async Task Update()
		{
			Application.targetFrameRate = 99999;
			if (Plugin.RestartKey.Value.IsDown())
			{
				if (!RestartKeyDown)
				{
					RestartKeyDown = true;
					SceneManager.LoadScene(1, LoadSceneMode.Single);
					Livesplit.SendCommand("reset");
					StartCoroutine(ResetTimer());
					SplitIndex = 0;
				}
			}
			else
			{
				RestartKeyDown = false;
			}
			if (CurrentScene != "Intro" && CurrentScene != "Menu")
			{
				if (SplitIndex < splits.Length) 
				{
					CustomSplit split = splits[SplitIndex];
					if (split.splitCondition != null)
					{
						bool splitFulfilled = split.splitCondition.Fulfilled();
						if (split.splitBounds != null)
						{
							if (splitFulfilled && split.splitBounds.Value.Contains(GameObject.Find("S-105.1").transform.position))
							{
								if (split.Command != null)
								{
									Plugin.Log.LogInfo($"Executing command {split.Command} at split {split.SplitName}");
									Livesplit.SendCommand(split.Command);
								}
								SplitIndex++;
							}
						}
						else
						{
							if (splitFulfilled)
							{
								if (split.Command != null)
								{
									Plugin.Log.LogInfo($"Executing command {split.Command} at split {split.SplitName}");
									Livesplit.SendCommand(split.Command);
								}
								SplitIndex++;
							}
						}
					}
					else if (split.splitBounds != null)
					{
						if (split.splitBounds.Value.Contains(GameObject.Find("S-105.1").transform.position))
						{
							if (split.Command != null)
							{
								Plugin.Log.LogInfo($"Executing command {split.Command} at split {split.SplitName}");
								Livesplit.SendCommand(split.Command);
							}
							SplitIndex++;
						}
					}
				}
				else 
				{
					if (Plugin.WebsocketConnected) 
					{
						StartCoroutine(StopRecording());
					}
				}
			}
		}
		
		internal IEnumerator StopRecording() 
		{
			yield return new WaitForSeconds(3);
			_StopRecording();
		}
		
		internal async Task _StopRecording() 
		{
			if (Plugin.Recording) 
			{
				Plugin.Recording = false;
				OBS.ObsWebsocket.StopRecording();
			}
		}
		
		internal void RecordingStopped(string output) 
		{
			Plugin.Log.LogInfo($"Recording stopped, output path {output}");
		}
	}

	public class Condition
	{
		public string Name;
		public string Path;
		public string Property;
		public string Value;
		public string Comparison;
		public string? ComponentName;
		private object ParseValue(object value, object conversionTarget)
		{
			return Convert.ChangeType(value, conversionTarget.GetType());
		}

		public bool Fulfilled()
		{
			if (ComponentName != null)
			{
				try
				{
					string[] splits = Path.Split("/".ToCharArray());
					GameObject obj = GameObject.Find(splits[0]);
					for (int i = 1; i < splits.Length; i++)
					{
						obj = obj.Find(splits[i]);
					}
					Component component = obj.GetComponent(ComponentName);
					object fieldValue = null;
					try
					{
						Type objType = component.GetType();
						// Attempt to get the field
						FieldInfo[] fields = objType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Static);
						foreach (FieldInfo field in fields)
						{
							if (field.Name == Property)
							{
								fieldValue = field.GetValue(component);
							}
						}
						// Attempt to get the property
						PropertyInfo[] properties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Static);
						foreach (PropertyInfo property in properties)
						{
							if (property.Name == Property)
							{
								fieldValue = property.GetValue(component);
							}
						}
					}
					catch (Exception ex)
					{
						Plugin.Log.LogError(ex);
					}
					object parsed = ParseValue(Value, fieldValue);

					if (fieldValue != null)
					{

#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
						return Comparison switch
						{
							"==" => fieldValue.Equals(parsed),
							"<" => Comparer.Default.Compare(fieldValue, parsed) < 0,
							">" => Comparer.Default.Compare(fieldValue, parsed) > 0,
							">=" => Comparer.Default.Compare(fieldValue, parsed) >= 0,
							"<=" => Comparer.Default.Compare(fieldValue, parsed) <= 0
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
					//PropertyInfo info = obj.GetType().GetProperty(Property);
					//object fieldValue = info.GetValue(obj);
					object fieldValue = null;
					try
					{
						Type objType = obj.GetType();
						// Attempt to get the field
						FieldInfo[] fields = objType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
						foreach (FieldInfo field in fields)
						{
							
							if (field.Name == Property)
							{
								fieldValue = field.GetValue(obj);
							}
						}

						// Attempt to get the property
						PropertyInfo[] properties = objType.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
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
					object parsed = ParseValue(Value, fieldValue);
					if (fieldValue != null)
					{

#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
						return Comparison switch
						{
							"==" => fieldValue.Equals(parsed),
							"<" => Comparer.Default.Compare(fieldValue, parsed) < 0,
							">" => Comparer.Default.Compare(fieldValue, parsed) > 0,
							">=" => Comparer.Default.Compare(fieldValue, parsed) >= 0,
							"<=" => Comparer.Default.Compare(fieldValue, parsed) <= 0
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
				ComponentName = comp,
				Property = (string)condition["Property"],
				Value = (string)condition["Value"],
				Comparison = (string)condition["Comparison"]
			};
			return cond;
		}
		
		// Commented out, will be used later for mulit-condition splits.
		/*
		internal static Condition[] ParseConditionArray(JArray Conditions)
		{
			Condition[] conditions = new Condition[0];
			foreach (JToken cond in Conditions)
			{
				List<Condition> conditions1 = conditions.ToList();
				conditions1.Add(ParseCondition(cond));
				conditions = conditions1.ToArray();
			}
			return conditions;
		}*/

		internal static string ParseBoundsToJson(Bounds bounds)
		{
			return $"{{ \"size\": \"{bounds.size.x} {bounds.size.y} {bounds.size.z}\", \"center\": \"{bounds.center.x} {bounds.center.y} {bounds.center.z}\"}}";
		}
		
		internal static string ParseConditionToJson(Condition condition)
		{
			return $"{{ \"Name\": \"{condition.Name}\", \"Path\": \"{condition.Path}\", \"Property\": \"{condition.Property}\", \"Value\": \"{condition.Value}\", \"Comparison\": \"{condition.Comparison}\"";
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
		
		// Commented out for later, may try to make an actual split editor.
		/*internal static string ParseBoundsToJson(Bounds bounds)
		{
			return $"{{ \"size\": \"{bounds.size.x} {bounds.size.y} {bounds.size.z}\", \"center\": \"{bounds.center.x} {bounds.center.y} {bounds.center.z}\"}}";
		}
		
		internal static string ParseConditionToJson(Condition condition)
		{
			return $"{{ \"Name\": \"{condition.Name}\", \"Path\": \"{condition.Path}\", \"Property\": \"{condition.Property}\", \"Value\": \"{condition.Value}\", \"Comparison\": \"{condition.Comparison}\"";
		}*/
		
		internal static void LoadSplits(string path)
		{

			string json = File.ReadAllText(path);

			JArray splits = JArray.Parse(json);
			int actualSplitIndex = 0;
			for (int i = 0; i < splits.Count; i++)
			{
				try
				{
					var split = splits[i];
					Plugin.Log.LogInfo($"Loading split {(string)split["SplitName"]}");
					bool addToLayout = false;
					bool splitHere = true;
					if (split.HasKey("splitHere"))
					{
						splitHere = (string)split["splitHere"] == "true";
					}
					CustomSplit spl = new()
					{
						SplitName = (string)split["SplitName"],
						splitCondition = ParseCondition(split["splitCondition"]),
						splitBounds = ParseBounds(split["splitBounds"]),
						Command = (string)split["Command"]
					};
					List<CustomSplit> splitList = SpeedrunnerUtils.splits.ToList();
					splitList.Add(spl);
					SpeedrunnerUtils.splits = splitList.ToArray();
					if (splitHere)
						actualSplitIndex++;
					Plugin.Log.LogInfo($"Loaded split {(string)split["SplitName"]}");
				}
				catch (Exception e)
				{
					Plugin.Log.LogError(e);
				}
			}
		}
	}

	public struct CustomSplit
	{
		public Bounds? splitBounds;
		public Condition? splitCondition;
		public string SplitName;
		public string Command;
	}
}