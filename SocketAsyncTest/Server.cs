using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class Server
{
	Socket socket;
	ManualResetEventSlim readyEvent;
	ManualResetEvent mainEvent;

	public Server ()
	{
		readyEvent = new ManualResetEventSlim (false);
		mainEvent = new ManualResetEvent (false);

		ThreadPool.QueueUserWorkItem (_ => DoWork ());
		readyEvent.Wait ();
		Console.WriteLine ("SERVER: {0}", socket.LocalEndPoint);
	}

	public EndPoint EndPoint {
		get { return socket.LocalEndPoint; }
	}

	public WaitHandle MainEvent {
		get { return mainEvent; }
	}

	void DoWork ()
	{
		socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		socket.Bind (new IPEndPoint (IPAddress.Loopback, 0));
		socket.Listen (1);

		var async = new SocketAsyncEventArgs ();
		async.Completed += (s,e) => OnAccepted (e);

		readyEvent.Set ();

		if (!socket.AcceptAsync (async))
			OnAccepted (async);
	}

	static void Debug (string message, params object[] args)
	{
		Console.Error.WriteLine (string.Format (message, args));
	}

	void OnAccepted (SocketAsyncEventArgs e)
	{
		Debug ("ON ACCEPTED: {0}", e.SocketError);

		MainLoop (e.AcceptSocket);
	}

	void MainLoop (Socket socket)
	{
		Debug ("MAIN LOOP: {0}", Thread.CurrentThread.ManagedThreadId);

		try {
			var header = new byte [8];
			Debug ("MAIN LOOP #0: {0}", socket.Available);
			var res = socket.Receive (header);
			Debug ("MAIN LOOP #1: {0}", res);
		} catch (Exception ex) {
			Debug ("MAIN LOOP EX: {0}", ex);
		}

		mainEvent.Set ();

		Debug ("MAIN LOOP DONE");
	}
}
