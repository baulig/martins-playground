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

		[TestFixtureSetUp]
		public void SetUp ()
		{
			Connection = new SqlConnection (ConnectionString);
			Assert.That (Connection.OpenAsync ().Wait (2500));
		}

		[TestFixtureTearDown]
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
			Assert.That (create.ExecuteNonQueryAsync ().Wait (1000));

			var drop = new SqlCommand (
				string.Format ("DROP TABLE {0}", tmpname), Connection);
			Assert.That (drop.ExecuteNonQueryAsync ().Wait (1000));
		}

		[Test]
		public void ReadTest ()
		{
			var cmd = new SqlCommand ("SELECT * FROM test", Connection);
			var task = cmd.ExecuteReaderAsync ();
			Assert.That (task.Wait (2500));

			var reader = task.Result;
			Assert.That (reader.FieldCount > 0);
		}
	}
}
