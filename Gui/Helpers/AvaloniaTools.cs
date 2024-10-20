using Avalonia.Controls;
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

	public static void WatchChildProperties<T>(this IReadOnlyCollection<T> sender, PropertyChangedEventHandler handler)
	{
		if(sender is not INotifyCollectionChanged incc) {
			throw new ArgumentException($"{typeof(T).FullName} is not an observable collection");
		}

		incc.CollectionChanged += (sender, args) => {
			if(args.NewItems != null) {
				foreach(T item in args?.NewItems) {
					if(item is INotifyPropertyChanged inpc) {
						inpc.PropertyChanged += handler;
					}
				}
			}
		};
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
}
