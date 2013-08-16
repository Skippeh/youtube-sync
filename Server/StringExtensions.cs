using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	public static class StringExtensions
	{
		public static int EndIndexOf(this string str, string value)
		{
			return str.IndexOf(value) + value.Length;
		}

		public static string Capitalize(this string str)
		{
			if (str.Length > 1)
				return str[0].ToString().ToUpper() + str.Substring(1);

			return str.ToUpper();
		}
	}
}