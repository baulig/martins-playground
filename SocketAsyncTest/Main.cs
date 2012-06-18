using System;

class MainClass
{
	public static void Main (string[] args)
	{
		var client = new Client ();

		byte[] buffer = new byte [512];
		client.Test (buffer, 500);
		client.Wait ();
	}
}
