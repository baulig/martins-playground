using System;
using System.Net;
using System.Threading;

namespace Test
{
	public static class MainClass
	{
		static void Main ()
		{
			var redirect = new Redirect ();
			Console.WriteLine (redirect.URI);

			// Thread.Sleep (Timeout.Infinite);

			var req = (HttpWebRequest)WebRequest.Create (redirect.URI);
			req.KeepAlive = true;
			req.AllowAutoRedirect = true;
			req.Method = "GET";
			req.CookieContainer = new CookieContainer ();

			var res = req.GetResponse ();
			Console.WriteLine ("TEST: {0}", res.ResponseUri);
		}
	}
}

