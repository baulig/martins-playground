using System;

namespace Test
{
	public static class MainClass
	{
		public static void Main ()
		{
			var parser = new CookieParser (CookieTest.A);
			string name, val;
			while (parser.GetNextNameValue (out name, out val)) {
				Console.WriteLine ("COOKIE: |{0}| -> |{1}|", name, val);
			}
		}
	}
}

