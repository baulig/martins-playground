using System;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace TestSqlClient
{
	public class TestConnection
	{
		public string ConnectionString {
			get;
			private set;
		}

		public TestConnection (string connectionString)
		{
			this.ConnectionString = connectionString;
		}

		public async Task Open ()
		{
			var conn = new SqlConnection (ConnectionString);
			await conn.OpenAsync ();

			Console.WriteLine ("DATABASE OPENED!");
		}
	}
}
