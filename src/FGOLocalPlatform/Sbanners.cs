using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace FGOLocalPlatform;

public partial class Sbanners : UserControl
{
	private const string PatchMarker = "fgo_event_toggles_v1";

	private readonly Dictionary<string, CheckBox> bannerOptions = new();

	private string ServerRoot => Path.GetFullPath(Path.Combine(GamePaths.GameRoot, "..", "Server"));

	private string ServerYamlPath => Path.GetFullPath(Path.Combine(ServerRoot, "artemis", "config", "fgo.yaml"));

	public Sbanners()
	{
		InitializeComponent();
		bannerOptions["HideBanners"] = HideBanners;
		bannerOptions["LTE8008"] = LTE8008;
		bannerOptions["LTE8010"] = LTE8010;
		bannerOptions["LTE8011"] = LTE8011;
		bannerOptions["LTE8012"] = LTE8012;
		bannerOptions["LTE8013"] = LTE8013;
		bannerOptions["LTE8014"] = LTE8014;
		bannerOptions["LTE8015"] = LTE8015;
		bannerOptions["LTE8016"] = LTE8016;
		bannerOptions["LTE8017"] = LTE8017;
		bannerOptions["LTE8018"] = LTE8018;
		bannerOptions["LTE8019"] = LTE8019;
		bannerOptions["LTE8020"] = LTE8020;
		bannerOptions["LTE8021"] = LTE8021;
		bannerOptions["LTE8022"] = LTE8022;
		bannerOptions["LTE8023"] = LTE8023;
		bannerOptions["LTE8024"] = LTE8024;
		bannerOptions["LTE8025"] = LTE8025;
		bannerOptions["LTE8026"] = LTE8026;
		bannerOptions["LTE8027"] = LTE8027;
		bannerOptions["LTE8028"] = LTE8028;
		bannerOptions["LTE8029"] = LTE8029;
		bannerOptions["LTE8030"] = LTE8030;
		bannerOptions["LTE8031"] = LTE8031;
		bannerOptions["LTE8032"] = LTE8032;
		bannerOptions["LTE8033"] = LTE8033;
		bannerOptions["LTE8034"] = LTE8034;
		bannerOptions["LTE8035"] = LTE8035;
		bannerOptions["LTE8036"] = LTE8036;
		bannerOptions["LTE8037"] = LTE8037;
		bannerOptions["LTE8038"] = LTE8038;
		bannerOptions["LTE8039"] = LTE8039;
		bannerOptions["LTE8040"] = LTE8040;
		bannerOptions["LTE8041"] = LTE8041;
		bannerOptions["LTE8042"] = LTE8042;
		bannerOptions["LTE8043"] = LTE8043;
		bannerOptions["LTE8044"] = LTE8044;
		bannerOptions["LTE8045"] = LTE8045;
		bannerOptions["LTE8046"] = LTE8046;
		bannerOptions["LTE8047"] = LTE8047;
		bannerOptions["LTE8048"] = LTE8048;
		bannerOptions["LTE8049"] = LTE8049;
	}

	private void Option_OnChanged(object sender, RoutedEventArgs e)
	{
		StatusText.Text = "Banner settings updated.";
	}

	public void Save()
	{
		try
		{
			WriteEnabledSingularityIds();
			StatusText.Text = "Banner settings saved.";
		}
		catch (Exception ex)
		{
			StatusText.Text = "Could not save the banner settings: " + ex.Message;
		}
	}

	private void Save_OnClick(object sender, RoutedEventArgs e)
	{
		Save();
	}

	private void DisableAll_OnClick(object sender, RoutedEventArgs e)
	{
		foreach (CheckBox checkBox in bannerOptions.Values)
		{
			if (checkBox != null)
			{
				checkBox.IsChecked = false;
			}
		}
		StatusText.Text = "All banners turned off.";
	}

	private void ApplyEventPatch_OnClick(object sender, RoutedEventArgs e)
	{
		try
		{
			StatusText.Text = ApplyEventTogglePatch();
		}
		catch (Exception ex)
		{
			StatusText.Text = ex.Message;
		}
	}

	private string ApplyEventTogglePatch()
	{
		string patchPath = ResolvePatchFile();
		if (!File.Exists(patchPath))
		{
			return "Patch file not found: " + patchPath;
		}

		string patchText = StripJsonComments(File.ReadAllText(patchPath));
		using JsonDocument document = JsonDocument.Parse(patchText);
		JsonElement root = document.RootElement;
		if (!root.TryGetProperty("edits", out JsonElement editsElement) || editsElement.ValueKind != JsonValueKind.Array)
		{
			throw new InvalidOperationException("The event toggle patch file is missing the edits array.");
		}

		List<string> messages = new List<string>();
		foreach (JsonElement editElement in editsElement.EnumerateArray())
		{
			string fileName = editElement.TryGetProperty("file", out JsonElement fileElement) ? fileElement.GetString() : null;
			string anchorOld = editElement.TryGetProperty("anchor_old", out JsonElement oldElement) ? oldElement.GetString() : null;
			string anchorNew = editElement.TryGetProperty("anchor_new", out JsonElement newElement) ? newElement.GetString() : null;
			if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(anchorOld) || string.IsNullOrEmpty(anchorNew))
			{
				throw new InvalidOperationException("One of the patch entries is missing file, anchor_old, or anchor_new.");
			}

			string targetPath = ResolvePatchTarget(fileName);
			if (!File.Exists(targetPath))
			{
				throw new InvalidOperationException("This file doesn't match what this patch expects — likely the fork's been updated since this version of Scooby was built. Missing file: " + targetPath);
			}

			string currentText = File.ReadAllText(targetPath);
			if (currentText.Contains(anchorNew, StringComparison.Ordinal) || currentText.Contains(PatchMarker, StringComparison.OrdinalIgnoreCase))
			{
				messages.Add(Path.GetFileName(targetPath) + " already applied.");
				continue;
			}

			int oldCount = CountExactOccurrences(currentText, anchorOld);
			if (oldCount != 1)
			{
				throw new InvalidOperationException("This file doesn't match what this patch expects — likely the fork's been updated since this version of Scooby was built. File: " + targetPath);
			}

			string backupPath = targetPath + ".bak-before-" + PatchMarker;
			if (!File.Exists(backupPath))
			{
				File.Copy(targetPath, backupPath);
			}

			string updatedText = currentText.Replace(anchorOld, anchorNew, StringComparison.Ordinal);
			File.WriteAllText(targetPath, updatedText);
			ValidatePythonFile(targetPath, backupPath);
			messages.Add("Applied to " + Path.GetFileName(targetPath));
		}

		return string.Join(Environment.NewLine, messages.Count > 0 ? messages : new[] { "No event-toggle changes were needed." });
	}

	private void WriteEnabledSingularityIds()
	{
		string path = ServerYamlPath;
		if (!File.Exists(path))
		{
			throw new IOException("The server config file was not found: " + path);
		}

		List<int> selectedIds = bannerOptions
			.Where(pair => pair.Value != null && pair.Value.IsChecked == true && pair.Key.StartsWith("LTE", StringComparison.Ordinal))
			.Select(pair => int.Parse(pair.Key.Substring(3), System.Globalization.CultureInfo.InvariantCulture))
			.OrderBy(id => id)
			.ToList();

		string valueText = selectedIds.Count == 0 ? "null" : "[" + string.Join(", ", selectedIds) + "]";
		string[] lines = File.ReadAllLines(path);
		bool found = false;
		int serverStart = -1;
		int serverEnd = lines.Length;
		for (int i = 0; i < lines.Length; i++)
		{
			string trimmed = lines[i].Trim();
			if (trimmed == "server:")
			{
				serverStart = i;
				break;
			}
		}
		if (serverStart < 0)
		{
			throw new InvalidOperationException("The server: block could not be found in fgo.yaml.");
		}
		for (int i = serverStart + 1; i < lines.Length; i++)
		{
			string trimmed = lines[i].Trim();
			if (trimmed.Length > 0 && !trimmed.StartsWith("#") && !lines[i].StartsWith("  ") && !lines[i].StartsWith("\t"))
			{
				serverEnd = i;
				break;
			}
		}
		for (int i = serverStart + 1; i < serverEnd; i++)
		{
			if (lines[i].TrimStart().StartsWith("enabled_singularity_ids:", StringComparison.Ordinal))
			{
				lines[i] = "  enabled_singularity_ids: " + valueText;
				found = true;
				break;
			}
		}
		if (!found)
		{
			List<string> expanded = new List<string>(lines.Length + 2);
			expanded.AddRange(lines.Take(serverEnd));
			expanded.Add("  enabled_singularity_ids: " + valueText);
			expanded.AddRange(lines.Skip(serverEnd));
			lines = expanded.ToArray();
		}
		File.WriteAllText(path, string.Join(Environment.NewLine, lines) + Environment.NewLine);
	}

	private string ResolvePatchFile()
	{
		string baseDirectory = AppContext.BaseDirectory;
		List<string> candidates = new List<string>
		{
			Path.Combine(baseDirectory, "..", "..", "..", "..", "patch", "scooby_fgo_events_patch.json"),
			Path.Combine(baseDirectory, "..", "..", "..", "patch", "scooby_fgo_events_patch.json"),
			Path.Combine(baseDirectory, "..", "..", "patch", "scooby_fgo_events_patch.json"),
			Path.Combine(baseDirectory, "..", "patch", "scooby_fgo_events_patch.json"),
			Path.Combine(Directory.GetCurrentDirectory(), "patch", "scooby_fgo_events_patch.json"),
			Path.Combine(ServerRoot, "..", "patch", "scooby_fgo_events_patch.json")
		};

		foreach (string candidate in candidates)
		{
			string fullPath = Path.GetFullPath(candidate);
			if (File.Exists(fullPath))
			{
				return fullPath;
			}
		}

		return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "patch", "scooby_fgo_events_patch.json"));
	}

	private string ResolvePatchTarget(string fileName)
	{
		string[] candidates = new[]
		{
			Path.Combine(ServerRoot, "artemis", "titles", "fgo", fileName),
			Path.Combine(ServerRoot, "artemis", fileName),
			Path.Combine(ServerRoot, fileName),
			Path.Combine(GamePaths.GameRoot, fileName),
			Path.Combine(GamePaths.GameRoot, "..", fileName)
		};

		foreach (string candidate in candidates)
		{
			string fullPath = Path.GetFullPath(candidate);
			if (File.Exists(fullPath))
			{
				return fullPath;
			}
		}

		return Path.GetFullPath(Path.Combine(ServerRoot, "artemis", "titles", "fgo", fileName));
	}

	private static int CountExactOccurrences(string text, string needle)
	{
		if (string.IsNullOrEmpty(needle))
		{
			return 0;
		}

		int count = 0;
		int index = 0;
		while ((index = text.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
		{
			count++;
			index += needle.Length;
		}
		return count;
	}

	private static string StripJsonComments(string text)
	{
		StringBuilder builder = new StringBuilder();
		bool inString = false;
		bool escaped = false;
		for (int i = 0; i < text.Length; i++)
		{
			char ch = text[i];
			if (inString)
			{
				builder.Append(ch);
				if (escaped)
				{
					escaped = false;
				}
				else if (ch == '\\')
				{
					escaped = true;
				}
				else if (ch == '"')
				{
					inString = false;
				}
				continue;
			}

			if (ch == '"')
			{
				inString = true;
				builder.Append(ch);
				continue;
			}

			if (ch == '/' && i + 1 < text.Length)
			{
				char next = text[i + 1];
				if (next == '/')
				{
					while (i + 1 < text.Length && text[i + 1] != '\n')
					{
						i++;
					}
					continue;
				}
				if (next == '*')
				{
					i += 2;
					while (i + 1 < text.Length)
					{
						if (text[i] == '*' && text[i + 1] == '/')
						{
							i++;
							break;
						}
						i++;
					}
					continue;
				}
			}

			builder.Append(ch);
		}
		return builder.ToString();
	}

	private static void ValidatePythonFile(string path, string backupPath)
	{
		string python = Path.GetFullPath(Path.Combine(GamePaths.GameRoot, "..", "Server", "python", "python.exe"));
		if (!File.Exists(python))
		{
			return;
		}

		ProcessStartInfo startInfo = new ProcessStartInfo(python)
		{
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			WorkingDirectory = Path.GetDirectoryName(path) ?? AppContext.BaseDirectory
		};
		startInfo.ArgumentList.Add("-m");
		startInfo.ArgumentList.Add("py_compile");
		startInfo.ArgumentList.Add(path);

		using Process process = Process.Start(startInfo) ?? throw new IOException("Could not run Python validation for the patched file.");
		string error = process.StandardError.ReadToEnd();
		process.WaitForExit();
		if (process.ExitCode != 0)
		{
			if (File.Exists(backupPath))
			{
				File.Copy(backupPath, path, overwrite: true);
			}
			throw new InvalidOperationException("The patched Python file failed validation and has been restored from backup. Error: " + error.Trim());
		}
	}
}
