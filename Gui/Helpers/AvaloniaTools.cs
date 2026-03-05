using Avalonia.Controls;
using Avalonia.Media;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ImageFunctions.Gui.Helpers;

public static class AvaloniaTools
{
	public static Control FindControlByNameFromTop(this Control parent, string name)
	{
		var top = TopLevel.GetTopLevel(parent);
		return FindControlByName(top, name);
	}

	public static Control FindControlByName(this Control parent, string name)
	{
		if(parent == null) {
			return null;
		}
		if(parent.Name == name) {
			return parent;
		}
		if(parent is Avalonia.LogicalTree.ILogical il) {
			foreach(var child in il.LogicalChildren) {
				if(child is Control cc) {
					var c = FindControlByName(cc, name);
					if(c != null) { return c; }
				}
			}
		}

		return null;
	}

	public static void WatchChildProperties<T>(this IReadOnlyCollection<T> sender,
		PropertyChangedEventHandler propertyHandler,
		NotifyCollectionChangedEventHandler collectionHandler)
	{
		if(sender is not INotifyCollectionChanged incc) {
			throw new ArgumentException($"{typeof(T).FullName} is not an observable collection");
		}

		incc.CollectionChanged += (sender, args) => {
			//Trace.WriteLine($"CollectionChanged a={args.Action} n={args.NewItems?.Count} o={args.OldItems?.Count}");
			if(args.NewItems != null) {
				foreach(T item in args.NewItems) {
					if(item is INotifyPropertyChanged inpc) {
						//Trace.WriteLine($"Watch += {inpc.GetType().FullName}");
						inpc.PropertyChanged += propertyHandler;
					}
				}
			}

			if(args.OldItems != null) {
				foreach(T item in args.OldItems) {
					if(item is INotifyPropertyChanged inpc) {
						//Trace.WriteLine($"Watch -= {inpc.GetType().FullName}");
						inpc.PropertyChanged -= propertyHandler;
					}
				}
			}

			collectionHandler?.Invoke(sender, args);
		};
	}

	public static Color ToColor(this Core.ColorRGBA c)
	{
		var ac = Color.FromArgb(
			(byte)(c.A * 255.0),
			(byte)(c.R * 255.0),
			(byte)(c.G * 255.0),
			(byte)(c.B * 255.0)
		);
		return ac;
	}

	public static StreamGeometry GetIconFromName(string name)
	{
		Avalonia.Application.Current.Resources.TryGetResource(name, null, out object icon);
		return (StreamGeometry)icon;
	}

	/// <summary>Splits a string into a sequence of strings based on whitespace and quotation marks.</summary>
	/// <param name="commandLine">A command line input string.</param>
	/// <returns>A sequence of strings.</returns>
	public static IEnumerable<string> SplitCommandLine(string commandLine)
	{
		//ChatGPT prompt:
		// 1: can you write a parser in c# that takes a string which is a command line for a program and parse it into args ? the parser should handle quotes and nested quotes and produce an IEnumerable<string> as the output
		// 2: instead of using args, curernt, and stack can you keep track of the indices of the start and end of the next token and yield return each token as the commandLine string is parsed ?

		if(commandLine == null) {
			Squeal.ArgumentNull(nameof(commandLine));
		}

		int length = commandLine.Length;
		int i = 0;

		while(i < length) {
			// Skip leading whitespace
			while(i < length && char.IsWhiteSpace(commandLine[i])) { i++; }
			if(i >= length) { yield break; }

			int start = i;
			bool activeQuote = false;

			while(i < length) {
				char c = commandLine[i];

				// Handle escaping for characters '\'
				if(c == '\\' && i + 1 < length) {
					i += 2;
					continue;
				}

				// Quote handling
				if(c == '"') {
					//swap inside or outside of quotes
					activeQuote = !activeQuote;
					i++;
					continue;
				}

				// If whitespace ends token (only if not inside quotes)
				if(char.IsWhiteSpace(c) && !activeQuote) {
					break;
				}

				i++;
			}

			// Extract raw token
			string token = commandLine.Substring(start, i - start);
			token = UnquoteAndUnescape(token);

			yield return token;
		}
	}

	static string UnquoteAndUnescape(string token)
	{
		// Remove surrounding matching quotes
		if(token.Length >= 2 &&
			((token[0] == '"' && token[^1] == '"') ||
			(token[0] == '\'' && token[^1] == '\''))) {
			token = token.Substring(1, token.Length - 2);
		}

		// Unescape \" \/ \\ \' inside token
		token = token
			.Replace("\\\"", "\"")
			.Replace("\\'", "'")
			.Replace("\\\\", "\\")
		;

		return token;
	}

	// This exists because Clear() doesn't fire the Remove Notification
	public static void RemoveDisposeAll<T>(this IList<T> itemList)
	{
		for(int i = itemList.Count - 1; i >= 0; i--) {
			itemList.RemoveDisposeAt(i);
		}
	}

	public static void RemoveDisposeAt<T>(this IList<T> itemList, int index)
	{
		var item = itemList[index];
		if(item is IDisposable id) {
			id.Dispose();
		}
		itemList.RemoveAt(index);
	}
}
