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
		return Reg.All<T>(Namespace);
	}

	public T Get(string name) {
		return Reg.Get<T>(Namespace,name);
	}

	public bool Try(string name, out T item) {
		return Reg.Try(Namespace,name,out item);
	}

	internal abstract string Namespace { get; }
	IRegister Reg;
}
