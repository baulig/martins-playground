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
		public void RelativeUri ()
		{
			var client = new HttpClient ();
			client.BaseAddress = new Uri ("http://en.wikipedia.org/wiki/");
			var uri = new Uri ("Computer", UriKind.Relative);
			Assert.That (client.GetStringAsync (uri).Result != null);
		}

		[Test]
		public void RelativeUri2 ()
		{
			var client = new HttpClient ();
			client.BaseAddress = new Uri ("http://en.wikipedia.org/wiki/");
			Assert.That (client.GetStringAsync ("Computer").Result != null);
		}

	}
}
