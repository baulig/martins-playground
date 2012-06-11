//
// AssertEx.cs
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
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Mono.Data.NetworkTests
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
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				if (ex is AggregateException)
					ex = ((AggregateException) ex).InnerException;
				if (ex is T)
					return;
				Assert.Fail (string.Format (
					"{0}Expected exception of type {1}, but got {2}",
					prefix, typeof (T), ex));
			}
		}

		public static void TaskCompleted (Task task)
		{
			TaskCompleted (task, NetworkConfig.NetworkTimeout, null);
		}

		public static void TaskCompleted (Task task, int timeout)
		{
			TaskCompleted (task, timeout, null);
		}

		public static void TaskCompleted (Task task, int timeout, string message)
		{
			TaskCompleted (task, TimeSpan.FromMilliseconds (timeout), message);
		}

		public static void TaskCompleted (Task task, TimeSpan timeout)
		{
			TaskCompleted (task, timeout, null);
		}

		public static void TaskCompleted (Task task, TimeSpan timeout, string message)
		{
			var suffix = message != null ? ": " + message : "";
			try {
				if (!task.Wait (timeout))
					Assert.Fail ("Task timed-out" + suffix);
			} catch (OperationCanceledException) {
				Assert.Fail ("Task canceled" + suffix);
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.Fail (string.Format ("Task threw exception '{0}'{1}", ex, suffix));
			}
		}

		public static TResult TaskCompleted<TResult> (Task<TResult> task)  
		{
			return TaskCompleted (task, NetworkConfig.NetworkTimeout, null);
		}

		public static TResult TaskCompleted<TResult> (Task<TResult> task, int timeout)
		{
			return TaskCompleted (task, timeout, null);
		}

		public static TResult TaskCompleted<TResult> (Task<TResult> task, int timeout, string message)
		{
			return TaskCompleted (task, TimeSpan.FromMilliseconds (timeout), message);
		}

		public static TResult TaskCompleted<TResult> (Task<TResult> task, TimeSpan timeout)
		{
			return TaskCompleted (task, timeout, null);
		}

		public static TResult TaskCompleted<TResult> (Task<TResult> task, TimeSpan timeout, string message)
		{
			var suffix = message != null ? ": " + message : "";
			try {
				if (!task.Wait (timeout))
					Assert.Fail ("Task timed-out" + suffix);
				return task.Result;
			} catch (OperationCanceledException) {
				Assert.Fail ("Task canceled" + suffix);
				throw;
			} catch (Exception ex) {
				Assert.Fail (string.Format ("Task threw exception '{0}'{1}", ex, suffix));
				throw;
			}
		}

		public static void TaskFailed<T> (Task task)
		{
			TaskFailed<T> (task, NetworkConfig.NetworkTimeout, null);
		}

		public static void TaskFailed<T> (Task task, int timeout)
		{
			TaskFailed<T> (task, timeout, null);
		}

		public static void TaskFailed<T> (Task task, int timeout, string message)
		{
			TaskFailed<T> (task, TimeSpan.FromMilliseconds (timeout), message);
		}

		public static void TaskFailed<T> (Task task, TimeSpan timeout)
		{
			TaskFailed<T> (task, timeout, null);
		}

		public static void TaskFailed<T> (Task task, TimeSpan timeout, string message)
		{
			var suffix = message != null ? ": " + message : "";
			try {
				if (!task.Wait (timeout))
					Assert.Fail ("Task timed-out" + suffix);
				Assert.Fail (string.Format (
					"Expected exception of type {0}{1}", typeof (T), suffix));
			} catch (OperationCanceledException) {
				Assert.Fail ("Task canceled" + suffix);
				throw;
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				if (ex is AggregateException)
					ex = ((AggregateException) ex).InnerException;
				if (ex is T)
					return;
				Assert.Fail (string.Format (
					"Expected exception of type {0}, but got {1}{2}",
					typeof (T), ex, suffix));
			}
		}

	}
}
