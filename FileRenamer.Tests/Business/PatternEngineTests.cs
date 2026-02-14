using JNOT.FileRenamer.Business;
using JNOT.FileRenamer.ExcelInterop;
using JNOT.FileRenamer.FileSystem;
using Xunit;

namespace JNOT.FileRenamer.Tests.Business
{
    public class PatternEngineTests
    {
        private readonly PatternEngine _engine = new();

        private PivotData MakePivot(params (string Parameter, string SiteId)[] pairs)
        {
            return new PivotData
            {
                Pairs = pairs
                    .Select(p => new PivotPair
                    {
                        Parameter = p.Parameter,
                        SiteId = p.SiteId
                    })
                    .ToList()
            };
        }

        [Fact]
        public void WeekendPattern_ShouldMatch_SingleTSS_EFA2()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "EFA-2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("S", result);
        }

        [Fact]
        public void DailyPattern_ShouldMatch_ThreeParameters_AllEFA2()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "EFA-2"),
                ("Carbonaceous Biochemical Oxygen Demand", "EFA-2"),
                ("Coliform, Fecal", "EFA-2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("D", result);
        }

        [Fact]
        public void ShouldReturnX_WhenParametersDoNotMatchAnyPattern()
        {
            var data = MakePivot(
                ("pH", "EFA-2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void ShouldReturnX_WhenDailyPatternMissingOneParameter()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "EFA-2"),
                ("Carbonaceous Biochemical Oxygen Demand", "EFA-2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void ShouldReturnX_WhenWeekendPatternHasExtraParameter()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "EFA-2"),
                ("Carbonaceous Biochemical Oxygen Demand", "EFA-2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void ShouldReturnX_WhenDailyPatternHasWrongSite()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "EFA-1"),
                ("Carbonaceous Biochemical Oxygen Demand", "EFA-1"),
                ("Coliform, Fecal", "EFA-1")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void WeeklyPattern_ShouldMatch_AmendedStructure()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "INF-1"),
                ("Total Suspended Solids", "EFA-1"),
                ("Carbonaceous Biochemical Oxygen Demand", "INF-1"),
                ("Coliform, Fecal", "EFA-1 CCC #1"),
                ("Coliform, Fecal", "EFA-1 CCC #2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("W", result);
        }
        [Fact]
        public void WeeklyPattern_ShouldReturnX_WhenMissingARequiredPair()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "INF-1"),
                ("Total Suspended Solids", "EFA-1"),
                ("Carbonaceous Biochemical Oxygen Demand", "INF-1"),
                ("Coliform, Fecal", "EFA-1 CCC #1")
            // missing CCC #2
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void WeeklyPattern_ShouldReturnX_WhenDistinctSiteCountIsWrong()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "INF-1"),
                ("Total Suspended Solids", "INF-1"), // duplicate site
                ("Carbonaceous Biochemical Oxygen Demand", "INF-1"),
                ("Coliform, Fecal", "EFA-1 CCC #1"),
                ("Coliform, Fecal", "EFA-1 CCC #2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void WeeklyPattern_ShouldReturnX_WhenParameterIsAtWrongSite()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "INF-1"),
                ("Total Suspended Solids", "EFA-1"),
                ("Carbonaceous Biochemical Oxygen Demand", "EFA-1"), // wrong site
                ("Coliform, Fecal", "EFA-1 CCC #1"),
                ("Coliform, Fecal", "EFA-1 CCC #2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void WeeklyPattern_ShouldReturnX_WhenExtraParameterPresent()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "INF-1"),
                ("Total Suspended Solids", "EFA-1"),
                ("Carbonaceous Biochemical Oxygen Demand", "INF-1"),
                ("Coliform, Fecal", "EFA-1 CCC #1"),
                ("Coliform, Fecal", "EFA-1 CCC #2"),
                ("pH", "INF-1") // extra parameter
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void WeeklyPattern_ShouldReturnX_WhenSitesAreCorrectButParametersWrong()
        {
            var data = MakePivot(
                ("pH", "INF-1"),
                ("pH", "EFA-1"),
                ("pH", "EFA-1 CCC #1"),
                ("pH", "EFA-1 CCC #2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void MonthlyPattern_ShouldMatch_AmendedStructure()
        {
            var data = MakePivot(
                ("Total Dissolved Solids", "EFA-1"),

                ("Ammonia (as N)", "INF-1"),
                ("Ammonia (as N)", "EFA-2"),

                ("Nitrogen, Kjeldahl", "INF-1"),
                ("Nitrogen, Kjeldahl", "EFA-2"),

                ("Orthophosphate as P, Dissolved", "INF-1"),
                ("Orthophosphate as P, Dissolved", "EFA-2"),

                ("Total Phosphorus as P", "INF-1"),
                ("Total Phosphorus as P", "EFA-2"),

                ("Nitrogen, Organic", "INF-1"),
                ("Nitrogen, Organic", "EFA-2"),

                ("Nitrogen, Total", "INF-1"),
                ("Nitrogen, Total", "EFA-2"),

                ("Nitrate as N", "INF-1"),
                ("Nitrate as N", "EFA-2"),

                ("Nitrate Nitrite as N", "INF-1"),
                ("Nitrate Nitrite as N", "EFA-2"),

                ("Nitrite as N", "INF-1"),
                ("Nitrite as N", "EFA-2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("M", result);
        }

        [Fact]
        public void MonthlyPattern_ShouldReturnX_WhenMissingARequiredPair()
        {
            var data = MakePivot(
                ("Total Dissolved Solids", "EFA-1"),

                ("Ammonia (as N)", "INF-1"),
                ("Ammonia (as N)", "EFA-2"),

                ("Nitrogen, Kjeldahl", "INF-1"),
                ("Nitrogen, Kjeldahl", "EFA-2"),

                ("Orthophosphate as P, Dissolved", "INF-1"),
                ("Orthophosphate as P, Dissolved", "EFA-2"),

                ("Total Phosphorus as P", "INF-1"),
                ("Total Phosphorus as P", "EFA-2"),

                ("Nitrogen, Organic", "INF-1"),
                ("Nitrogen, Organic", "EFA-2"),

                ("Nitrogen, Total", "INF-1"),
                ("Nitrogen, Total", "EFA-2"),

                ("Nitrate as N", "INF-1"),
                ("Nitrate as N", "EFA-2"),

                ("Nitrate Nitrite as N", "INF-1"),
                ("Nitrate Nitrite as N", "EFA-2")

            // Missing ("Nitrite as N", "INF-1") and ("Nitrite as N", "EFA-2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void MonthlyPattern_ShouldReturnX_WhenDistinctSiteCountIsWrong()
        {
            var data = MakePivot(
                ("Total Dissolved Solids", "EFA-1"),

                ("Ammonia (as N)", "INF-1"),
                ("Ammonia (as N)", "INF-1"), // EFA-2 missing

                ("Nitrogen, Kjeldahl", "INF-1"),
                ("Nitrogen, Kjeldahl", "INF-1"), // EFA-2 missing

                ("Orthophosphate as P, Dissolved", "INF-1"),
                ("Orthophosphate as P, Dissolved", "INF-1"),

                ("Total Phosphorus as P", "INF-1"),
                ("Total Phosphorus as P", "INF-1"),

                ("Nitrogen, Organic", "INF-1"),
                ("Nitrogen, Organic", "INF-1"),

                ("Nitrogen, Total", "INF-1"),
                ("Nitrogen, Total", "INF-1"),

                ("Nitrate as N", "INF-1"),
                ("Nitrate as N", "INF-1"),

                ("Nitrate Nitrite as N", "INF-1"),
                ("Nitrate Nitrite as N", "INF-1"),

                ("Nitrite as N", "INF-1"),
                ("Nitrite as N", "INF-1")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void MonthlyPattern_ShouldReturnX_WhenParameterIsAtWrongSite()
        {
            var data = MakePivot(
                ("Total Dissolved Solids", "INF-1"), // wrong site

                ("Ammonia (as N)", "INF-1"),
                ("Ammonia (as N)", "EFA-2"),

                ("Nitrogen, Kjeldahl", "INF-1"),
                ("Nitrogen, Kjeldahl", "EFA-2"),

                ("Orthophosphate as P, Dissolved", "INF-1"),
                ("Orthophosphate as P, Dissolved", "EFA-2"),

                ("Total Phosphorus as P", "INF-1"),
                ("Total Phosphorus as P", "EFA-2"),

                ("Nitrogen, Organic", "INF-1"),
                ("Nitrogen, Organic", "EFA-2"),

                ("Nitrogen, Total", "INF-1"),
                ("Nitrogen, Total", "EFA-2"),

                ("Nitrate as N", "INF-1"),
                ("Nitrate as N", "EFA-2"),

                ("Nitrate Nitrite as N", "INF-1"),
                ("Nitrate Nitrite as N", "EFA-2"),

                ("Nitrite as N", "INF-1"),
                ("Nitrite as N", "EFA-2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void MonthlyPattern_ShouldReturnX_WhenExtraParameterPresent()
        {
            var data = MakePivot(
                ("Total Dissolved Solids", "EFA-1"),

                ("Ammonia (as N)", "INF-1"),
                ("Ammonia (as N)", "EFA-2"),

                ("Nitrogen, Kjeldahl", "INF-1"),
                ("Nitrogen, Kjeldahl", "EFA-2"),

                ("Orthophosphate as P, Dissolved", "INF-1"),
                ("Orthophosphate as P, Dissolved", "EFA-2"),

                ("Total Phosphorus as P", "INF-1"),
                ("Total Phosphorus as P", "EFA-2"),

                ("Nitrogen, Organic", "INF-1"),
                ("Nitrogen, Organic", "EFA-2"),

                ("Nitrogen, Total", "INF-1"),
                ("Nitrogen, Total", "EFA-2"),

                ("Nitrate as N", "INF-1"),
                ("Nitrate as N", "EFA-2"),

                ("Nitrate Nitrite as N", "INF-1"),
                ("Nitrate Nitrite as N", "EFA-2"),

                ("Nitrite as N", "INF-1"),
                ("Nitrite as N", "EFA-2"),

                ("pH", "INF-1") // extra parameter
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }


    }
}
