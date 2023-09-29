namespace Elars.CsvToSql.Core
{
    public class Options
    {
        public bool CreateTable { get; set; } = true;
        public bool AllowSpaces { get; set; } = false;

        public string TableName { get; set; } = "#tmp";


        public int BatchSize { get; set; } = 50;

        public bool IdentityInsert { get; set; } = false;

        public bool NoCount { get; set; } = true;


        public bool ClusteredIndex { get; set; } = false;
        public int? IndexColumn { get; set; } = null;

        public bool Reseed { get; set; } = false;

        public int StringSize { get; set; } = 100;
        public int DecimalPlaces { get; set; } = 2;
        public bool IncludeTime { get; set; } = false;
        public bool TruncateTable { get; internal set; }
    }
}