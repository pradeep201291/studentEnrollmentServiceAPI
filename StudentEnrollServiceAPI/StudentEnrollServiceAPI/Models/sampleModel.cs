using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StudentEnrollServiceAPI.Models
{
    public class SampleModel
    {
        public string connectionname { get; set; } = "";
        public string hostipaddress { get; set; } = "";
        public string orderentryport { get; set; } = "";
        public string customerserviceport { get; set; } = "";
        public string database { get; set; } = "";
        public string wsdl { get; set; } = "";
        public bool adcredentials { get; set; } = false;
    }

    public class ConfigModel
    {
        public string env { get; set; }
        public string apiPort { get; set; }
        public string url { get; set; }
        public string domain { get; set; }
        public List<SampleModel> sampleModel { get; set; }
    }
}