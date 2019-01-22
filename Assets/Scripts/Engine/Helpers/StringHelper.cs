using System;
using System.Collections.Generic;
using System.Linq;

namespace PiRhoSoft.UtilityEngine
{
	public static class StringHelper
	{
		public static int ParseInt(string s, int start, int end)
		{
			// This exists as a way to parse an integer without having to instantiate a substring first.

			var value = 0;
			for (var i = start; i < end; i++)
				value = value * 10 + (s[i] - '0');

			return value;
		}

		public static string FindCommonPath(IEnumerable<string> paths)
		{
			var prefix = paths.FirstOrDefault() ?? "";

			foreach (var path in paths)
			{
				var index = 0;
				var count = Math.Min(prefix.Length, path.Length);

				for (; index < count; index++)
				{
					if (prefix[index] != path[index])
						break;
				}

				prefix = prefix.Substring(0, index);

				var slash = prefix.LastIndexOf('/');
				if (slash != prefix.Length - 1)
					prefix = slash >= 0 ? prefix.Substring(0, slash + 1) : "";
			}

			return prefix;
		}
	}
}
