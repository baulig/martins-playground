using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Threading;
using System.Threading.Tasks;

namespace WebClientTests
{
	class Test
	{
		public static void Main (string[] args)
		{
			var test = new Test ();
			test.Run ();
		}

		static int GetThreadCount ()
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				return Process.GetCurrentProcess ().Threads.Count;

			return Directory.GetDirectories ("/proc/self/task").Length;
		}

		void Run ()
		{
			var proc = Process.GetCurrentProcess ();

			var oneMb = (long) Math.Pow (2, 20);

			for (int i = 0; i < 20; i++) {
				Console.WriteLine ("ITERATION #{0}: {1} - {2} {3}", i,
				                   GetThreadCount (),
				                   proc.PrivateMemorySize64 / oneMb,
				                   proc.VirtualMemorySize64 / oneMb);
				Download ();
			}
		}

		void Download ()
		{
			var server = new Server ();

			var progress = new ManualResetEventSlim (false);

			var client = new WebClient ();
			client.DownloadDataCompleted += (sender, e) => {
				Console.WriteLine ("DOWNLOAD COMPLETED: {0}", e.Error);
			};
			client.DownloadProgressChanged += (sender, e) => {
				Console.WriteLine ("PROGRESS CHANGED: {0} {1} {2}",
				                   e.BytesReceived, e.TotalBytesToReceive,
				                   e.ProgressPercentage);
				progress.Set ();
			};

			var uri = new Uri ("http://localhost:" + server.Port + "/");
			client.DownloadDataTaskAsync (uri);
			progress.Wait (5000);
			client.CancelAsync ();
		}
	}
}