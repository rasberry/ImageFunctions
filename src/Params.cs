using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageFunctions
{
	public sealed class Params
	{
		public enum Result {
			Missing = 0,
			Invalid = 1,
			Good = 2
		}

		public delegate bool Parser<T>(string inp, out T val);

		public Params(string[] args)
		{
			Args = new List<string>(args);
		}

		List<string> Args;

		public string[] Remaining()
		{
			return Args.ToArray();
		}

		// check for existance of a single parameter
		public Result Has(params string[] @switch)
		{
			int ii = -1;
			foreach(string sw in @switch) {
				int i = Args.IndexOf(sw);
				if (i != -1) {
					Args.RemoveAt(i);
					ii = i;
				}
			}
			return ii == -1 ? Result.Missing : Result.Good;
		}

		// check for a non-qualified (leftover) parameter
		public Result Default<T>(out T val, T def = default(T),Parser<T> par = null)
		{
			val = def;
			if (Args.Count <= 0) { return Result.Missing; }
			string curr = Args[0];
			if (par == null) { par = TryParse; }
			if (!par(curr,out val)) {
				return Result.Invalid;
			}
			Args.RemoveAt(0);
			return Result.Good;
		}

		//find or default a parameter with one argument
		public Result Default<T>(string @switch,out T val,T def = default(T),Parser<T> par = null)
		{
			val = def;
			int i = Args.IndexOf(@switch);
			if (i == -1) {
				return Result.Missing;
			}
			if (i+1 >= Args.Count) {
				Tell.MissingArgument(@switch);
				return Result.Invalid;
			}
			if (par == null) { par = TryParse; }
			if (!par(Args[i+1],out val)) {
				Tell.CouldNotParse(@switch,Args[i+1]);
				return Result.Invalid;
			}
			Args.RemoveAt(i+1);
			Args.RemoveAt(i);
			return Result.Good;
		}

		public Result Default<T>(string[] @switch,out T val,T def = default(T),Parser<T> par = null)
		{
			val = default(T);
			Result rr = Result.Missing;
			foreach(string sw in @switch) {
				var r = Default<T>(sw,out val,def,par);
				if (r == Result.Invalid) { return r; }
				if (r == Result.Good) { rr = r; }
			}
			return rr;
		}

		//find or default a parameter with two arguments
		//Condition function determines when second argument is required (defaults to always true)
		public Result Default<T,U>(string @switch,out T tval, out U uval,
			T tdef = default(T), U udef = default(U), Func<T,bool> Cond = null,
			Parser<T> tpar = null, Parser<U> upar = null)
			where T : IConvertible where U : IConvertible
		{
			tval = tdef;
			uval = udef;
			int i = Args.IndexOf(@switch);
			if (i == -1) {
				return Result.Missing;
			}
			if (i+1 >= Args.Count) {
				Tell.MissingArgument(@switch);
				return Result.Invalid;
			}
			if (tpar == null) { tpar = TryParse; }
			if (!tpar(Args[i+1],out tval)) {
				Tell.CouldNotParse(@switch,Args[i+1]);
				return Result.Invalid;
			}

			//if condition function returns false - we don't look for a second arg
			if (Cond != null && !Cond(tval)) {
				Args.RemoveAt(i+1);
				Args.RemoveAt(i);
				return Result.Good;
			}

			if (i+2 >= Args.Count) {
				Tell.MissingArgument(@switch);
				return Result.Invalid;
			}
			if (upar == null) { upar = TryParse; }
			if (!upar(Args[i+2],out uval)) {
				Tell.CouldNotParse(@switch,Args[i+2]);
				return Result.Invalid;
			}
			Args.RemoveAt(i+2);
			Args.RemoveAt(i+1);
			Args.RemoveAt(i);
			return Result.Good;
		}

		public Result Expect<T>(out T val, string name)
		{
			if (Result.Good != Default(out val)) {
				Tell.MustProvideInput(name);
				return Result.Invalid;
			}
			return Result.Good;
		}

		public Result Expect(string @switch)
		{
			var has = Has(@switch);
			if (Result.Good != has) {
				if (has == Result.Missing) {
					Tell.MustProvideInput(@switch);
				}
				return Result.Invalid;
			}
			return Result.Good;
		}

		public Result Expect<T>(string @switch, out T val,Parser<T> par = null) where T : IConvertible
		{
			var has = Default(@switch,out val, par:par);
			if (Result.Good != has) {
				if (has == Result.Missing) {
					Tell.MustProvideInput(@switch);
				}
				return Result.Invalid;
			}
			return Result.Good;
		}

		public Result Expect<T,U>(string @switch, out T tval, out U uval,Parser<T> tpar = null,Parser<U> upar = null)
			where T : IConvertible where U : IConvertible
		{
			var has = Default(@switch,out tval,out uval, tpar:tpar, upar:upar);
			if (Result.Good != has) {
				if (has == Result.Missing) {
					Tell.MustProvideInput(@switch);
				}
				return Result.Invalid;
			}
			return Result.Good;
		}

		// consolidated the tryparse here - trying to make the code a bit more portable
		bool TryParse<T>(string item, out T val)
		{
			bool worked = Helpers.OptionsHelpers.TryParse(item,out val);
			//Log.Debug($"parse {item} as {typeof(T).Name} = {worked}");
			return worked;
		}
	}

	public static class ParamsExtensions
	{
		public static bool IsGood(this Params.Result r)
		{
			return r == Params.Result.Good;
		}

		public static bool IsBad(this Params.Result r)
		{
			return r != Params.Result.Good;
		}

		public static bool IsInvalid(this Params.Result r)
		{
			return r == Params.Result.Invalid;
		}

		public static bool IsMissing(this Params.Result r)
		{
			return r == Params.Result.Missing;
		}

	}
}