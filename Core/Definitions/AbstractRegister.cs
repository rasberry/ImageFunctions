using System.Collections.Generic;

namespace ImageFunctions.Core;

public abstract class AbstractRegistrant<T> : IRegistrant<T>
{
	public AbstractRegistrant(IRegister register) {
		Reg = register;
	}

	public void Add(string name, T item) {
		Reg.Add(Namespace,name,item);
	}

	public IEnumerable<string> All() {
		foreach(var i in Reg.All()) {
			if (i.StartsWith(Namespace)) {
				yield return StripPrefix(i,Namespace);
			}
		}
	}

	public T Get(string name) {
		return Reg.Get<T>(Namespace,name);
	}

	public bool Try(string name, out T item) {
		return Reg.Try(Namespace,name,out item);
	}

	static string StripPrefix(string text, string prefix) {
		//the + 1 is to remove the extra '.'
		return text.StartsWith(prefix) ? text.Substring(prefix.Length + 1) : text;
	}

	internal abstract string Namespace { get; }
	IRegister Reg;
}
