using BepInEx.Configuration;
using UnityEngine;

namespace speedrunningutils;

class Config {
	internal ConfigEntry<bool> VisualizeHitboxesByDefault;
	internal ConfigEntry<string> LastLoadedConfig;
	internal ConfigEntry<KeyboardShortcut> RestartKey;
	internal ConfigEntry<string> WebsocketPassword;
	internal ConfigEntry<string> WebsocketURL;
	internal ConfigEntry<int> WebsocketPort;
	internal ConfigEntry<bool> EnableOBSWebsocket;
	internal ConfigEntry<bool> UseTCP;

	public Config(ConfigFile file) {
		VisualizeHitboxesByDefault = file.Bind("Speedrunning", "Visualise split bounds by default", true, "Should a split's bounds be visualised by default?");
		LastLoadedConfig = file.Bind("Speedrunning", "Last loaded config", "", "The config last loaded by SpeedrunningUtils.");
		RestartKey = file.Bind("Keybinds", "Restart keybind", new KeyboardShortcut(KeyCode.P), "Keybind to restart from the beginning of the game.");
		EnableOBSWebsocket = file.Bind("OBS Integration", "Enable", false, "Enable OBS integration. Starts recording when you enter the save menu, stops 3 seconds after the run ends.");
		WebsocketPassword = file.Bind("OBS Integration", "Password", "", "The password for the OBS Websocket Server. Leave empty if no password.");
		WebsocketURL = file.Bind("OBS Integration", "Websocket URL", "127.0.0.1", "The URL for the server. Leave empty if you haven't changed anything.");
		WebsocketPort = file.Bind("OBS Integration", "Websocket Port", 4455, "The port the websocket server is listening on.");
		UseTCP = file.Bind("Livesplit Socket", "Use TCP", true, "Should the socket use TCP instead of named pipes?");
	}
}