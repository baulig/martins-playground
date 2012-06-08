using System;
using NUnit.Framework;

namespace TestSqlClient
{
	public static class AssertEx
	{
		public static void Exception<T> (Action action)
			where T:Exception
		{
			Exception<T> (action, null);
		}

		public static void Exception<T> (Action action, string message)
			where T:Exception
		{
			var prefix = message != null ? message + ": " : "";
			try {
				action ();
				Assert.Fail (string.Format (
					"{0}Expected exception of type {1}", prefix, typeof (T)));
			} catch (Exception ex) {
				if (ex is AssertionException)
					throw;
				if (ex is AggregateException)
					ex = ((AggregateException) ex).InnerException;
				if (ex is T)
					return;
				Assert.Fail (string.Format (
					"{0}Expected exception of type {1}, but got {2}",
					prefix, typeof (T), ex));
			}
		}
	}
}
