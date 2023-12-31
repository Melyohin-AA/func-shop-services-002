using System;
using System.Text;

internal static class Utils
{
	public static int? IntFromRawString(string rawStr)
	{
		if (rawStr == null) return null;
		var sb = new StringBuilder(rawStr.Length);
		foreach (char ch in rawStr)
			if (char.IsDigit(ch))
				sb.Append(ch);
		return int.TryParse(sb.ToString(), out int integer) ? integer : null;
	}
}
