using Sylvan.Data.Csv;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elars.CsvToSql.Core.Extensions
{
    internal static class CsvDataReaderExtension
    {
        internal static Type GetFieldTypeExtended(this CsvDataReader reader, int index, IList<string> stringFields)
        {
            bool isStringField = stringFields.Any(s => string.Equals(s.Replace(" ", ""), reader.GetName(index).Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
            if (isStringField)
            {
                return typeof(string);
            }

            var value = reader.GetValue(index).ToString();

            if (DateTime.TryParse(value, out _))
                return typeof(DateTime);
            
            var cleaned = value.Replace(",", "$");
            if (int.TryParse(cleaned, out _))
                return typeof(int);

            if (decimal.TryParse(cleaned, out _))
                return typeof(decimal);

            return typeof(string);
        }
    }
}
