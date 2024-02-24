using ImageFunctions.Core;

namespace ImageFunctions.Test;

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
		stack.AddRange(new[] { 'A','B','C','D' });

		//enumerable iterates from top of stack down.
		var test = new char[] { 'D','C','B','A' };
		Assert.IsTrue(test.SequenceEqual(stack));
	}

	[TestMethod]
	public void Move()
	{
		var stack = new StackList<char>();
		//order will be reversed since we're pushing each element
		stack.AddRange(new[] { 'A','B','C','D' });
		stack.Move(2,1);

		var test1 = new char[] { 'D','B','C','A' };
		Assert.IsTrue(test1.SequenceEqual(stack));

		stack.Move(3,1);

		var test2 = new char[] { 'D','A','B','C' };
		Assert.IsTrue(test2.SequenceEqual(stack));
	}

	[TestMethod]
	public void PushAtPopAt()
	{
		var stack = new StackList<char>();
		//order will be reversed since we're pushing each element
		stack.AddRange(new[] { 'A','B','C','D' });
		stack.PushAt(2,'E');

		var test1 = new char[] { 'D','C','E','B','A' };
		Assert.IsTrue(test1.SequenceEqual(stack));

		var c = stack.PopAt(1);

		Assert.AreEqual('C',c);
		var test2 = new char[] { 'D','E','B','A' };
		Assert.IsTrue(test2.SequenceEqual(stack));
	}

	[TestMethod]
	public void Indexer()
	{
		var stack = new StackList<char>();
		//order will be reversed since we're pushing each element
		stack.AddRange(new[] { 'A','B','C','D' });

		Assert.AreEqual('D',stack[0]);
		Assert.AreEqual('C',stack[1]);
		Assert.AreEqual('B',stack[2]);
		Assert.AreEqual('A',stack[3]);

		stack[1] = 'E';
		Assert.AreEqual('E',stack[1]);
	}
}
