using System;
using mTIM.Models;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace mTIM.Helpers
{
    public class PostResultHelper
    {
        readonly string tag = "PostResultHelper";
        public static PostResultHelper _instance;
        public static PostResultHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PostResultHelper();
                }

                return _instance;
            }
        }

        public async Task<List<TaskResult>> GetOfflineResults()
        {
            List<TaskResult> list = new List<TaskResult>();
            if (FileHelper.IsFileExists(GlobalConstants.POST_RESULT))
            {
                var json = await FileHelper.ReadTextAsync(GlobalConstants.POST_RESULT);
                list = JsonConvert.DeserializeObject<List<TaskResult>>(json);
            }
            return list;
        }

        public async Task SaveTaskResults(TaskResult[] taskResults)
        {
            try
            {
                var list = await GetOfflineResults();
                list.AddRange(taskResults);
                var json = JsonConvert.SerializeObject(list);
                await FileHelper.WriteTextAsync(GlobalConstants.POST_RESULT, json);
            }
            catch (Exception ex)
            {
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, tag);
            }
        }
    }
}

