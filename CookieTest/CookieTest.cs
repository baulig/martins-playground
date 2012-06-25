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
		// Only comma serves as separation character.
		public const string D = "A=B, C=D; E=F, G=H";
		public const string E = "A; C; expires=Mon, 21-Jun-14 23:45:00 GMT, E=F";

		CookieCollection DoRequest (string header)
		{
#if USE_WEB
			HttpWebResponse res;
			using (var listener = new Listener ("Set-Cookie: " + header)) {
				var req = (HttpWebRequest)HttpWebRequest.Create (listener.URI);
				req.CookieContainer = new CookieContainer ();
				req.Method = "POST";
				res = (HttpWebResponse)req.GetResponse ();
			}

			Assert.AreEqual (header, res.Headers.Get ("Set-Cookie"));
			return res.Cookies;
#else
			var cookies = new CookieCollection ();
			var parser = new CookieParser (header);
			foreach (var cookie in parser.Parse ())
				cookies.Add (cookie);
			return cookies;
#endif
		}

		void AssertCookie (Cookie cookie, string name, string value, long ticks)
		{
			AssertCookie (cookie, name, value);
			Assert.AreEqual (ticks, cookie.Expires.ToUniversalTime ().Ticks);
		}

		void AssertCookie (Cookie cookie, string name, string value)
		{
			Assert.AreEqual (name, cookie.Name);
			Assert.AreEqual (value, cookie.Value);
		}

		[Test]
		public void TestExpires ()
		{
			var cookies = DoRequest (A);

#if CHECK_HEADERS
			var values = res.Headers.GetValues ("Set-Cookie");
			Assert.AreEqual (4, values.Length);
			Assert.AreEqual ("Foo=Bar", values [0]);
			Assert.AreEqual ("expires=World; expires=Sat", values [1]);
			Assert.AreEqual ("11-Oct-14 22:45:19 GMT", values [2]);
			Assert.AreEqual ("A=B", values [3]);
#endif

			Assert.AreEqual (3, cookies.Count);
			AssertCookie (cookies [0], "Foo", "Bar", 0);
			AssertCookie (cookies [1], "expires", "World", 635486643190000000);
			AssertCookie (cookies [2], "A", "B", 0);
		}

		[Test]
		public void TestInvalidCookie ()
		{
			var cookies = DoRequest (B);

#if CHECK_HEADERS
			var values = res.Headers.GetValues ("Set-Cookie");
			Assert.AreEqual (4, values.Length);
			Assert.AreEqual ("A=B=C", values [0]);
			Assert.AreEqual ("expires=Sat", values [1]);
			Assert.AreEqual ("99-Dec-01 01:00:00 XDT; Hello=World", values [2]);
			Assert.AreEqual ("Foo=Bar", values [3]);
#endif

			Assert.AreEqual (3, cookies.Count);
			AssertCookie (cookies [0], "A", "B=C");
			AssertCookie (cookies [1], "expires", "Sat");
			AssertCookie (cookies [2], "Foo", "Bar");
		}

		[Test]
		public void TestLocalCulture ()
		{
			var old = Thread.CurrentThread.CurrentCulture;
			try {
				var culture = new CultureInfo ("de-DE");
				Thread.CurrentThread.CurrentCulture = culture;

				var cookies = DoRequest (C);
				Assert.AreEqual (2, cookies.Count);
				AssertCookie (cookies [0], "Foo", "Bar");
				AssertCookie (cookies [1], "expires", "Montag");
			} finally {
				Thread.CurrentThread.CurrentCulture = old;
			}
		}

		[Test]
		public void TestMultiple ()
		{
			var cookies = DoRequest (D);

#if CHECK_HEADERS
			var values = res.Headers.GetValues ("Set-Cookie");
			Assert.AreEqual (3, values.Length);
			Assert.AreEqual ("A=B", values [0]);
			Assert.AreEqual ("C=D; E=F", values [1]);
			Assert.AreEqual ("G=H", values [2]);
#endif

			Assert.AreEqual (3, cookies.Count);
			AssertCookie (cookies [0], "A", "B");
			AssertCookie (cookies [1], "C", "D");
			AssertCookie (cookies [2], "G", "H");
		}

		[Test]
		public void TestMultiple2 ()
		{
			var cookies = DoRequest (E);
			Assert.AreEqual (2, cookies.Count);
			AssertCookie (cookies [0], "A", string.Empty, 635389911000000000);
			AssertCookie (cookies [1], "E", "F");
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
