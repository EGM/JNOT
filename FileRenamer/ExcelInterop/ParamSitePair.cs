namespace JNOT.FileRenamer.ExcelInterop
{
    public class ParamSitePair
    {
        public string Parameter { get; }
        public string SiteId { get; }

        public ParamSitePair(string parameter, string siteId)
        {
            Parameter = parameter;
            SiteId = siteId;
        }
    }
}
