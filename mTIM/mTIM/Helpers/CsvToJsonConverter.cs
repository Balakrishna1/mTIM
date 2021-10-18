using System;
using Newtonsoft.Json;
using System.Linq;
namespace mTIM.Helpers
{
    public static class CsvToJsonConverter
    {
        public static string ConvertCsvToJson(string data)
        {
            var lines = data.Split("\n");
            if (string.IsNullOrEmpty(lines[lines.Length - 1]))
                lines = lines.SkipLast(1).ToArray();
            //var datas = lines[0].Split(",");
            //var datas = data.Split(':'); // string[] containing each line of the CSV
            var MemberNames = lines[0].Split(','); // the first line, that contains the member names
            var MYObj = lines.Skip(1) // don't take the first line (member names)
                             .Select((x) => x.Split(',') // split columns
                                             /*
                                              * create an anonymous collection
                                              * with object having 2 properties Key and Value
                                              * (and removes the unneeded ")
                                              */
                                             .Select((y, i) => new {
                                                 Key = MemberNames[i].Trim('"'),
                                                 Value = y.Trim('"')
                                             })
                                             // convert it to a Dictionary
                                             .ToDictionary(d => d.Key, d => d.Value));

            // MYObject is IEnumerable<Dictionary<string, string>>
            // serialize (remove indented if needed)
            var Json = JsonConvert.SerializeObject(MYObj, Formatting.None);
            return Json;
        }
    }
}
