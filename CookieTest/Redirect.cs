using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Test
{
	public class Redirect
	{
		Socket socket;

		public Redirect ()
		{
			Start ();
		}

		void Start ()
		{
			socket = new Socket (
					AddressFamily.InterNetwork, SocketType.Stream,
					ProtocolType.Tcp);
			socket.Bind (new IPEndPoint (IPAddress.Loopback, 9999));
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
			var target = "http://www.google.com/ncr";
			using (var stream = new NetworkStream (accepted)) {
				using (var writer = new StreamWriter (stream)) {
					writer.WriteLine ("HTTP/1.1 302 Found");
					writer.WriteLine ("Location: " + target);
					writer.WriteLine ();
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

