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
	public class TestDatabase
	{
		public static string ConnectionString {
			get { return ConfigurationManager.AppSettings ["ConnectionString"]; }
		}

		public SqlConnection Connection {
			get;
			private set;
		}

		public static TimeSpan NetworkTimeout {
			get {
				if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
					// We're on the same machine.
					return TimeSpan.FromMilliseconds (250);
				} else {
					// We're using a network connection.
					return TimeSpan.FromMilliseconds (2500);
				}
			}
		}

		public static TimeSpan LongTimeout {
			get {
				if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
					// Must be more than one second.
					return TimeSpan.FromMilliseconds (1500);
				} else {
					return TimeSpan.FromMilliseconds (6000);
				}
			}
		}

		[SetUp]
		public void SetUp ()
		{
			Connection = new SqlConnection (ConnectionString);
			Assert.That (Connection.OpenAsync ().Wait (NetworkTimeout));
		}

		[TearDown]
		public void TearDown ()
		{
			Connection.Close ();
		}

		[Test]
		public void CreateTable ()
		{
			var tmpname = string.Format ("tmp{0}", DateTime.Now.Ticks);

			var create = new SqlCommand (
				string.Format ("CREATE TABLE {0} (a int)", tmpname), Connection);
			Assert.That (create.ExecuteNonQueryAsync ().Wait (NetworkTimeout));

			var drop = new SqlCommand (
				string.Format ("DROP TABLE {0}", tmpname), Connection);
			Assert.That (drop.ExecuteNonQueryAsync ().Wait (NetworkTimeout));
		}

		[Test]
		public void ReadTest ()
		{
			var cmd = new SqlCommand ("SELECT * FROM test", Connection);
			var task = cmd.ExecuteReaderAsync ();
			Assert.That (task.Wait (NetworkTimeout));

			var reader = task.Result;
			Assert.That (reader.FieldCount > 0);
			reader.Close ();
		}

		[Test]
		public void MustCloseReader ()
		{
			var cmd = new SqlCommand ("SELECT * FROM test", Connection);
			Console.WriteLine ("TEST: {0}", cmd.CommandTimeout);
			var task = cmd.ExecuteReaderAsync ();
			Assert.That (task.Wait (NetworkTimeout));

			try {
				var errorCmd = new SqlCommand ("SELECT * FROM test", Connection);
				AssertEx.Exception<InvalidOperationException> (
					() => errorCmd.ExecuteReaderAsync ().Wait (NetworkTimeout));
			} finally {
				task.Result.Close ();
			}
		}

		[Test]
		public void TestCommandTimeout ()
		{
			var cmd = new SqlCommand ("WAITFOR DELAY '00:00:15'", Connection);
			cmd.CommandTimeout = (int)(NetworkTimeout.TotalMilliseconds + 999) / 1000;
			var task = cmd.ExecuteNonQueryAsync ();
			AssertEx.Exception<SqlException> (() => task.Wait (LongTimeout));
		}

		[Test]
		public void Cancel ()
		{
			var cmd = new SqlCommand ("WAITFOR DELAY '00:00:15'", Connection);
			var task = cmd.ExecuteNonQueryAsync ();
			Task.Delay (NetworkTimeout).Wait ();
			cmd.Cancel ();
			AssertEx.Exception<SqlException> (() => task.Wait (LongTimeout));
		}

		[Test]
		public void CancelWithToken ()
		{
			var cmd = new SqlCommand ("WAITFOR DELAY '00:00:15'", Connection);
			var cts = new CancellationTokenSource (NetworkTimeout);
			var task = cmd.ExecuteNonQueryAsync (cts.Token);
			AssertEx.Exception<SqlException> (() => task.Wait (LongTimeout));
		}

	}
}
