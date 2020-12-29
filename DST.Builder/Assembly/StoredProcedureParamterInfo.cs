using System;
using System.Collections.Generic;
using System.Text;
using Dapper.Sp.Helper;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DST.Builder.Assembly
{
    internal class StoredProcedureParamterInfo
    {
        private static readonly Dictionary<string, string> Map =
            new(StringComparer.InvariantCultureIgnoreCase)
            {
                {"bigint", "Int64"}, {"binary", "Byte[]"}, {"bit", "Boolean"}, {"char", "String"}, {"date", "DateTime"},
                {"datetime", "DateTime"}, {"datetime2", "DateTime"}, {"datetimeoffset", "DateTimeOffset"},
                {"decimal", "Decimal"}, {"float", "Double"}, {"image", "Byte[]"}, {"int", "Int32"},
                {"money", "Decimal"}, {"nchar", "String"}, {"ntext", "String"}, {"numeric", "Decimal"},
                {"nvarchar", "String"}, {"real", "Single"}, {"rowversion", "Byte[]"}, {"smalldatetime", "DateTime"},
                {"smallint", "Int16"}, {"smallmoney", "Decimal"}, {"sql_variant", "Object"}, {"text", "String"},
                {"time", "TimeSpan"}, {"timestamp", "Byte[]"}, {"tinyint", "Byte"}, {"uniqueidentifier", "Guid"},
                {"varbinary", "Byte[]"}, {"varchar", "String"}, {"xml", "String"}
            };

        public string Database { get; set; }
        public string Schema { get; set; }
        public string Proc { get; set; }
        public int ParamterSeq { get; set; }
        public string Direction { get; set; }
        public string IsResult { get; set; }
        public string Name { get; set; }
        public string DataType { get; set; }

        public void WriteTo(StringBuilder code)
        {
            if (string.IsNullOrEmpty(Name)) return;

            if (Direction.ToUpperInvariant().Contains("OUT"))
                code.AppendLine($"[{nameof(OutParameterAttribute).Replace("Attribute", "")}]");

            code.AppendFormat("public {0} {1} ", Map[DataType], Name.Replace("@", ""))
                .AppendLine("{get;set;}");
        }
    }
}