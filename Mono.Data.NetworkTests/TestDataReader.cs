//
// TestDatabase.cs
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
	public class TestDataReader : DatabaseTestFixture
	{
		public string TableName {
			get;
			private set;
		}

		[SetUp]
		public override void SetUp ()
		{
			base.SetUp ();

			TableName = string.Format ("tmp{0}", DateTime.Now.Ticks);
			ExecuteNonQuery ("CREATE TABLE {0} (a int, b text)", TableName);

			ExecuteNonQuery ("INSERT INTO {0} VALUES (1, 'Async Monkey')", TableName);
			ExecuteNonQuery ("INSERT INTO {0} VALUES (9, 'World')", TableName);
		}

		[TearDown]
		public override void TearDown ()
		{
			ExecuteNonQuery ("DROP TABLE {0}", TableName);
			base.TearDown ();
		}

		[Test]
		[Category("Database")]
		public void TestReader ()
		{
			var cmd = CreateCommand ("SELECT * FROM {0}", TableName);
			var reader = AssertEx.TaskCompleted (cmd.ExecuteReaderAsync ());

			try {
				var result = AssertEx.TaskCompleted (reader.ReadAsync ());
				Assert.That (result);

				var a = reader.GetFieldValueAsync<int> (0);
				var b = reader.GetFieldValueAsync<string> (1);
				AssertEx.TaskCompleted (Task.WhenAll (a, b));

				Assert.AreEqual (a.Result, 1);
				Assert.AreEqual (b.Result, "Async Monkey");

				result = AssertEx.TaskCompleted (reader.ReadAsync ());
				Assert.That (result);

				var c = reader.GetFieldValueAsync<int> (0);
				var d = reader.GetFieldValueAsync<string> (1);
				AssertEx.TaskCompleted (Task.WhenAll (c, d));

				Assert.AreEqual (c.Result, 9);
				Assert.AreEqual (d.Result, "World");

				Assert.IsFalse (AssertEx.TaskCompleted (reader.ReadAsync ()));
				Assert.IsFalse (AssertEx.TaskCompleted (reader.NextResultAsync ()));
			} finally {
				reader.Close ();
			}
		}
	}
}
