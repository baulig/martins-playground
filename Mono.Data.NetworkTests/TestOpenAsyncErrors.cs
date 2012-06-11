using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using NUnit.Framework;

namespace Mono.Data.NetworkTests
{
	[TestFixture]
	public class TestOpenAsyncErrors
	{
		Socket socket;

		[SetUp]
		public void SetUp ()
		{
			socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Bind (new IPEndPoint (IPAddress.Loopback, 0));
			socket.Listen (1);

			Task.Factory.StartNew (() => socket.Accept ());
		}

		[TearDown]
		public void TearDown ()
		{
			socket.Dispose ();
			socket = null;
		}

		[Test]
		public void OpenMultiple ()
		{
			var ep = (IPEndPoint)socket.LocalEndPoint;
			var cstr = string.Format ("Data Source={0},{1}", ep.Address, ep.Port);
			var conn = new SqlConnection (cstr);

			conn.OpenAsync ();
			Assert.That (conn.State == ConnectionState.Connecting);

			var error = conn.OpenAsync ();
			AssertEx.Exception<InvalidOperationException> (
				() => error.Wait (5000), "Multiple OpenAsync()");
		}

		[Test]
		public void CancelOpen ()
		{
			var ep = (IPEndPoint)socket.LocalEndPoint;
			var cstr = string.Format ("Data Source={0},{1}; timeout=2", ep.Address, ep.Port);
			var conn = new SqlConnection (cstr);

			var cts = new CancellationTokenSource (250);
			var task = conn.OpenAsync (cts.Token);
			Assert.That (conn.State == ConnectionState.Connecting);

			try {
				task.Wait (3000);
				Assert.Fail ("Cancel OpenAsync()!");
			} catch (Exception) {
				;
			}

			Assert.That (task.IsCanceled || task.IsFaulted);

			Assert.That (conn.State == ConnectionState.Closed);
		}

		[Test]
		public void CloseWhileOpen ()
		{
			var ep = (IPEndPoint)socket.LocalEndPoint;
			var cstr = string.Format ("Data Source={0},{1}; timeout=2", ep.Address, ep.Port);
			var conn = new SqlConnection (cstr);

			var task = conn.OpenAsync ();
			Assert.That (conn.State == ConnectionState.Connecting);

			Task.Delay (1000).ContinueWith (_ => conn.Close ());

			try {
				task.Wait (3000);
				Assert.Fail ("Cancel OpenAsync()!");
			} catch (Exception) {
				;
			}

			Assert.That (task.IsCanceled || task.IsFaulted);

			Assert.That (conn.State == ConnectionState.Closed);
		}

	}
}
