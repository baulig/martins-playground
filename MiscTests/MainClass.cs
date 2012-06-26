using System;
using NUnit.Framework;

namespace MiscTests
{
	public class MainClass
	{
		public static void Main (string[] args)
		{
			// Randomly generated time outside DST.
			var utc = new DateTime (1993, 01, 28, 08, 49, 48, DateTimeKind.Utc);
			var local = utc.ToLocalTime ();
			var unspecified = new DateTime (1993, 01, 28, 08, 49, 48);

			Assert.AreEqual (DateTimeKind.Utc, utc.Kind);
			Assert.AreEqual (DateTimeKind.Local, local.Kind);
			Assert.AreEqual (DateTimeKind.Unspecified, unspecified.Kind);

			Assert.AreEqual (628638077880000000, utc.Ticks);
			Console.WriteLine (local.Ticks - utc.Ticks);

			var offset = TimeZone.CurrentTimeZone.GetUtcOffset (local);

			var utcFt = utc.ToFileTime ();
			var localFt = local.ToFileTime ();
			var unspecifiedFt = unspecified.ToFileTime ();

			var utcUft = utc.ToFileTimeUtc ();
			var localUft = local.ToFileTimeUtc ();
			var unspecifiedUft = unspecified.ToFileTimeUtc ();

			Console.WriteLine ("TEST: {0} - {1} {2} {3} - {4} {5} {6}", offset,
			                   utcFt, localFt, unspecifiedFt,
			                   utcUft, localUft, unspecifiedUft);

			Assert.AreEqual (123726845880000000, utcFt);
			Assert.AreEqual (utcFt, localFt);

			Assert.AreEqual (offset.Ticks, utcFt - unspecifiedFt);

			Assert.AreEqual (utcFt, utcUft);
			Assert.AreEqual (utcFt, localUft);
			Assert.AreEqual (utcFt, unspecifiedUft);
		}
	}
}
