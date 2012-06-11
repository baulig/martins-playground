//
// TestOpenAsyncErrors.cs
//
// Authors:
//      Martin Baulig (martin.baulig@googlemail.com)
//
// Copyright 2012 Xamarin Inc. (http://www.xamarin.com)
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
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
