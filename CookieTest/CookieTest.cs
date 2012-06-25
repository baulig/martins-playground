using System;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;

namespace Test
{
	[TestFixture]
	public class CookieTest
	{
		public const string A = "Foo=Bar, expires=World; expires=Sat, 11-Oct-14 22:45:19 GMT, A=B";
		public const string B = "A=B=C, expires=Sat, 99-Dec-01 01:00:00 XDT; Hello=World, Foo=Bar";
		// Only the invariant culture is allowed.
		public const string C = "Foo=Bar, expires=Montag, 21. Juni 2012 23:11:45";

		[Test]
		public void TestExpires ()
		{
			HttpWebResponse res;
			using (var listener = new Listener ("Set-Cookie: " + A)) {
				var req = (HttpWebRequest)HttpWebRequest.Create (listener.URI);
				req.CookieContainer = new CookieContainer ();
				req.Method = "POST";
				res = (HttpWebResponse)req.GetResponse ();
			}

			Assert.AreEqual (A, res.Headers.Get ("Set-Cookie"));

			var values = res.Headers.GetValues ("Set-Cookie");
			Assert.AreEqual (4, values.Length);
			Assert.AreEqual ("Foo=Bar", values [0]);
			Assert.AreEqual ("expires=World; expires=Sat", values [1]);
			Assert.AreEqual ("11-Oct-14 22:45:19 GMT", values [2]);
			Assert.AreEqual ("A=B", values [3]);

			Assert.AreEqual (3, res.Cookies.Count);
			Assert.AreEqual ("Foo", res.Cookies [0].Name);
			Assert.AreEqual (0, res.Cookies [0].Expires.Ticks);
			Assert.AreEqual ("expires", res.Cookies [1].Name);
			Assert.AreEqual (635486643190000000, res.Cookies [1].Expires.ToUniversalTime ().Ticks);
			Assert.AreEqual ("A", res.Cookies [2].Name);
			Assert.AreEqual (0, res.Cookies [2].Expires.Ticks);
		}

		[Test]
		public void TestInvalidCookie ()
		{
			HttpWebResponse res;
			using (var listener = new Listener ("Set-Cookie: " + B)) {
				var req = (HttpWebRequest)HttpWebRequest.Create (listener.URI);
				req.CookieContainer = new CookieContainer ();
				req.Method = "POST";
				res = (HttpWebResponse)req.GetResponse ();
			}

			Assert.AreEqual (B, res.Headers.Get ("Set-Cookie"));

			var values = res.Headers.GetValues ("Set-Cookie");
			Assert.AreEqual (4, values.Length);
			Assert.AreEqual ("A=B=C", values [0]);
			Assert.AreEqual ("expires=Sat", values [1]);
			Assert.AreEqual ("99-Dec-01 01:00:00 XDT; Hello=World", values [2]);
			Assert.AreEqual ("Foo=Bar", values [3]);

			Assert.AreEqual (3, res.Cookies.Count);
			Assert.AreEqual ("A", res.Cookies [0].Name);
			Assert.AreEqual ("B=C", res.Cookies [0].Value);
			Assert.AreEqual ("expires", res.Cookies [1].Name);
			Assert.AreEqual ("Sat", res.Cookies [1].Value);
			Assert.AreEqual ("Foo", res.Cookies [2].Name);
			Assert.AreEqual ("Bar", res.Cookies [2].Value);
		}

		[Test]
		public void TestLocalCulture ()
		{
			var old = Thread.CurrentThread.CurrentCulture;
			try {
				var culture = new CultureInfo ("de-DE");
				Thread.CurrentThread.CurrentCulture = culture;
				DoTestLocalCulture ();
			} finally {
				Thread.CurrentThread.CurrentCulture = old;
			}
		}

		void DoTestLocalCulture ()
		{
			HttpWebResponse res;
			using (var listener = new Listener ("Set-Cookie: " + C)) {
				var req = (HttpWebRequest)HttpWebRequest.Create (listener.URI);
				req.CookieContainer = new CookieContainer ();
				req.Method = "POST";
				res = (HttpWebResponse)req.GetResponse ();
			}

			Assert.AreEqual (C, res.Headers.Get ("Set-Cookie"));

			Assert.AreEqual (2, res.Cookies.Count);
			Assert.AreEqual ("Foo", res.Cookies [0].Name);
			Assert.AreEqual ("Bar", res.Cookies [0].Value);
			Assert.AreEqual ("expires", res.Cookies [1].Name);
			Assert.AreEqual ("Montag", res.Cookies [1].Value);
		}

		public class Listener : IDisposable
		{
			Socket socket;
			string[] headers;

			public Listener (params string[] headers)
			{
				this.headers = headers;
				Start ();
			}

			void Start ()
			{
				socket = new Socket (
					AddressFamily.InterNetwork, SocketType.Stream,
					ProtocolType.Tcp);
				socket.Bind (new IPEndPoint (IPAddress.Loopback, 0));
				socket.Listen (1);
				socket.BeginAccept ((result) => {
					var accepted = socket.EndAccept (result);
					HandleRequest (accepted);
				}, null);
			}

			public void Dispose ()
			{
				if (socket != null) {
					socket.Close ();
					socket = null;
				}
			}

			void HandleRequest (Socket accepted)
			{
				using (var stream = new NetworkStream (accepted)) {
					using (var writer = new StreamWriter (stream)) {
						writer.WriteLine ("HTTP/1.1 200 OK");
						writer.WriteLine ("Content-Type: text/plain");
						foreach (var header in headers)
							writer.WriteLine (header);
						writer.WriteLine ();
						writer.WriteLine ("HELLO");
					}
				}
			}

			public EndPoint EndPoint {
				get { return socket.LocalEndPoint; }
			}

			public string URI {
				get { return string.Format ("http://{0}/", EndPoint); }
			}
		}
	}
}
