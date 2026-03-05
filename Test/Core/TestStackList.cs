using ImageFunctions.Core;

namespace ImageFunctions.Test.Core;

// dotnet test --filter "ClassName~TestStackList"

[TestClass]
public class TestStackList
{
	[TestMethod]
	public void PushPop()
	{
		var stack = new StackList<char>();
		stack.Push('A');
		Assert.AreEqual(1, stack.Count);
		Assert.AreEqual('A', stack[0]);

		stack.Push('B');
		Assert.AreEqual(2, stack.Count);
		Assert.AreEqual('B', stack[0]);

		char p1 = stack.Pop();
		Assert.AreEqual(1, stack.Count);
		Assert.AreEqual('A', stack[0]);
		Assert.AreEqual('B', p1);

		char p2 = stack.Pop();
		Assert.AreEqual(0, stack.Count);
		Assert.AreEqual('A', p2);
	}

	[TestMethod]
	public void AddRange()
	{
		var stack = new StackList<char>();
		//order will be reversed since we're pushing each element
		stack.AddRange(new[] { 'A', 'B', 'C', 'D' });

		//enumerable iterates from top of stack down.
		var test = new char[] { 'D', 'C', 'B', 'A' };
		CollectionAssert.AreEqual(test, stack);
	}

	[TestMethod]
	public void Move()
	{
		var stack = new StackList<char>();
		//order will be reversed since we're pushing each element
		stack.AddRange(new[] { 'A', 'B', 'C', 'D' });
		stack.Move(2, 1);

		var test1 = new char[] { 'D', 'B', 'C', 'A' };
		CollectionAssert.AreEqual(test1, stack);

		stack.Move(3, 1);

		var test2 = new char[] { 'D', 'A', 'B', 'C' };
		CollectionAssert.AreEqual(test2, stack);

		stack.Move(0, 2);

		var test3 = new char[] { 'A', 'B', 'D', 'C' };
		CollectionAssert.AreEqual(test3, stack);
	}

	[TestMethod]
	public void PushAtPopAt()
	{
		var stack = new StackList<char>();
		//order will be reversed since we're pushing each element
		stack.AddRange(new[] { 'A', 'B', 'C', 'D' });
		stack.PushAt(2, 'E');

		var test1 = new char[] { 'D', 'C', 'E', 'B', 'A' };
		CollectionAssert.AreEqual(test1, stack);

		var c = stack.PopAt(1);

		Assert.AreEqual('C', c);
		var test2 = new char[] { 'D', 'E', 'B', 'A' };
		CollectionAssert.AreEqual(test2, stack);
	}

	[TestMethod]
	public void Indexer()
	{
		var stack = new StackList<char>();
		//order will be reversed since we're pushing each element
		stack.AddRange(new[] { 'A', 'B', 'C', 'D' });

		Assert.AreEqual('D', stack[0]);
		Assert.AreEqual('C', stack[1]);
		Assert.AreEqual('B', stack[2]);
		Assert.AreEqual('A', stack[3]);

		stack[1] = 'E';
		Assert.AreEqual('E', stack[1]);
	}

	[TestMethod]
	public void IListMethods()  // tests for interface functions since Avalonia is using those
	{
		var stack = new StackList<char>();
		System.Collections.IList list = stack;
		list.Add('A');
		list.Add('B');
		list.Add('C');
		list.Add('D');

		Assert.HasCount(4, list);
		// should come out in stack order (reversed)
		Assert.AreEqual('D', list[0]);
		Assert.AreEqual('C', list[1]);
		Assert.AreEqual('B', list[2]);
		Assert.AreEqual('A', list[3]);

#pragma warning disable MSTEST0037 // We want to test the list contains not the Assert version
		Assert.IsTrue(list.Contains('A'));
		Assert.IsTrue(list.Contains('D'));
#pragma warning restore MSTEST0037

		//test GetEnumerator
		CollectionAssert.AreEqual(new char[] { 'D', 'C', 'B', 'A' }, list);

		var temp = new char[list.Count];
		list.CopyTo(temp, 0);
		// should come out in stack order (reversed)
		CollectionAssert.AreEqual(new char[] { 'D', 'C', 'B', 'A' }, temp);

		list.Insert(4, 'E');
		Assert.HasCount(5, list);
		list.Remove('E');
		Assert.HasCount(4, list);

		list.Add('E');
		Assert.HasCount(5, list);
		list.RemoveAt(0);
		Assert.HasCount(4, list);

		Assert.AreEqual(0, list.IndexOf('D'));
		Assert.AreEqual(1, list.IndexOf('C'));
		Assert.AreEqual(2, list.IndexOf('B'));
		Assert.AreEqual(3, list.IndexOf('A'));

		Assert.IsFalse(list.IsFixedSize);
		Assert.IsFalse(list.IsReadOnly);
		Assert.IsFalse(list.IsSynchronized);
	}
}
