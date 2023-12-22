using BepInEx;
using Settings = SettingsAPI.Plugin;
using BepInEx.Logging;
using UnityEngine;
using SettingsAPI;
using UnityEngine.UI;

namespace SpeedrunningUtils
{
    [BepInPlugin("tairasoul.vaproxy.speedrunning", "SpeedrunningUtils", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        internal static GameObject ColliderStorage;
        internal static bool VisualisingHitboxes = false;
        internal GameObject SettingsPage;
        private bool init = false;

        private void Awake()
        {
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
                DontDestroyOnLoad(ColliderStorage);
                DontDestroyOnLoad(Utils);
                Utils.AddComponent<SpeedrunnerUtils>();
                Log.LogInfo("SpeedrunningUtils initialized. Good luck speedrunning!");
                Option[] options = [];
                Option VisualizeOption = new()
                {
                    Create = (GameObject page) =>
                    {
                        GameObject Toggle = ComponentUtils.CreateToggle("Visualize Split Colliders", "speedrunning.visualise.splitcolliders");
                        Toggle.SetParent(page, false);
                        Toggle.GetComponent<Toggle>().onValueChanged.AddListener((bool enabled) =>
                        {
                            VisualisingHitboxes = enabled;
                        });
                    }
                };
                options = options.Append(VisualizeOption);
                Settings.API.RegisterMod("tairasoul.speedrunning.utils", "SpeedrunningUtils", options);
            }
        }
    }
}
