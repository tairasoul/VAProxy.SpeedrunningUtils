using BepInEx.Logging;

namespace speedrunningutils.impls;

public static class ExtraMethods {
	static ManualLogSource logSource = BepInEx.Logging.Logger.CreateLogSource("speedrunningutils-extramethods");
	public static void LogStr(string str) {
		logSource.LogInfo(str);
	}
}