using System;

namespace Test
{
	public static class MainClass
	{
		public static void Main ()
		{
			var test = new CookieTest ();
			test.TestExpires ();
			test.TestInvalidCookie ();
		}
	}
}

