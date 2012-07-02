using System;
using NUnit.Framework;

namespace MiscTests
{
	public class MainClass
	{
		public static void Main (string[] args)
		{
			var a = new Uri ("http://[fe80::1]/");
			Console.WriteLine (a);
		}
	}
}
