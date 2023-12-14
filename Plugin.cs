using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace SpeedrunningUtils
{
    [BepInPlugin("tairasoul.vaproxy.speedrunning", "SpeedrunningUtils", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private static ConfigEntry<KeyCode> ResetKey;
        public static ManualLogSource Log;
        private static GameObject ColliderStorage;
        private bool init = false;

        private void Awake()
        {
            ResetKey = Config.Bind("SpeedrunnerUtils", "ResetKey", KeyCode.Alpha0, "What key to press in order to restart.");
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
            }
        }
    }
}
