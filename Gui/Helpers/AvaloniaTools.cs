using Avalonia.Controls;
using DynamicData;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

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

	public static void WatchChildProperties<T>(this IReadOnlyCollection<T> sender, PropertyChangedEventHandler handler)
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
						inpc.PropertyChanged += handler;
					}
				}
			}
			
			if (args.OldItems != null && args.Action == NotifyCollectionChangedAction.Remove) {
				foreach(T item in args.OldItems) {
					if (item is INotifyPropertyChanged inpc) {
						//Trace.WriteLine($"Watch -= {inpc.GetType().FullName}");
						inpc.PropertyChanged -= handler;
					}
				}
			}
		};
	}

	public static Avalonia.Media.Color ToColor(this Core.ColorRGBA c)
	{
		var ac = Avalonia.Media.Color.FromArgb(
			(byte)(c.A * 255.0),
			(byte)(c.R * 255.0),
			(byte)(c.G * 255.0),
			(byte)(c.B * 255.0)
		);
		return ac;
	}

	// public static IObservable<TRet> WhenAnyChildValue<TSender, TRet>(this IReadOnlyCollection<TSender> sender, Expression<Func<TSender, TRet>> property1)
	// {
	// 	if (sender is not INotifyCollectionChanged incc) {
	// 		throw new ArgumentException($"{typeof(TSender).FullName} is not an observable collection");
	// 	}

	// 	Dictionary<TSender, IObservable<TRet>> oList = new();

	// 	incc.CollectionChanged += (sender, args) => {
	// 		if (args.OldItems != null) {
	// 			foreach(TSender item in args.OldItems) {
	// 				oList.Remove(item);
	// 			}
	// 		}

	// 		if (args.NewItems != null) {
	// 			foreach(TSender item in args?.NewItems) {
	// 				var o = item.WhenAnyValue(property1);
	// 				oList.Add(item, o);
	// 			}
	// 		}
	// 	};

	// 	var bco = new BroadcastObservable<TRet>(oList.Values);
	// 	return bco;
	// }

	// class BroadcastObservable<T> : IObservable<T>
	// {
	// 	public BroadcastObservable(IReadOnlyCollection<IObservable<T>> items)
	// 	{
	// 		this.Items = items;
	// 	}

	// 	readonly IReadOnlyCollection<IObservable<T>> Items;

	// 	public IDisposable Subscribe(IObserver<T> observer)
	// 	{
	// 		var cleanup = new List<IDisposable>();

	// 		foreach(var item in Items) {
	// 			var id = item.Subscribe(observer);
	// 			cleanup.Add(id);
	// 		}

	// 		return new MultiCleanup(cleanup);
	// 	}
	// }

	// class MultiCleanup : IDisposable
	// {
	// 	public MultiCleanup(List<IDisposable> list)
	// 	{
	// 		this.TheList = list;
	// 	}

	// 	readonly List<IDisposable> TheList;

	// 	public void Dispose()
	// 	{
	// 		if (TheList != null) {
	// 			foreach(var item in TheList) {
	// 				item?.Dispose();
	// 			}
	// 		}
	// 	}
	// }

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
	public static void RemoveAll<T>(this IList<T> itemList, bool disposeItems = false)
	{
		for(int i = itemList.Count - 1; i >= 0; i--) {
			if (disposeItems) {
				var item = itemList[i];
				if (item is IDisposable id) {
					id.Dispose();
				}
			}
			itemList.RemoveAt(i);
		}
	}
}
