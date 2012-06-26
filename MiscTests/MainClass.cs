using System;
using NUnit.Framework;
using MonoTests.System.Net.Sockets;

namespace MiscTests
{
	public class MainClass
	{
		public static void Main (string[] args)
		{
			var test = new UdpClientTest ();
			test.Constructor5 ();
		}
	}
}
