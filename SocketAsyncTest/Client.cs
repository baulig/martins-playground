using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class Client
{
	Socket socket;
	Server server;

	public Client ()
	{
		server = new Server ();

		socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		socket.Connect (server.EndPoint);
		socket.NoDelay = true;
	}

	static void Debug (string message, params object[] args)
	{
		Console.Error.WriteLine (string.Format (message, args));
	}

	public void Test (byte[] buffer, int size)
	{
		var m = new ManualResetEventSlim (false);
		var e = new SocketAsyncEventArgs ();
		e.SetBuffer (buffer, 0, size);
		e.Completed += (s,o) => {
			Debug ("DO WRITE #1: {0} {1}", o == e, o.SocketError);
			m.Set ();
		};
		bool res = socket.SendAsync (e);
		Debug ("DO WRITE: {0}", res);
		if (res)
			m.Wait ();
		Debug ("DO WRITE DONE");
	}

	public void Wait ()
	{
		server.MainEvent.WaitOne ();
	}
}

