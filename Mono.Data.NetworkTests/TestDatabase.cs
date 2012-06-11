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
	public class TestDatabase
	{
		public SqlConnection Connection {
			get;
			private set;
		}

		[SetUp]
		public void SetUp ()
		{
			Connection = new SqlConnection (NetworkConfig.ConnectionString);
			Assert.That (Connection.OpenAsync ().Wait (NetworkConfig.NetworkTimeout),
			             "Database connection failed.");
		}

		[TearDown]
		public void TearDown ()
		{
			Connection.Close ();
		}

		SqlCommand CreateCommand (string text)
		{
			var cmd = new SqlCommand (text, Connection);
			cmd.CommandTimeout = NetworkConfig.CommandTimeout;
			return cmd;
		}

		SqlCommand CreateCommand (string text, params object[] args)
		{
			return CreateCommand (string.Format (text, args));
		}

		[Test]
		public void CreateTable ()
		{
			var tmpname = string.Format ("tmp{0}", DateTime.Now.Ticks);

			var create = CreateCommand ("CREATE TABLE {0} (a int)", tmpname);
			AssertEx.TaskCompleted (create.ExecuteNonQueryAsync (), NetworkConfig.NetworkTimeout);

			var drop = CreateCommand ("DROP TABLE {0}", tmpname);
			AssertEx.TaskCompleted (drop.ExecuteNonQueryAsync (), NetworkConfig.NetworkTimeout);
		}

		[Test]
		[Category("Database")]
		public void ReadTest ()
		{
			var cmd = CreateCommand ("SELECT * FROM test");
			var reader = AssertEx.TaskCompleted (cmd.ExecuteReaderAsync (), NetworkConfig.NetworkTimeout);
			Assert.That (reader.FieldCount > 0);
			reader.Close ();
		}

		[Test]
		[Category("Database")]
		public void MustCloseReader ()
		{
			var cmd = new SqlCommand ("SELECT * FROM test", Connection);
			var task = cmd.ExecuteReaderAsync ();
			Assert.That (task.Wait (NetworkConfig.NetworkTimeout));

			try {
				var errorCmd = CreateCommand ("SELECT * FROM test");
				AssertEx.TaskFailed<InvalidOperationException> (
					errorCmd.ExecuteNonQueryAsync (), NetworkConfig.NetworkTimeout);
			} finally {
				task.Result.Close ();
			}
		}

		[Test]
		[Category("Database")]
		public void MustCloseReader2 ()
		{
			var cmd = new SqlCommand ("SELECT * FROM test", Connection);
			var task = cmd.ExecuteReaderAsync ();
			Assert.That (task.Wait (NetworkConfig.NetworkTimeout));

			try {
				var errorCmd = CreateCommand ("SELECT * FROM test");
				AssertEx.TaskFailed<InvalidOperationException> (
					errorCmd.ExecuteReaderAsync (), NetworkConfig.NetworkTimeout);
			} finally {
				task.Result.Close ();
			}
		}

		[Test]
		[Category("Database")]
		public void MultiReadTest ()
		{
			var cmd = CreateCommand ("SELECT * FROM test");
			var reader = AssertEx.TaskCompleted (cmd.ExecuteReaderAsync (), NetworkConfig.NetworkTimeout);
			Assert.That (reader.FieldCount > 0);
			reader.Close ();

			cmd = CreateCommand ("SELECT * FROM test");
			reader = AssertEx.TaskCompleted (cmd.ExecuteReaderAsync (), NetworkConfig.NetworkTimeout);
			Assert.That (reader.FieldCount > 0);
			reader.Close ();
		}

		[Test]
		[Category("Cancel")]
		public void CommandTimeout ()
		{
			var cmd = CreateCommand ("WAITFOR DELAY '00:00:15'");
			AssertEx.TaskFailed<SqlException> (
				cmd.ExecuteNonQueryAsync (), NetworkConfig.LongTimeout);
		}

		[Test]
		[Category("Sync")]
		[Category("NotWorking")]
		public void SyncTimeout ()
		{
			var cmd = new SqlCommand ("WAITFOR DELAY '00:00:30'", Connection);
			cmd.CommandTimeout = NetworkConfig.CommandTimeout;
			var start = DateTime.Now;

			try {
				cmd.ExecuteNonQuery ();
				Assert.Fail ();
			} catch (SqlException) {
				Assert.That (DateTime.Now - start < TimeSpan.FromSeconds (2 * NetworkConfig.CommandTimeout));
			}
		}

		[Test]
		[Category("Sync")]
		[Category("NotWorking")]
		public void TraditionalTimeout ()
		{
			var cmd = new SqlCommand ("WAITFOR DELAY '00:00:30'", Connection);
			cmd.CommandTimeout = NetworkConfig.CommandTimeout;
			var start = DateTime.Now;

			var ar = cmd.BeginExecuteNonQuery (_ => {
				Console.WriteLine ("TRADITIONAL CALLBACK!");
			}, null);

			bool result = ar.AsyncWaitHandle.WaitOne (TimeSpan.FromSeconds (3 * NetworkConfig.CommandTimeout));
			Console.WriteLine ("AFTER WAITING: {0} {1} {2}", DateTime.Now - start,
			                   NetworkConfig.CommandTimeout, result);
			Assert.IsFalse (result);
			Assert.That (DateTime.Now - start > TimeSpan.FromSeconds (2 * NetworkConfig.CommandTimeout));
		}

		[Test]
		[Category("Cancel")]
		public void Cancel ()
		{
			var cmd = new SqlCommand ("WAITFOR DELAY '00:00:30'", Connection);
			var task = cmd.ExecuteNonQueryAsync ();
			Task.Delay (NetworkConfig.NetworkTimeout).Wait ();
			cmd.Cancel ();
			AssertEx.TaskFailed<SqlException> (task, NetworkConfig.LongTimeout);
		}

		[Test]
		[Category("Cancel")]
		public void CancelWithToken ()
		{
			var cmd = new SqlCommand ("WAITFOR DELAY '00:00:15'", Connection);
			var cts = new CancellationTokenSource (NetworkConfig.NetworkTimeout);
			AssertEx.TaskFailed<SqlException> (
				cmd.ExecuteNonQueryAsync (cts.Token), NetworkConfig.LongTimeout);
		}

		[Test]
		[Category("Database")]
		public void TestWait ()
		{
			var cmd = new SqlCommand ("WAITFOR DELAY '00:00:05'", Connection);
			var task = cmd.ExecuteNonQueryAsync ();
			AssertEx.TaskCompleted (task, TimeSpan.FromSeconds (15));
		}

		[Test]
		[Category("Sync")]
		[Category("NotWorking")]
		public void SyncTimeoutAndRead ()
		{
			var cmd = new SqlCommand ("WAITFOR DELAY '00:05:00'", Connection);
			cmd.CommandTimeout = NetworkConfig.CommandTimeout;
			var start = DateTime.Now;

			try {
				cmd.ExecuteNonQuery ();
				Assert.Fail ();
			} catch (SqlException) {
				Assert.That (DateTime.Now - start < TimeSpan.FromSeconds (2 * NetworkConfig.CommandTimeout));
			}

			var select = CreateCommand ("SELECT * FROM test");
			var reader = select.ExecuteReader ();
			reader.Close ();
		}

		public static void DumpSqlException (SqlException ex)
		{
			Console.WriteLine ("SQL EXCEPTION: {0:x} {1:x} {2:x} {3:x} - {4}",
			                   ex.Class, ex.ErrorCode, ex.Number,
			                   ex.State, ex.Errors.Count);
			foreach (var obj in ex.Errors) {
				var error = (SqlError)obj;
				Console.WriteLine ("  ERROR: {0} - {1:x} {2} {3} {4} {5} - |{6}|",
				                   error, error.Class, error.Number, error.State,
				                   error.Source, error.Server, error.Procedure);
			}
		}

		[Test]
		[Category("Cancel")]
		public void CancelAndRead ()
		{
			var cmd = CreateCommand ("WAITFOR DELAY '00:05:00'");
			var cts = new CancellationTokenSource (NetworkConfig.NetworkTimeout);
			var task = cmd.ExecuteNonQueryAsync (cts.Token);
			AssertEx.TaskFailed<SqlException> (task, NetworkConfig.LongTimeout);

			var select = CreateCommand ("SELECT * FROM test");
			var reader = AssertEx.TaskCompleted (select.ExecuteReaderAsync (), NetworkConfig.NetworkTimeout);
			reader.Close ();
		}

		[Test]
		[Category("Database")]
		public void TestParams ()
		{
			var cmd = CreateCommand ("SELECT * FROM test WHERE a = @a");
			var param = new SqlParameter ("a", SqlDbType.Int);
			param.Value = 1;
			cmd.Parameters.Add (param);

			cmd.Prepare ();

			var reader = AssertEx.TaskCompleted (cmd.ExecuteReaderAsync (), NetworkConfig.NetworkTimeout);
			reader.Close ();
		}

		[Test]
		[Category("Database")]
		public void DelayProc ()
		{
			var cmd = CreateCommand ("Delay");
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandTimeout = 10;

			var param = new SqlParameter ("@delay", SqlDbType.DateTime);
			param.Value = DateTime.Parse ("00:00:01");
			cmd.Parameters.Add (param);

			AssertEx.TaskCompleted (cmd.ExecuteNonQueryAsync (), TimeSpan.FromSeconds (15));
		}

		[Test]
		[Category("Database")]
		public void TestXmlReader ()
		{
			var cmd = CreateCommand ("SELECT * FROM test FOR XML AUTO");
			var xml = AssertEx.TaskCompleted (cmd.ExecuteXmlReaderAsync (), NetworkConfig.NetworkTimeout);
			xml.Close ();

			cmd = CreateCommand ("SELECT * FROM test");
			var reader = AssertEx.TaskCompleted (cmd.ExecuteReaderAsync (), NetworkConfig.NetworkTimeout);
			Assert.That (reader.FieldCount > 0);
			reader.Close ();
		}
	}
}
