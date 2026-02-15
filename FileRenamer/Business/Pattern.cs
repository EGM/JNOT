using System.Collections.Generic;

namespace Jnot.FileRenamer.Business
{
    public class Pattern
    {
        public string TypeCode { get; set; } = string.Empty;

        // These allow duplicates and preserve order
        public List<string> RequiredSites { get; } = new();
        public List<string> RequiredParameters { get; } = new();

        // This MUST allow duplicates — Weekly and Monthly depend on it
        public List<(string Parameter, string SiteId)> RequiredPairs { get; } = new();

        // Cardinality constraints
        public int? RequiredSiteCount { get; set; }
        public int? RequiredParameterCount { get; set; }
    }
}
