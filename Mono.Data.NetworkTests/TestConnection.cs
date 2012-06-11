//
// TestConnection.cs
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
	public class TestConnection
	{
		public static string ConnectionString {
			get { return ConfigurationManager.AppSettings ["ConnectionString"]; }
		}

		[Test]
		public void OpenAsync ()
		{
			bool state_changed = false;
			bool state_changed_error = false;

			var conn = new SqlConnection (ConnectionString);
			conn.StateChange += (sender, e) => {
				state_changed = true;
				state_changed_error = e.CurrentState != ConnectionState.Open;
			};

			var openTask = conn.OpenAsync ();
			openTask.Wait (2500);

			Assert.That (openTask.IsCompleted, "#1");
			Assert.That (conn.State == ConnectionState.Open, "#2");
			Assert.That (state_changed, "#3");
			Assert.That (!state_changed_error, "#4");
		}
	}
}
