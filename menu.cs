using MainMenuSettings;
using MainMenuSettings.Extensions;
using tairasoul.unity.common.speedrunning.runtime;
using UnityEngine;
using UnityEngine.UI;

namespace speedrunningutils;

static class MenuImpl {
	static Text ActiveText;

	public static IEnumerable<ButtonOption> Buttons() {
		IEnumerable<FileEntry> files = RuntimeInterface.GetAvailableFiles();
		List<ButtonOption> options = [];
		options.Add(new()
		{
			Id = "tairasoul.speedrunningutils.livesplit.reconnect",
			Text = "Reconnect to Livesplit",
			Clicked = async () => {
				RuntimeInterface.livesplitInstance.Disconnect();
				await Task.Delay(50);
				RuntimeInterface.livesplitInstance.Connect();
			}
		});
		options.Add(new()
		{
			Id = "tairasoul.speedrunningutils.obs.reconnect",
			Text = "Reconnect to OBS",
			Clicked = () => {
				Plugin.obs.Close();
				Plugin.obs.Connect();
			}
		});
		// options.Add(new() {
			// Id = "tairasoul.speedrunningutils.obs.reconnect",
			// Text = "Close OBS socket",
			// Clicked = Plugin.obs.Close
		// });
		foreach (FileEntry entry in Flatten(files)) {
			options.Add(CreateFileButton(entry));
		}
		return options;
	}

	static ButtonOption CreateFileButton(FileEntry file) {
		ButtonOption opt = new()
		{
			Id = $"tairasoul.speedrunningutils.split.{file.name}",
			Text = $"Split file {file.name}",
			Clicked = () =>
			{
				Plugin.cfg.LastLoadedConfig.Value = file.relativePath;
				RuntimeInterface.Load(file);
				ActiveText.text = $"Active: {file.relativePath}";
			}
		};
		return opt;
	}

	static IEnumerable<FileEntry> Flatten(IEnumerable<FileEntry> root)
	{
		List<FileEntry> entries = [];
		foreach (FileEntry entry in root) {
			if (!entry.isDirectory) {
				entries.Add(entry);
			}
			else {
				foreach (FileEntry item in Flatten(entry.entries))
					entries.Add(item);
			}
		}
		return entries;
	}

	internal static void CreateActiveText(GameObject page) 
	{
		GameObject Text = new("ActiveText");
		RectTransform txtt = Text.AddComponent<RectTransform>();
		txtt.sizeDelta = new(220.05f, 91.79f);
		Text txt = Text.AddComponent<Text>();
		txt.text = $"Active: {Plugin.cfg.LastLoadedConfig.Value}";
		txt.font = MenuComponents.GetFont("Arial");
		txt.fontSize = 15;
		txt.alignment = TextAnchor.UpperLeft;
		Text.SetParent(page, true);
		txtt.anchoredPosition3D = new(192.1614f, 83.2671f, 0);
		txtt.localScale = new(1, 1, 1);
		txtt.localRotation = new(0, 0, 0, 1);
		ActiveText = txt;
	}
}