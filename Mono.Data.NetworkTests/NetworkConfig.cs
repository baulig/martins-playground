//
// NetworkConfig.cs
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
using System.Configuration;

namespace Mono.Data.NetworkTests
{
	public static class NetworkConfig
	{
		public static string ConnectionString {
			get { return ConfigurationManager.AppSettings ["ConnectionString"]; }
		}

		static TimeSpan GetTimeSpan (string name, int timeout)
		{
			var value = ConfigurationManager.AppSettings [name];
			if (value != null)
				timeout = int.Parse (value);
			return TimeSpan.FromMilliseconds (timeout);
		}

		public static TimeSpan NetworkTimeout {
			get {
				if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
					// We're on the same machine.
					return GetTimeSpan ("NetworkTimeout", 750);
				} else {
					// We're using a network connection.
					return GetTimeSpan ("RemoteNetworkTimeout", 2500);
				}
			}
		}

		public static TimeSpan LongTimeout {
			get {
				if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
					// Must be more than one second.
					return GetTimeSpan ("LongTimeout", 2500);
				} else {
					return GetTimeSpan ("RemoteLongTimeout", 6000);
				}
			}
		}

		public static int CommandTimeout {
			get {
				var timeout = ConfigurationManager.AppSettings ["CommandTimeout"];
				if (timeout != null)
					return int.Parse (timeout);
				else
					return (int)(NetworkTimeout.TotalMilliseconds + 999) / 1000;
			}
		}
	}
}
