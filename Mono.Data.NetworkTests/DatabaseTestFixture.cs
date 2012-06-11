//
// DatabaseTestFixture.cs
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
using System.Data.SqlClient;
using NUnit.Framework;

namespace Mono.Data.NetworkTests
{
	public abstract class DatabaseTestFixture
	{
		public SqlConnection Connection {
			get;
			private set;
		}

		protected SqlCommand CreateCommand (string cmdText, params object[] args)
		{
			var cmd = new SqlCommand (string.Format (cmdText, args), Connection);
			cmd.CommandTimeout = NetworkConfig.CommandTimeout;
			return cmd;
		}

		protected void ExecuteNonQuery (string cmdText, params object[] args)
		{
			var sql = string.Format (cmdText, args);
			var cmd = new SqlCommand (sql, Connection);
			cmd.CommandTimeout = NetworkConfig.CommandTimeout;
			AssertEx.TaskCompleted (
				cmd.ExecuteNonQueryAsync (), NetworkConfig.NetworkTimeout, sql);
		}

		[SetUp]
		public virtual void SetUp ()
		{
			Connection = new SqlConnection (NetworkConfig.ConnectionString);
			AssertEx.TaskCompleted (Connection.OpenAsync (), NetworkConfig.NetworkTimeout,
			                        "Database connection failed.");
		}

		[TearDown]
		public virtual void TearDown ()
		{
			Connection.Close ();
		}
	}
}
