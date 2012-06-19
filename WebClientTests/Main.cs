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
		string tempPath;

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
			tempPath = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			Directory.CreateDirectory (tempPath);

			Console.WriteLine ("TEMP PATH: {0}", tempPath);
			try {
				DoRun ();
			} finally {
				Directory.Delete (tempPath, true);
			}
		}

		void DoRun ()
		{
			var proc = Process.GetCurrentProcess ();
			var oneMb = (long) Math.Pow (2, 20);

			var startPrivateMem = proc.PrivateMemorySize64;
			var startVirtualMem = proc.VirtualMemorySize64;
			var lastPrivateMem = startPrivateMem;
			var lastVirtualMem = startVirtualMem;

			for (int i = 0; i < 200; i++) {
				Download ();

				var privateMem = proc.PrivateMemorySize64 - startPrivateMem;
				var virtualMem = proc.VirtualMemorySize64 - startVirtualMem;

				var gainPrivateMem = proc.PrivateMemorySize64 - lastPrivateMem;
				var gainVirtualMem = proc.VirtualMemorySize64 - lastVirtualMem;
				lastPrivateMem = proc.PrivateMemorySize64;
				lastVirtualMem = proc.VirtualMemorySize64;

				Console.WriteLine ("ITERATION #{0}: {1} - {2} {3} - {4} {5}", i,
				                   GetThreadCount (), privateMem / oneMb,
				                   virtualMem / oneMb, gainPrivateMem / oneMb,
				                   gainVirtualMem / oneMb);

				if (Math.Max (privateMem, virtualMem) / oneMb > 2000)
					throw new OutOfMemoryException ();
			}
		}

		void Download ()
		{
			var server = new Server ();

			var progress = new ManualResetEventSlim (false);

			var client = new WebClient ();
			client.DownloadDataCompleted += (sender, e) => {
				Console.WriteLine ("DOWNLOAD COMPLETED: {0} {1}",
				                   e.Cancelled, e.Error);
			};
			client.DownloadFileCompleted += (sender, e) => {
				Console.WriteLine ("DOWNLOAD COMPLETE: {0} {1}",
				                   e.Cancelled, e.Error);
			};
			client.DownloadProgressChanged += (sender, e) => {
				Console.WriteLine ("PROGRESS CHANGED: {0} {1} {2}",
				                   e.BytesReceived, e.TotalBytesToReceive,
				                   e.ProgressPercentage);
				progress.Set ();
			};

			var file = Path.Combine (tempPath, Path.GetRandomFileName ());

			var uri = new Uri ("http://localhost:" + server.Port + "/");
			client.DownloadFileAsync (uri, file);
			progress.Wait (5000);
			client.CancelAsync ();
		}
	}
}
