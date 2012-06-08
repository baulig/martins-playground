using System;
using NUnit.Framework;

namespace TestSqlClient
{
	public static class AssertEx
	{
		public static void Exception<T> (Action action, string message)
			where T:Exception
		{
			try {
				action ();
				Assert.Fail (message);
			} catch (Exception ex) {
				if (ex is AggregateException)
					ex = ((AggregateException) ex).InnerException;
				Assert.IsInstanceOfType (typeof (T), ex, message);
			}
		}

		public static void Exception (Type t, Action action, string message)
		{
			Exception (t, action, message);
		}

		public static void Exception (Type t, Action action, string message, bool allowAggregate)
		{
			try {
				action ();
				Assert.Fail (message);
			} catch (Exception ex) {
				if (allowAggregate && (ex is AggregateException))
					ex = ((AggregateException) ex).InnerException;
				Assert.IsInstanceOfType (t, ex, message);
			}
		}
	}
}
