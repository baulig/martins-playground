using System;
using System.Configuration;

namespace TestSqlClient
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
