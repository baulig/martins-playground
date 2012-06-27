using System;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using MonoTests.System.Net.Sockets;

namespace MiscTests
{
	public class MainClass
	{
		public static void Main (string[] args)
		{
			foreach (var addr in Dns.GetHostAddresses ("localhost")) {
				Console.WriteLine ("LOCALHOST: {0}", addr);
			}
			Console.WriteLine ();

			var entry = Dns.GetHostEntry (string.Empty);
			Console.WriteLine ("HOST ENTRY: {0} {1}", entry, entry.HostName);
			foreach (var addr in entry.AddressList)
				Console.WriteLine ("HOST ADDR: {0}", addr);
			foreach (var alias in entry.Aliases)
				Console.WriteLine ("HOST ALIAS: {0}", alias);

			var test = new UdpClientTest ();
			test.JoinMulticastGroup1_IPv6 ();
		}
	}
}
