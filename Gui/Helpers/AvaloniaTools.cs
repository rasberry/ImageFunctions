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

	// https://github.com/dotnet/command-line-api/blob/main/src/System.CommandLine/Parsing/CliParser.cs#L40

	/// <summary>Splits a string into a sequence of strings based on whitespace and quotation marks.</summary>
	/// <param name="commandLine">A command line input string.</param>
	/// <returns>A sequence of strings.</returns>
	public static IEnumerable<string> SplitCommandLine(string commandLine)
	{
		var memory = commandLine.AsMemory();
		var startTokenIndex = 0;
		var pos = 0;

		var seeking = Boundary.TokenStart;
		var seekingQuote = Boundary.QuoteStart;

		while(pos < memory.Length) {
			var c = memory.Span[pos];
			if(char.IsWhiteSpace(c)) {
				if(seekingQuote == Boundary.QuoteStart) {
					switch(seeking) {
					case Boundary.WordEnd:
						yield return CurrentToken();
						startTokenIndex = pos;
						seeking = Boundary.TokenStart;
						break;

					case Boundary.TokenStart:
						startTokenIndex = pos;
						break;
					}
				}
			}
			else if(c == '\"') {
				if(seeking == Boundary.TokenStart) {
					switch(seekingQuote) {
					case Boundary.QuoteEnd:
						yield return CurrentToken();
						startTokenIndex = pos;
						seekingQuote = Boundary.QuoteStart;
						break;

					case Boundary.QuoteStart:
						startTokenIndex = pos + 1;
						seekingQuote = Boundary.QuoteEnd;
						break;
					}
				}
				else {
					switch(seekingQuote) {
					case Boundary.QuoteEnd:
						seekingQuote = Boundary.QuoteStart;
						break;

					case Boundary.QuoteStart:
						seekingQuote = Boundary.QuoteEnd;
						break;
					}
				}
			}
			else if(seeking == Boundary.TokenStart && seekingQuote == Boundary.QuoteStart) {
				seeking = Boundary.WordEnd;
				startTokenIndex = pos;
			}

			Advance();

			if(IsAtEndOfInput()) {
				switch(seeking) {
				case Boundary.TokenStart:
					break;
				default:
					yield return CurrentToken();
					break;
				}
			}
		}

		string CurrentToken()
		{
			return memory.Slice(startTokenIndex, IndexOfEndOfToken()).ToString().Replace("\"", "");
		}

		void Advance() { pos++; }
		int IndexOfEndOfToken() { return pos - startTokenIndex; }
		bool IsAtEndOfInput() { return pos == memory.Length; }
	}

	enum Boundary
	{
		TokenStart,
		WordEnd,
		QuoteStart,
		QuoteEnd
	}

	// This exists because Clear() doesn't remove fire the Remove Notification
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
