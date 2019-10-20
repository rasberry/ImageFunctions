using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Runtime.CompilerServices;

namespace test
{
	[TestClass]
	public class TestHelpers
	{
		[TestMethod]
		public void TestToTuple()
		{
			for(int i=1; i<100; i++)
			{
				var seq = Enumerable.Range(0,i);
				var tuple = seq.ToTuple();

				//unroll the nested tuples and check that everything matches
				int count = 0;
				ITuple curr = tuple;
				while(true)
				{
					for(int t=0; t<Math.Min(curr.Length,7); t++) {
						int val = (int)curr[t];
						Assert.AreEqual(count,val);
						count++;
					}
					if (curr.Length > 7) {
						curr = (ITuple)curr[7];
					}
					else {
						break;
					}
				}

				Assert.AreEqual(i,count);
			}
		}

		[TestMethod]
		public void TestAppend()
		{
			var list = new string[] { "1","2","3" };
			var more = Helpers.Append(list,"4","5");
			string test = string.Join(' ',more);
			Assert.AreEqual("1 2 3 4 5",test);
		}

		[TestMethod]
		public void TestCheckFile()
		{
			var one = Tuple.Create("one");
			string fone = Helpers.CheckFile(ImageFunctions.Activity.PixelateDetails,one,0,true);
			Assert.AreEqual("img/img-1-one-1.png",fone);

			var two = Tuple.Create("two","two");
			string ftwo = Helpers.CheckFile(ImageFunctions.Activity.PixelRules,two,1,true);
			Assert.AreEqual("img/img-9-two-two-2.png",ftwo);
		}

		[TestMethod]
		public void TestEnumerate()
		{
			var one = Tuple.Create("one");
			var eone = Helpers.Enumerate<string>(one);
			bool bone = eone.Any((s) => s != "one");
			Assert.IsFalse(bone);

			var three = Tuple.Create("three","three","three");
			var ethree = Helpers.Enumerate<string>(three);
			bool bthree = ethree.Any((s) => s != "three");
			Assert.IsFalse(bthree);
		}

		[TestMethod]
		public void TestInFile()
		{
			var one = Tuple.Create("one");
			ITuple fone = Helpers.InFile(one,true);
			Assert.AreEqual("img/one.png",(string)fone[0]);

			var two = Tuple.Create("two","two");
			ITuple ftwo = Helpers.InFile(two,true);
			Assert.AreEqual("img/two.png",(string)ftwo[0]);
			Assert.AreEqual("img/two.png",(string)ftwo[1]);
		}

		[TestMethod]
		public void TestTupleify()
		{
			var array = new int[] { 1,2,3,4,5 };
			var list = Helpers.Tupleify(array);
			Assert.AreEqual(array.Length,list.Length);

			for(int a=0; a<array.Length; a++) {
				int check = array[a];
				int test = (int)list[a][0];
				Assert.AreEqual(check,test);
			}
		}
	}
}