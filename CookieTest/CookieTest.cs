using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;

namespace Test2
{
	[TestFixture]
	public class CookieTest
	{
		const string A = "Set-Cookie: Foo=Bar, expires=World; expires=Sat, 11-Oct-14 22:45:19 GMT, A=B";
		const string B = "Set-Cookie: A=B=C, expires=Sat, 99-Dec-01 01:00:00 XDT; Hello=World, Foo=Bar";

		[Test]
		public void TestExpires ()
		{
			HttpWebResponse res;
			using (var listener = new Listener (A)) {
				var req = (HttpWebRequest)HttpWebRequest.Create (listener.URI);
				req.CookieContainer = new CookieContainer ();
				req.Method = "POST";
				res = (HttpWebResponse)req.GetResponse ();
			}

			var values = res.Headers.GetValues ("Set-Cookie");
			Assert.That (values.Length == 4, "#1");
			Assert.AreEqual ("Foo=Bar", values [0]);
			Assert.AreEqual ("expires=World; expires=Sat", values [1]);
			Assert.AreEqual ("11-Oct-14 22:45:19 GMT", values [2]);
			Assert.AreEqual ("A=B", values [3]);

			Assert.That (res.Cookies.Count == 3, "#2");
			Assert.AreEqual ("Foo", res.Cookies [0].Name);
			Assert.That (res.Cookies [0].Expires.Ticks == 0, "#3");
			Assert.AreEqual ("expires", res.Cookies [1].Name);
			Assert.AreEqual (635486643190000000, res.Cookies [1].Expires.ToUniversalTime ().Ticks);
			Assert.AreEqual ("A", res.Cookies [2].Name);
			Assert.That (res.Cookies [2].Expires.Ticks == 0, "#4");
		}

		[Test]
		public void TestInvalidCookie ()
		{
			HttpWebResponse res;
			using (var listener = new Listener (B)) {
				var req = (HttpWebRequest)HttpWebRequest.Create (listener.URI);
				req.CookieContainer = new CookieContainer ();
				req.Method = "POST";
				res = (HttpWebResponse)req.GetResponse ();
			}

			var values = res.Headers.GetValues ("Set-Cookie");
			Assert.That (values.Length == 4, "#1");
			Assert.AreEqual ("A=B=C", values [0]);
			Assert.AreEqual ("expires=Sat", values [1]);
			Assert.AreEqual ("99-Dec-01 01:00:00 XDT; Hello=World", values [2]);
			Assert.AreEqual ("Foo=Bar", values [3]);

			Assert.That (res.Cookies.Count == 3, "#2");
			Assert.AreEqual ("A", res.Cookies [0].Name);
			Assert.AreEqual ("B=C", res.Cookies [0].Value);
			Assert.AreEqual ("expires", res.Cookies [1].Name);
			Assert.AreEqual ("Sat", res.Cookies [1].Value);
			Assert.AreEqual ("Foo", res.Cookies [2].Name);
			Assert.AreEqual ("Bar", res.Cookies [2].Value);
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
