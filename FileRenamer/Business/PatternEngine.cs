using JNOT.FileRenamer.ExcelInterop;
using System.Linq;
using Tomlyn;

namespace JNOT.FileRenamer.Business
{
    public class PatternEngine
    {
        private readonly Pattern[] _patterns;

        public PatternEngine()
        {
            _patterns = new[]
            {
                BuildWeekendPattern(),
                BuildDailyPattern(),
                BuildWeeklyPattern(),
                BuildMonthlyPattern()
            };
        }

        public string ResolveTypeCode(PivotData data)
        {
            foreach (var p in _patterns)
            {
                if (Matches(p, data))
                    return p.TypeCode;
            }

            return "X";
        }

        private bool Matches(Pattern p, PivotData data)
        {
            var sites = data.Pairs.Select(x => x.SiteId).ToList();
            var distinctSites = sites.Distinct().ToList();

            var parameters = data.Pairs.Select(x => x.Parameter).ToList();
            var distinctParams = parameters.Distinct().ToList();

            // Cardinality checks
            if (p.RequiredSiteCount.HasValue &&
                distinctSites.Count != p.RequiredSiteCount.Value)
                return false;

            if (p.RequiredParameterCount.HasValue &&
                distinctParams.Count != p.RequiredParameterCount.Value)
                return false;

            // RequiredSites as a list (duplicates allowed)
            if (!p.RequiredSites.All(rs => distinctSites.Contains(rs)))
                return false;

            if (!p.RequiredParameters.All(rp => distinctParams.Contains(rp)))
                return false;

            // Required (Parameter, SiteId) pairs
            foreach (var kv in p.RequiredPairs)
            {
                if (!data.Pairs.Any(x => x.Parameter == kv.Parameter && x.SiteId == kv.SiteId))
                    return false;
            }
            return true;
        }

        private Pattern BuildWeekendPattern()
        {
            return new Pattern
            {
                TypeCode = "S",
                RequiredSites = { "EFA-2" },
                RequiredParameters = { "Total Suspended Solids" },
                RequiredPairs = { ( "Total Suspended Solids", "EFA-2" ) },
                RequiredSiteCount = 1,
                RequiredParameterCount = 1
            };
        }

        private Pattern BuildDailyPattern()
        {
            return new Pattern
            {
                TypeCode = "D",
                RequiredSites = { "EFA-2", "EFA-2", "EFA-2" }, // now preserved
                RequiredParameters =
                {
                    "Total Suspended Solids",
                    "Carbonaceous Biochemical Oxygen Demand",
                    "Coliform, Fecal"
                },
                RequiredPairs =
                {
                    ( "Total Suspended Solids", "EFA-2" ),
                    ( "Carbonaceous Biochemical Oxygen Demand", "EFA-2" ),
                    ( "Coliform, Fecal", "EFA-2" )
                },
                RequiredSiteCount = 1,
                RequiredParameterCount = 3
            };
        }

        private Pattern BuildWeeklyPattern()
        {
            return new Pattern
            {
                TypeCode = "W",
                RequiredSites = { "INF-1", "EFA-1", "EFA-1 CCC #1", "EFA-1 CCC #2" },
                RequiredParameters =
                {
                    "Total Suspended Solids",
                    "Carbonaceous Biochemical Oxygen Demand",
                    "Coliform, Fecal"
                },
                RequiredPairs =
                {
                    ( "Total Suspended Solids", "INF-1" ),
                    ( "Total Suspended Solids", "EFA-1" ),
                    ( "Carbonaceous Biochemical Oxygen Demand", "INF-1" ),
                    ( "Coliform, Fecal", "EFA-1 CCC #1" ),
                    ( "Coliform, Fecal", "EFA-1 CCC #2" )
                },
                RequiredSiteCount = 4,
                RequiredParameterCount = 3
            };
        }

        private Pattern BuildMonthlyPattern()
        {
            return new Pattern
            {
                TypeCode = "M",
                RequiredSites = { "INF-1", "EFA-1", "EFA-2" },
                RequiredParameters =
                {
                    "Total Dissolved Solids",
                    "Ammonia (as N)",
                    "Nitrogen, Kjeldahl",
                    "Orthophosphate as P, Dissolved",
                    "Total Phosphorus as P",
                    "Nitrogen, Organic",
                    "Nitrogen, Total",
                    "Nitrate as N",
                    "Nitrate Nitrite as N",
                    "Nitrite as N"
                },
                RequiredPairs =
                {
                    ( "Total Dissolved Solids", "EFA-1" ),
                    ( "Ammonia (as N)","INF-1" ),
                    ( "Ammonia (as N)","EFA-2" ),
                    ( "Nitrogen, Kjeldahl","INF-1" ),
                    ( "Nitrogen, Kjeldahl","EFA-2" ),
                    ( "Orthophosphate as P, Dissolved","INF-1" ),
                    ( "Orthophosphate as P, Dissolved","EFA-2" ),
                    ( "Total Phosphorus as P","INF-1" ),
                    ( "Total Phosphorus as P","EFA-2" ),
                    ( "Nitrogen, Organic","INF-1" ),
                    ( "Nitrogen, Organic","EFA-2" ),
                    ( "Nitrogen, Total","INF-1" ),
                    ( "Nitrogen, Total","EFA-2" ),
                    ( "Nitrate as N", "INF-1"),
                    ( "Nitrate as N","EFA-2" ),
                    ( "Nitrate Nitrite as N","INF-1" ),
                    ( "Nitrate Nitrite as N","EFA-2" ),
                    ( "Nitrite as N", "INF-1"),
                    ( "Nitrite as N","EFA-2" )
                },
                RequiredSiteCount = 3,
                RequiredParameterCount = 10
            };
        }
    }
}
