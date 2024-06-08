using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;
using System;
using BepInEx.Configuration;
using System.IO;
using Devdog.General.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using UIWindowPageFramework;
using HarmonyLib;

namespace SpeedrunningUtils
{
	internal class PluginInfo
	{
		internal const string GUID = "tairasoul.vaproxy.speedrunning";
		internal const string Name = "SpeedrunningUtils";
		internal const string Version = "3.1.0";
	}

	[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
	public class Plugin : BaseUnityPlugin
	{
		public static ManualLogSource Log;
		internal static GameObject ColliderStorage;
		internal static bool VisualisingHitboxes = false;

		internal static int CurrentSaveSlot = 0;
		internal static VisualiserComponent Visualiser;
		internal static ConfigEntry<bool> VisualizeHitboxesByDefault;
		internal static ConfigEntry<bool> SetLayout;
		internal static ConfigEntry<string> LastLoadedConfig;
		internal static ConfigEntry<KeyboardShortcut> RestartKey;
		internal static ConfigFile cfg;
		internal static SpeedrunnerUtils utils;

		Harmony harmony = new("tairasoul.vaproxy.speedrunning");
		private bool init = false;

		private void Awake()
		{
			cfg = Config;
			VisualizeHitboxesByDefault = cfg.Bind("Speedrunning", "Visualise split bounds by default", true, "Should a split's bounds be visualised by default?");
			LastLoadedConfig = cfg.Bind("Speedrunning", "Last loaded config", "", "The config last loaded by SpeedrunningUtils.");
			SetLayout = cfg.Bind("Speedrunning", "Set Layout", false, "Should SpeedrunningUtils forcibly set the layout where specified?");
			RestartKey = cfg.Bind("Keybinds", "Restart keybind", new KeyboardShortcut(KeyCode.P), "Keybind to restart from the beginning of the game.");
			VisualisingHitboxes = VisualizeHitboxesByDefault.Value;
			Log = Logger;
			harmony.PatchAll();
			Log.LogInfo("SpeedrunningUtils awake.");
			SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) => {
				if (scene.name == "Menu") {
					SettingsMainMenu menu = GameObject.Find("Canvas").Find("Optimize").GetComponent<SettingsMainMenu>();
					foreach (Toggle toggle in menu.toggles) {
						toggle.isOn = false;
						toggle.onValueChanged.Invoke(false);
					}
					PlayerPrefs.SetInt("StaticAI", 1);
					PlayerPrefs.SetInt("DynamicAI", 1);
					PlayerPrefs.SetInt("Shadow", 1);
					PlayerPrefs.SetInt("Graphics", 1);
					QualitySettings.SetQualityLevel(0);
				}
			};
		}

		private void Start()
		{
			Init();
		}

		private void OnDestroy()
		{
			Init();
		}

		private void Init()
		{
			if (!init)
			{
				Log.LogInfo("Initializing SpeedrunningUtils.");
				init = true;
				GameObject Utils = new("SpeedrunnerUtils");
				ColliderStorage = new("SpeedrunnerUtils.Colliders");
				Visualiser = ColliderStorage.AddComponent<VisualiserComponent>();
				DontDestroyOnLoad(ColliderStorage);
				DontDestroyOnLoad(Utils);
				utils = Utils.AddComponent<SpeedrunnerUtils>();
				utils.enabled = true;
				GameObject window = Framework.CreateWindow("SpeedrunningUtils");
				Framework.RegisterWindow(window, (GameObject window) => 
				{
					PageHandlers.Setup(window);
				});
				Log.LogInfo("SpeedrunningUtils initialized. Good luck speedrunning!");
			}
		}
	}
	
	public static class PageHandlers 
	{
		static T Find<T>(Func<T, bool> predicate)
		{
			foreach (T find in GameObject.FindObjectsOfTypeAll(typeof(T)).Cast<T>())
			{
				if (predicate(find)) return find;
			}
			return default;
		}

		static void SetupHeader(GameObject RegisteredWindow)
		{
			Plugin.Log.LogInfo($"Setting up header for {RegisteredWindow}");
			try
			{
				GameObject Header = RegisteredWindow.Find("Header"); 
				CanvasRenderer rend = Header.GetComponent<CanvasRenderer>() ?? Header.AddComponent<CanvasRenderer>();
				rend.materialCount = 1;
				Material origin = Find((Material m) =>
				{
					return m.name == "Default UI Material";
				});
				Material newM = new Material(origin)
				{
					name = "Modified UI Material",
					renderQueue = origin.renderQueue + 1
				};
				rend.SetMaterial(newM, 0);
			}
			catch (Exception ex)
			{
				Plugin.Log.LogError(ex);
			}
		}
		
		internal static void Setup(GameObject Window) 
		{
			SetupHeader(Window);
			CreateButtons(Window);
			CreatePages(Window);
		}
		
		static void CreateButtons(GameObject window) 
		{
			GameObject ButtonStorage = window.AddObject("Content");
			ButtonStorage.AddComponent<RectTransform>();
			GameObject Settings = ComponentUtils.CreateButton("Settings", "tairasoul.speedrunningutils.settings");
			Settings.SetParent(ButtonStorage, false);
			Settings.GetComponent<RectTransform>().anchoredPosition = new(-533, 213);
			Settings.GetComponent<RectTransform>().sizeDelta = new(141, 50);
			Settings.Find("ItemName").GetComponent<RectTransform>().anchoredPosition = new(9, 0);
			GameObject Splits = ComponentUtils.CreateButton("Splits", "tairasoul.speedrunningutils.splits");
			Splits.SetParent(ButtonStorage, false);
			Splits.GetComponent<RectTransform>().anchoredPosition = new(-533, 150);
			Splits.GetComponent<RectTransform>().sizeDelta = new(141, 50);
			Splits.Find("ItemName").GetComponent<RectTransform>().anchoredPosition = new(9, 0);
			Settings.GetComponent<Button>().onClick.AddListener(() => 
			{
				window.Find("Content").Find("Settings").SetActive(true);
				window.Find("Content").Find("Splits").SetActive(false);
			});
			Splits.GetComponent<Button>().onClick.AddListener(() => 
			{
				window.Find("Content").Find("Settings").SetActive(false);
				window.Find("Content").Find("Splits").SetActive(true);
			});
		}
		
		static void CreatePages(GameObject window) 
		{
			GameObject Content = window.Find("Content");
			GameObject Splits = Content.AddObject("Splits");
			Splits.AddComponent<RectTransform>().anchoredPosition = new(0, 69);
			GameObject Settings = Content.AddObject("Settings");
			Settings.AddComponent<RectTransform>();
			Splits.SetActive(false);
			CreateSplitsBox(Splits);
			GameObject Toggle = ComponentUtils.CreateToggle("Visualize Split Colliders", "speedrunning.visualise.splitcolliders");
			Plugin.Log.LogInfo("Created toggle.");
			Toggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-396.9787f, 210);
			Toggle.transform.localScale = new Vector3(1.9268f, 1.9268f, 1.9268f);
			Toggle.Find("Label").GetComponent<RectTransform>().anchoredPosition = new Vector2(32.7437f, 0);
			Toggle.SetParent(Settings, false);
			Toggle.GetComponent<Toggle>().isOn = Plugin.VisualizeHitboxesByDefault.Value;
			Toggle.GetComponent<Toggle>().onValueChanged.AddListener((bool enabled) =>
			{
				Plugin.VisualisingHitboxes = enabled;
				Plugin.VisualizeHitboxesByDefault.Value = enabled;
			});
			string dir = $"{Paths.PluginPath}/SpeedrunningUtils.Splits";
			if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
			string[] files = Directory.EnumerateFiles(dir).ToArray();
			foreach (string file in files) 
			{
				CreateSplitButton(Splits, file);
			}
		}
		
		static void CreateSplitsBox(GameObject cntent) 
		{
			GameObject ScrollbarVertical = GameObject.Find("MAINMENU").Find("Canvas").Find("Pages").Find("Inventory").Find("Content").Find("__INVENTORY_CONTAINER").Find("Container").Find("InventorySlots").Find("Scrollbar Vertical");
			GameObject ScrollVertical = ScrollbarVertical.Instantiate();
			ScrollVertical.name = "Scrollbar Vertical";
			ScrollVertical.SetParent(cntent, false);
			ScrollVertical.GetComponent<RectTransform>().anchoredPosition = new Vector2(483.2355f, -113.0717f);
			Scrollbar bar = ScrollVertical.GetComponent<Scrollbar>();
			bar.direction = Scrollbar.Direction.TopToBottom;
			bar.interactable = true;
			bar.navigation = Navigation.defaultNavigation;
			bar.useGUILayout = true;
			ScrollVertical.GetComponent<RectTransform>().localScale = new Vector3(1, 5, 1);
			GameObject Viewport = new GameObject("Viewport");
			RectTransform ViewportRect = Viewport.AddComponent<RectTransform>();
			Viewport.AddComponent<CanvasRenderer>();
			Viewport.AddComponent<Animator>();
			CanvasGroup canvas = Viewport.AddComponent<CanvasGroup>();
			canvas.blocksRaycasts = true;
			canvas.interactable = true;
			Viewport.AddComponent<Mask>();
			Viewport.AddComponent<AnimatorHelper>();
			Viewport.SetParent(cntent, false);
			ViewportRect.anchoredPosition = new Vector2(73.3544f, -190.1612f);
			ViewportRect.sizeDelta = new Vector2(1000, 700);
			ScrollRect scroll = cntent.AddComponent<ScrollRect>();
			scroll.inertia = true;
			scroll.horizontal = false;
			scroll.decelerationRate = 0.135f;
			scroll.elasticity = 0.1f;
			scroll.movementType = ScrollRect.MovementType.Elastic;
			scroll.scrollSensitivity = 25;
			scroll.vertical = true;
			scroll.verticalScrollbar = bar;
			scroll.viewport = ViewportRect;
			scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
			GameObject Content = Viewport.AddObject("Content");
			RectTransform contentTransform = Content.AddComponent<RectTransform>();
			contentTransform.anchoredPosition = new Vector2(-11.8327f, 0.0009f);
			contentTransform.sizeDelta = new Vector2(900, 600);
			scroll.content = contentTransform;
			GridLayoutGroup group = Content.AddComponent<GridLayoutGroup>();
			group.childAlignment = TextAnchor.UpperLeft;
			group.spacing = new Vector2(80, 20);
			group.cellSize = new Vector2(350, 50);
			group.startCorner = GridLayoutGroup.Corner.UpperLeft;
			group.startAxis = GridLayoutGroup.Axis.Horizontal;
			group.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			group.constraintCount = 2;
		}
		
		static void CreateSplitButton(GameObject window, string file) 
		{
			GameObject button = ComponentUtils.CreateButton(Path.GetFileName(file), $"split.file.{Path.GetFileName(file)}");
			Button b = button.GetComponent<Button>();
			b.onClick.AddListener(() =>
			{
				Plugin.LastLoadedConfig.Value = Path.GetFileName(file);
				Plugin.utils.Clear();
				SplitLoader.LoadSplits(file);
				Livesplit.SendCommand("reset");
			});
			Text text = button.Find("ItemName").GetComponent<Text>();
			text.resizeTextForBestFit = true;
			text.verticalOverflow = VerticalWrapMode.Overflow;
			text.horizontalOverflow = HorizontalWrapMode.Overflow;
			button.Find("ItemName").GetComponent<RectTransform>().anchoredPosition = new Vector2(-89.3072f, 0);
			LayoutElement elem = button.AddComponent<LayoutElement>();
			elem.minWidth = 50f;
			elem.minHeight = 50f;
			button.SetParent(window.Find("Viewport").Find("Content"), false);
		}
	}
}
