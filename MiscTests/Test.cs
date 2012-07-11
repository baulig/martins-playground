using System;
using System.Net;
using System.Net.Http;
using NUnit.Framework;

namespace MiscTests
{
	[TestFixture]
	public class Test
	{
		[Test]
		public void GetString_RelativeUri ()
		{
			var client = new HttpClient ();
			client.BaseAddress = new Uri ("http://en.wikipedia.org/wiki/");
			var uri = new Uri ("Computer", UriKind.Relative);

			Assert.That (client.GetStringAsync (uri).Result != null);
			Assert.That (client.GetStringAsync ("Computer").Result != null);
		}

		[Test]
		public void Ctor_RelativeUri ()
		{
			var client = new HttpClient ();
			client.BaseAddress = new Uri ("http://en.wikipedia.org/wiki/");
			var uri = new Uri ("Computer", UriKind.Relative);
			var req = new HttpRequestMessage (HttpMethod.Get, uri);
			// HttpRequestMessage does not rewrite it here.
			Assert.AreEqual (req.RequestUri, uri);
		}

		[Test]
		public void Ctor_RelativeUriString ()
		{
			var client = new HttpClient ();
			client.BaseAddress = new Uri ("http://en.wikipedia.org/wiki/");
			var req = new HttpRequestMessage (HttpMethod.Get, "Computer");
			// HttpRequestMessage does not rewrite it here.
			Assert.IsFalse (req.RequestUri.IsAbsoluteUri);
		}
	}
}
