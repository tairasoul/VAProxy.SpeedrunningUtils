using BepInEx;
using Settings = SettingsAPI.Plugin;
using BepInEx.Logging;
using UnityEngine;
using SettingsAPI;
using UnityEngine.UI;
using System;
using BepInEx.Configuration;

namespace SpeedrunningUtils
{
    [BepInPlugin("tairasoul.vaproxy.speedrunning", "SpeedrunningUtils", "1.0.2")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        internal static GameObject ColliderStorage;
        internal static bool VisualisingHitboxes = false;
        internal static VisualiserComponent Visualiser;
        internal static ConfigEntry<bool> VisualizeHitboxesByDefault;
        private bool init = false;

        private void Awake()
        {
            VisualizeHitboxesByDefault = Config.Bind("Speedrunning", "Visualise split bounds by default", true, "Should a split's bounds be visualised by default?");
            VisualisingHitboxes = VisualizeHitboxesByDefault.Value;
            Log = Logger;
            Log.LogInfo("SpeedrunningUtils awake.");
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
                Utils.AddComponent<SpeedrunnerUtils>();
                Log.LogInfo("SpeedrunningUtils initialized. Good luck speedrunning!");
                Option[] options = [];
                Option VisualizeOption = new()
                {
                    Create = (GameObject page) =>
                    {
                        try
                        {
                            GameObject Toggle = ComponentUtils.CreateToggle("Visualize Split Colliders", "speedrunning.visualise.splitcolliders");
                            Toggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-396.9787f, 122.7571f);
                            Toggle.transform.localScale = new Vector3(1.9268f, 1.9268f, 1.9268f);
                            Toggle.Find("Label").GetComponent<RectTransform>().anchoredPosition = new Vector2(32.7437f, 0);
                            Toggle.SetParent(page, false);
                            Toggle.GetComponent<Toggle>().isOn = VisualizeHitboxesByDefault.Value;
                            Toggle.GetComponent<Toggle>().onValueChanged.AddListener((bool enabled) =>
                            {
                                VisualisingHitboxes = enabled;
                                VisualizeHitboxesByDefault.Value = enabled;
                            });
                        }
                        catch (Exception ex)
                        {
                            Log.LogError(ex);
                        }
                    }
                };
                options = options.Append(VisualizeOption);
                Settings.API.RegisterMod("tairasoul.speedrunning.utils", "SpeedrunningUtils", options);
            }
        }
    }
}
