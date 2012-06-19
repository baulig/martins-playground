using System;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Threading;
using System.Threading.Tasks;

namespace WebClientTests
{
	public class Server
	{
		HttpListener listener;

		public int Port {
			get;
			private set;
		}

		public Server ()
		{
			var r = new Random ();
		again:
			listener = new HttpListener ();
			Port = r.Next (1025, 65535);
			listener.Prefixes.Add ("http://+:" + Port + "/");
			try {
				listener.Start ();
			} catch (SocketException ex) {
				if (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
					goto again;
				throw;
			}

			Task.Factory.StartNew (() => Run ());
		}

		public async Task Run ()
		{
			var context = await listener.GetContextAsync ().ConfigureAwait (false);
			Console.WriteLine ("GOT CONTEXT: {0}", context);

			var res = context.Response;
			res.ContentLength64 = (long) Math.Pow (2, 28);

			var buffer = new byte [65536];
			res.OutputStream.Write (buffer, 0, buffer.Length);

			Thread.Sleep (15000);
		}
	}
}
