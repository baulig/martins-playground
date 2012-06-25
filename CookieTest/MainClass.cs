using System;

namespace Test
{
	public static class MainClass
	{
		public static void Main ()
		{
			var parser = new CookieParser (CookieTest.A);
			foreach (var cookie in parser.Parse ()) {
				Console.WriteLine ("COOKIE: |{0}| -> |{1}|",
				                   cookie.Name, cookie.Value);
			}
		}
	}
}

