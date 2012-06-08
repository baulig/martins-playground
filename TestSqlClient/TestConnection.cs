using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using NUnit.Framework;

namespace TestSqlClient
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
