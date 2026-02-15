using System.Collections.Generic;

namespace Jnot.Excel.Interop
{
    public class PivotData
    {
        public string SampleDateRaw { get; set; } = "";
        public string JobNumberRaw { get; set; } = "";
        public List<PivotPair> Pairs { get; set; } = new();

        public PivotData() { }

        public PivotData(string sampleDateRaw, List<PivotPair> pairs)
        {
            SampleDateRaw = sampleDateRaw;
            Pairs = pairs;
        }

        public PivotData(string sampleDateRaw, string jobNumberRaw, List<PivotPair> pairs)

        {
            SampleDateRaw = sampleDateRaw;
            JobNumberRaw = jobNumberRaw;
            Pairs = pairs;
        }
    }
    public class PivotPair
    {
        public string Parameter { get; set; } = "";
        public string SiteId { get; set; } = "";

        public PivotPair() { }

        public PivotPair(string parameter, string siteID)
        {
            Parameter = parameter;
            SiteId = siteID;
        }
        
   }
}

