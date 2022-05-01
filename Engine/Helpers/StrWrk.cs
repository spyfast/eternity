using VkNet;

namespace Eternity.Engine.Helpers
{
    internal static class StrWrk
    {
		public static string qSubstr(string str, string substr, bool before = false)
		{
			if (before)
				return str.Substring(0, str.IndexOf(substr));
			return str.Substring(str.IndexOf(substr) + substr.Length);
		}

		public static string GetBetween(string str, string left, string right)
		{
			return qSubstr(qSubstr(str, left, false), right, true);
		}
		public static int IsInteger(string value, int delay)
		{
			var result = 0;
			return int.TryParse(value, out result) ? result > delay ? result : delay : delay;
		}
	}
}
