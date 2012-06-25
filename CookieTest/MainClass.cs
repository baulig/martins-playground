using System;
using System.Reflection;
using NUnit.Framework;

namespace Test
{
	public static class MainClass
	{
		public static void Main ()
		{
			var test = new CookieTest ();
			var klass = test.GetType ();

			var bf = BindingFlags.Instance | BindingFlags.Public;
			foreach (var method in klass.GetMethods (bf)) {
				var cattr = method.GetCustomAttributes (typeof(TestAttribute), false);
				if ((cattr == null) || (cattr.Length != 1))
					continue;

				Console.WriteLine ("INVOKING: {0}", method.Name);
				method.Invoke (test, null);
			}
		}
	}
}

