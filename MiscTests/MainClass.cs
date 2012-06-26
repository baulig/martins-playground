using System;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using MonoTests.System.Net.Sockets;

namespace MiscTests
{
	public class MainClass
	{
		public static void IPv6Test ()
		{
			var ep = new IPEndPoint (IPAddress.IPv6Any, IPEndPoint.MaxPort);
			Assert.AreEqual (IPAddress.IPv6Any, ep.Address);

			Console.WriteLine (IPAddress.IPv6Any.ScopeId);
			var data = new byte [16];
			data [10] = 0xff;
			data [11] = 0xff;
			var addr = new IPAddress (data, 0);
			Console.WriteLine (addr);
		}

		public static void Main (string[] args)
		{
			var ep = new IPEndPoint (IPAddress.IPv6Any, IPEndPoint.MaxPort);
			Console.WriteLine ("TEST: {0} {1}", IPAddress.IPv6Any, ep.Address);

			IPv6Test ();

			Console.WriteLine ("OK");

			var test = new UdpClientTest ();
			test.Constructor5 ();
		}
	}
}
