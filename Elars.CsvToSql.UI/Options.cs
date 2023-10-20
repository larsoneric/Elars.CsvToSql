using Elars.CsvToSql.Core;
using Elars.CsvToSql.Core.DatabaseTypes;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Elars.CsvToSql.UI
{
    [CategoryOrder("Database", 0)]
    [CategoryOrder("Table", 1)]
    [CategoryOrder("Identity", 2)]
    [CategoryOrder("Fields", 3)]
    [CategoryOrder("Insert SQL", 4)]
    internal class Options
    {
        internal enum IndexTypes
        {
            Clustered,
            Nonclustered
        }

        public Options()
        {
            var converter = new Converter();

            DatabaseType = converter.DatabaseType;
            IsTempTable = converter.IsTempTable;
            TableName = converter.TableName;
            BatchSize = converter.BatchSize;
            UseBatches = converter.BatchSize > 1;
            AllowSpaces = converter.AllowSpaces;
            CreateTable = converter.CreateTable;
            Reseed = converter.Reseed;
            NoCount = converter.NoCount;
            IndexType = converter.ClusteredIndex ? IndexTypes.Clustered : IndexTypes.Nonclustered;
            IdentityInsert = converter.IdentityInsert;
            TruncateTable = converter.TruncateTable;
            StringFields = converter.StringFields.ToList();
            IndexColumn = 1;
            DecimalPlaces = converter.DecimalPlaces;
            StringSize = converter.StringSize;
            UseGoStatements = converter.UseGoStatements;
        }

        [Category("Database")]
        [Display(Name = "Database Type", Order = 0, Description = "Type of database to generate statements for")]
        public DatabaseTypes DatabaseType { get; set; }

        [Category("Database")]
        [Display(Name = "Use GO Statements", Order = 1, Description = "Use GO statements between operations")]
        public bool UseGoStatements { get; set; }

        [Category("Table")]
        [Display(Name = "Table Name", Order = 0, Description = "Name of the database table to insert records into")]
        public string TableName { get; set; }

        [Category("Table")]
        [Display(Name = "Temporary Table", Order = 1, Description = "Use a temporary table to store data")]
        public bool IsTempTable { get; set; }

        [Category("Table")]
        [Display(Name = "Create Table", Order = 2, Description = "Flag indicating whether to generate the CREATE TABLE staement")]
        public bool CreateTable { get; set; }

        [Category("Table")]
        [Display(Name = "Allow Spaces", Order = 3, Description = "Allow spaces in table and column names")]
        public bool AllowSpaces { get; set; }

        [Category("Table")]
        [Display(Name = "Truncate Table", Order = 4, Description = "Delete all records from a table before inserting")]
        public bool TruncateTable { get; set; }


        [Category("Identity")]
        [Display(Name = "Create Index", Order = 0, Description = "Create an index")]
        public bool CreateIndex { get; set; }

        [Category("Identity")]
        [Display(Name = "Index Type", Order = 1, Description = "Create a clustered or non/clustered index")]
        public IndexTypes IndexType { get; set; }

        [Category("Identity")]
        [Display(Name = "Identity Insert", Order = 2, Description = "Toggle IDENTITY_INSERT to allow ID fields to be inserted")]
        public bool IdentityInsert { get; set; }

        [Category("Identity")]
        [Display(Name = "Reseed", Order = 3, Description = "Reseed primary key, useful when IDENTITY_INSERT is on; Assumes the first column is the index")]
        public bool Reseed { get; set; }

        [Category("Identity")]
        [Range(1, int.MaxValue)]
        [Display(Name = "Index Column", Order = 4, Description = "Index of which column to create an index of. If NULL, don't create an index")]
        public int IndexColumn { get; set; }


        [Category("Insert SQL")]
        [Display(Name = "Use Batches", Order = 0, Description = "If false, records will be inserted one at a time")]
        public bool UseBatches { get; set; }

        [Category("Insert SQL")]
        [Display(Name = "Batch Size", GroupName = "Insert SQL", Order = 1, Description = "Size of INSERT batches")]
        [Range(1, int.MaxValue)]
        public int BatchSize { get; set; }

        [Category("Insert SQL")]
        [Display(Name = "NoCount", GroupName = "Insert SQL", Order = 2, Description = "Toggle NOCOUNT to prevent records from being counted")]
        public bool NoCount { get; set; }


        [Category("Fields")]
        [Display(Name = "String Fields", Description = "Fields that should always be treated as strings (e.g. Zip Code)")]
        public List<string> StringFields { get; set; }

        [Category("Fields")]
        [Display(Name = "String Size", Description = "Size for VARCHAR fields")]
        [Range(1, int.MaxValue)]
        public int StringSize { get; set; }

        [Category("Fields")]
        [Display(Name = "Decimal Places", Description = "How many decimal places should DECIMAL fields have")]
        [Range(0, 18)]
        public int DecimalPlaces { get; set; }

        [Category("Fields")]
        [Display(Name = "Include Time", Description = "Include times in Date/Time fields")]
        public bool IncludeTime { get; set; }


        internal bool ShowBatchSize()
        {
            return UseBatches;
        }

        internal bool ShowIdentityInsert()
        {
            return !CreateIndex;
        }

        internal bool ShowIndexType()
        {
            return CreateIndex;
        }

        internal bool ShowReseed()
        {
            return IdentityInsert && !CreateIndex;
        }

        internal bool ShowIndexColumn()
        {
            return CreateIndex || (Reseed && IdentityInsert);
        }

        internal bool ShowTruncateTable()
        {
            return !CreateTable;
        }

        internal Converter ToConverter()
        {
            return new Converter
            {
                IsTempTable = this.IsTempTable,
                TableName = this.TableName,
                CreateTable = this.CreateTable,
                Reseed = this.Reseed,
                NoCount = this.NoCount,
                AllowSpaces = this.AllowSpaces,
                ClusteredIndex = this.IndexType == IndexTypes.Clustered,
                IdentityInsert = this.IdentityInsert,
                BatchSize = this.UseBatches ? this.BatchSize : 1,
                IndexColumn = this.CreateIndex || this.Reseed ? this.IndexColumn - 1 : 0,
                TruncateTable = this.TruncateTable,
                StringFields = this.StringFields,
                CreateIndex = this.CreateIndex,
                DatabaseType = this.DatabaseType,
                UseGoStatements = this.UseGoStatements,
                StringSize = this.StringSize,
            };
        }
    }
}
