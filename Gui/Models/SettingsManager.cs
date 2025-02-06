using Rasberry.Cli;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImageFunctions.Gui.Models;

public class SettingsManager
{
	//using unix-y convention of starting hidden files with a dot.
	static readonly string SettingsFileName = $".{nameof(ImageFunctions)}-{nameof(Gui)}.conf".ToLowerInvariant();

	public void Load()
	{
		var path = GetFilePath();
		if(File.Exists(path)) {
			var text = File.ReadAllText(path);
			SettingsData = JsonSerializer.Deserialize<Dictionary<string, object>>(text, JsonOpts);
		}
	}

	public void Save()
	{
		var path = GetFilePath();
		var text = JsonSerializer.Serialize(SettingsData, JsonOpts);
		Program.Log.Debug($"{nameof(SettingsManager)} Saving {path} json={text}");
		File.WriteAllText(path, text);
	}

	public bool TryGet(string name, out string val)
	{
		val = null;
		bool w = SettingsData.TryGetValue(name, out object oval);
		if(w) { val = oval.ToString(); }
		return w;
	}

	public bool TryGetAs<T>(string name, out T val)
	{
		val = default;
		bool w = SettingsData.TryGetValue(name, out object oval);
		if(w && oval != null) {
			if(typeof(T).IsAssignableFrom(oval.GetType())) {
				val = (T)oval;
				return true;
			}
			else {
				try {
					val = Parser.Parse<T>(oval.ToString());
					return true;
				}
				catch {
					//nothing to do
				}
			}
		}
		return false;
	}

	public void Set<T>(string name, T val)
	{
		//Trace.WriteLine($"{nameof(SettingsManager)} Set n={name} v={val}");
		SettingsData[name] = val;
	}

	string GetFilePath()
	{
		string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		string path = Path.Combine(folder, SettingsFileName);
		return path;
	}

	static readonly DefaultParser Parser = new();
	static readonly JsonSerializerOptions JsonOpts = new() {
		AllowTrailingCommas = true,
		ReadCommentHandling = JsonCommentHandling.Skip,
		WriteIndented = true,
		ReferenceHandler = ReferenceHandler.IgnoreCycles
	};
	Dictionary<string, object> SettingsData = new();
}
