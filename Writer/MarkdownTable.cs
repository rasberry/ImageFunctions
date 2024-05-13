using System;
using System.Collections.Generic;
using System.Text;

namespace ImageFunctions.Writer;

public class MarkdownTable
{
	public void AddRow(IEnumerable<string> cells = null)
	{
		var row = new List<string>();
		if (cells != null) {
			row.AddRange(cells);
		}
		Table.Add(row);
	}

	public void SetHeader(IEnumerable<string> cells)
	{
		Header.Clear();
		Header.AddRange(cells);
	}

	public void SetCell(int rowIx, int colIx, string s)
	{
		//Console.WriteLine($"rowIx = {rowIx} TC = {Table.Count}");
		while (rowIx >= Table.Count) {
			AddRow();
			//Console.WriteLine("AddRow");
		}
		var row = Table[rowIx];
		//Console.WriteLine($"colIx = {colIx} RC = {row.Count}");
		while (colIx >= row.Count) {
			Table[rowIx].Add("");
		}
		row[colIx] = s;
	}

	public int RowCount { get {
		return Table.Count;
	}}

	public override string ToString()
	{
		var sb = new StringBuilder();
		if (Header.Count > 0) {
			var hr = new StringBuilder();
			sb.Append('|');
			hr.Append('|');

			foreach(string c in Header) {
				string d = new string('-',c.Length);
				sb.Append(' ').Append(c).Append(" |");
				hr.Append('-').Append(d).Append("-|");
			}
			sb.AppendLine();
			sb.AppendLine(hr.ToString());
		}

		if (Table.Count > 0) {
			foreach(var row in Table) {
				sb.Append('|');
				foreach(string c in row) {
					sb.Append(' ').Append(c).Append(" |");
				}
				sb.AppendLine();
			}
		}

		return sb.ToString();
	}

	List<string> Header = new List<string>();
	List<List<string>> Table = new List<List<string>>();
}
