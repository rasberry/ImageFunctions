using System;
using System.Text;

namespace ImageFunctions.Maze
{
	public static class Aids
	{
		public static PickWall Opposite(this PickWall w)
		{
			switch(w) {
				case PickWall.N: return PickWall.S;
				case PickWall.E: return PickWall.W;
				case PickWall.S: return PickWall.N;
				case PickWall.W: return PickWall.E;
			}
			return PickWall.None;
		}

		public static PickWall AddFlag(this PickWall w, PickWall f)
		{
			return w |= f;
		}

		public static PickWall CutFlag(this PickWall w, PickWall f)
		{
			return w &= ~f & PickWall.All;
		}

		public static String MazeToString(this IMaze m)
		{
			var sa = new StringBuilder();
			for(int y=0; y<m.CellsHigh; y++) {
				var st = new StringBuilder();
				var sm = new StringBuilder();
				var sb = new StringBuilder();
				for(int x=0; x<m.CellsWide; x++) {
					bool b = m.IsBlocked(x,y,PickWall.None);
					bool n = m.IsBlocked(x,y,PickWall.N);
					bool e = m.IsBlocked(x,y,PickWall.E);
					bool s = m.IsBlocked(x,y,PickWall.S);
					bool w = m.IsBlocked(x,y,PickWall.W);
					st.Append(' ')      .Append(n?'-':' ').Append(' ');
					sm.Append(w?'|':' ').Append(b?'C':'*').Append(e?'|':' ');
					sb.Append(' ')      .Append(s?'-':' ').Append(' ');
				}
				sa.AppendLine(st.ToString());
				sa.AppendLine(sm.ToString());
				sa.AppendLine(sb.ToString());
			}
			return sa.ToString();
		}
	}
}
