using System;
using System.Configuration;

namespace TestSqlClient
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var cstr = ConfigurationManager.AppSettings ["ConnectionString"];
			var test = new TestConnection (cstr);

			var task = test.Open ();
			if (!task.Wait (5000))
				Console.WriteLine ("Failed to open connection!");
		}
	}
}
