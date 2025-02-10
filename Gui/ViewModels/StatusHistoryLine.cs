using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using ImageFunctions.Core;

namespace ImageFunctions.Gui.ViewModels;

public sealed class StatusHistoryLine
{
	public StatusHistoryLine(string text, LogCategory category)
	{
		Text = text;
		Category = category;
		StatusClass = GetClassForCategory(category);
		InlineItems = new();
		CreateStatusRun(InlineItems, text, category);
	}

	public string Text { get; private set; }
	public LogCategory Category { get; private set ; }
	public string StatusClass { get; private set; }
	public InlineCollection InlineItems { get; private set; }

	public static string GetClassForCategory(LogCategory category)
	{
		var @class = category switch {
			LogCategory.Debug => "Tertiary",
			LogCategory.Info => "Secondary",
			LogCategory.Warning => "Warning",
			LogCategory.Error => "Danger",
			_ => "",
		};
		return @class;
	}

	static int DebugCounter = 0;
	public static void CreateStatusRun(InlineCollection inlinses, string text, LogCategory category)
	{
		Interlocked.Add(ref DebugCounter,1);
		var line = new Run($"{DebugCounter} {text}");
		if (
			category == LogCategory.Debug ||
			category == LogCategory.Info ||
			category == LogCategory.Warning ||
			category == LogCategory.Error
		) {
			var statusClass = GetClassForCategory(category);
			line.Classes.Add(statusClass);
		}
		inlinses.Add(line);

		if (_statusCategoryIconCache.TryGetValue(category, out var geometry)) {
			var icon = new PathIcon { Data = geometry };
			var inline = new InlineUIContainer(icon);
			inlinses.Add(inline);
		}
	}

	//creating this cache up-front to avoid cross-thread problems when rendering
	static readonly Dictionary<LogCategory, StreamGeometry> _statusCategoryIconCache = InitStatusCategoryIconCache();
	static Dictionary<LogCategory, StreamGeometry> InitStatusCategoryIconCache()
	{
		Dictionary<LogCategory, StreamGeometry> cache = new();
		cache.Add(LogCategory.Warning, GetIconForName("IconAlert"));
		cache.Add(LogCategory.Error, GetIconForName("IconAlertOctagram"));
		cache.Add(LogCategory.Debug, GetIconForName("IconDeveloperBoard"));
		cache.Add(LogCategory.Info, GetIconForName("IconInformationOutline"));
		return cache;
	}

	static StreamGeometry GetIconForName(string name)
	{
		Avalonia.Application.Current.Resources.TryGetResource(name, null, out object icon);
		return (StreamGeometry)icon;
	}
}