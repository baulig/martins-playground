using System;
using NUnit.Framework;

namespace MiscTests
{
	public class MainClass
	{
		public static void Main (string[] args)
		{
			var test = new MonoTests.System.Net.Sockets.SocketTest ();
			test.ConnectedProperty ();
		}
	}
}
