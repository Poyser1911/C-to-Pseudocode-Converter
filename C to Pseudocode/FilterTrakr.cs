using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace C_to_Pseudocode
{
    public class FilterOptions
    {
        public bool RemoveComments { get; set; }
        public bool RemoveColours { get; set; }
        public bool EnableLiveUpdate { get; set; }
        public bool UseArrowEqual { get; set; }
        public bool RemoveFuncPrototype { get; set; }
        public bool EnableAutoIndent { get; set; }
        public bool RemoveIncludes { get; set; }
    }
    class FilterTrakr
    {
        private string filename;
        public FilterOptions filter { get; set; }
        public FilterTrakr(string _filename = "filter.json")
        {
            filename = _filename;
            try
            {
                filter = JsonConvert.DeserializeObject<FilterOptions>(File.ReadAllText(filename));
            }
            catch (Exception) { }
        }
        public void Save()
        {
            File.WriteAllText(filename, JsonConvert.SerializeObject(filter));
        }

    }
}
