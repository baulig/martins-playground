using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

namespace MiscTests
{
	public class MainClass
	{
		public static void Main (string[] args)
		{
			var test = new Test ();
			test.GetString_RelativeUri ();
			test.Ctor_RelativeUri ();
			test.Ctor_RelativeUriString ();
			TestRelative ().Wait ();
		}

		static async Task TestRelative ()
		{
			var client = new HttpClient ();
			client.BaseAddress = new Uri ("http://en.wikipedia.org/wiki/");

			var uri = new Uri ("Apple", UriKind.Relative);
			var res = await client.GetStringAsync (uri);
			Console.WriteLine ("Got {0} characters in response.", res.Length);

			var req = new HttpRequestMessage (HttpMethod.Get, uri);
			var res2 = await client.SendAsync (req);
			Console.WriteLine ("TEST: {0}", res2);
		}
	}
}
