using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MainMenuSettings;
using speedrunningutils.impls;
using tairasoul.unity.common.embedded;
using tairasoul.unity.common.events;
using tairasoul.unity.common.speedrunning.dsl.config;
using tairasoul.unity.common.speedrunning.dsl.eventbus;
using tairasoul.unity.common.speedrunning.dsl.internals;
using tairasoul.unity.common.speedrunning.livesplit;
using tairasoul.unity.common.speedrunning.runtime;
using tairasoul.unity.common.util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace speedrunningutils;

[BepInPlugin("tairasoul.vaproxy.speedrunning", "SpeedrunningUtils", "4.0.2")]
class Plugin : BaseUnityPlugin {
	internal static ManualLogSource Log = null!;
	internal static Config cfg = null!;
	internal static OBS obs = null!;
	Harmony harmony = new("tairasoul.vaproxy.speedrunning");

	static Plugin() {
		string library = Path.Combine(Paths.PluginPath, "libraries");
		if (!Directory.Exists(library))
			Directory.CreateDirectory(library);
		string a4Path = Path.Combine(library, "Antlr4.Runtime.Standard.dll");
		if (!File.Exists(a4Path))
		{
			byte[] antlr4Bytes = AssemblyUtils.GetResourceBytes("speedrunningutils.Antlr4.Runtime.Standard.dll");
			File.WriteAllBytes(a4Path, antlr4Bytes);
			Assembly.LoadFrom(a4Path);
		}
		EmbeddedDependencyLoader.Init(AppDomain.CurrentDomain, "speedrunningutils", ["Newtonsoft.Json", "WatsonWebsocket"]);
	}

	bool restartKeyDown = false;

	public void Start() {
		Log = Logger;
		cfg = new(Config);
		obs = new();
		Task.Run(obs.Connect);
		GameObject br = new("speedrunningutils.boundsregistry");
		DontDestroyOnLoad(br);
		DslCompilationConfig.BoundsRegistryClass = br.AddComponent<BoundsRegistry>();
		EventTypeRegistry.Register("ItemPickup", [typeof(string), typeof(int), typeof(int)]);
		ITimer timer;
		if (cfg.UseTCP.Value)
			timer = new LivesplitTCP();
		else
			timer = new Livesplit();
		RuntimeInterface.Setup("4.0.2", Path.Combine(Paths.PluginPath, "split-src"), Path.Combine(Paths.PluginPath, "split-build"), timer);
		if (cfg.EnableOBSWebsocket.Value)
			EventBus.Listen(new DslFileCompleted(), "file-completed", (_) => {
				Task.Run(async () =>
				{
					await Task.Delay(TimeSpan.FromSeconds(3));
					obs.StopRecording();
				});
			});
		harmony.PatchAll();

		ToggleOption visualize = new()
		{
			defaultState = cfg.VisualizeHitboxesByDefault.Value,
			Id = "tairasoul.speedrunningutils.visualize",
			Text = "Visualize Bounds",
			Toggled = toggled =>
			{
				cfg.VisualizeHitboxesByDefault.Value = toggled;
			}
		};

		ModOptions opts = new()
		{
			toggles = [visualize],
			buttons = [.. MenuImpl.Buttons()],
			CreationCallback = MenuImpl.CreateActiveText
		};
		MenuSettings.RegisterMod("SpeedrunningUtils", "tairasoul.vaproxy.speedrunningutils", opts);
		if (cfg.LastLoadedConfig.Value != "")
			RuntimeInterface.Load(cfg.LastLoadedConfig.Value);
		SceneManager.activeSceneChanged += (old, _new) => {
			if (!RuntimeInterface.behaviour.IsActive)
			{
				if (_new.buildIndex == 2)
				{
					RuntimeInterface.GameStarted();
				}
			}
			else {
				EventBus.Send(new DslId("SceneChange"), new DslData([_new.buildIndex]));
			}
		};
	}

	void Update() {
		if (cfg.RestartKey.Value.IsDown())
		{
			if (!restartKeyDown)
			{
				restartKeyDown = true;
				SceneManager.LoadScene(1, LoadSceneMode.Single);
				RuntimeInterface.Reset();
				int saveSlot = PlayerPrefs.GetInt("Slot");
				Task.Run(async () =>
				{
					while (GameObject.FindFirstObjectByType<SaveSlotSelect>() == null)
						await Task.Delay(100);
					var select = GameObject.FindFirstObjectByType<SaveSlotSelect>();
					select.currentSlot = saveSlot;
					select.ClearSlotData();
				});
			}
		}
		else
		{
			restartKeyDown = false;
		}
	}
}