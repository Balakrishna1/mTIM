using System;
using System.Collections.Generic;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace mTIM
{
	public static class CrashReportManager
	{
		public static void ReportError(Exception ex, string method , string tag = "mTIM")
		{
			var dict = new Dictionary<string, string>();
			dict.Add(tag, method);
			Crashes.TrackError(ex, dict);
		}
	}
}

