using System;
using System.Collections.Generic;
using Microsoft.AppCenter.Analytics;
using mTIM.Helpers;

namespace mTIM.Managers
{
	public static class AnalyticsManager
	{
		public static void TrackEvent(string eventMessage, AnalyticsType analyticsType = AnalyticsType.Method)
        {
            var dict = new Dictionary<string, string>();
            dict.Add(GetTypeString(analyticsType), eventMessage);
			Analytics.TrackEvent(string.Format("Device :{0}", GlobalConstants.GetUser()), dict);
		}

		private static string GetTypeString(AnalyticsType analyticsType)
		{
			string type = string.Empty;
			switch(analyticsType)
			{
				case AnalyticsType.Method:
					type = "Method";
					break;
				case AnalyticsType.ClickorSelected:
					type = "Selected Or Clicked";
                    break;
				default:
					type = "Message";
                    break;
			}
			return type;
		}

    }

	public enum AnalyticsType
	{
		Method,
		ClickorSelected,
		Message
	}
}

