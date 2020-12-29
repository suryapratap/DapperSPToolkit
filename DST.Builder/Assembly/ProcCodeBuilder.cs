using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Dapper;

namespace DST.Builder.Assembly
{
    internal class ProcCodeBuilder
    {
        #region Parameter Query

        private const string Query = @"
SELECT 
       SPECIFIC_CATALOG AS [DataBase]
     , SPECIFIC_SCHEMA AS [Schema] 
     , SPECIFIC_NAME AS [Proc]
     , ORDINAL_POSITION AS [ParamterSeq]
     , PARAMETER_MODE AS [Direction]
     , IS_RESULT AS [IsResult]
     , PARAMETER_NAME AS [Name]
     , DATA_TYPE AS [DataType] 
FROM INFORMATION_SCHEMA.PARAMETERS
ORDER BY SPECIFIC_CATALOG, SPECIFIC_SCHEMA, SPECIFIC_NAME, ORDINAL_POSITION
";

        #endregion

        private readonly string _connectionString;
        private readonly string _nameSpace;
        private readonly Dictionary<string, StoredProcedureInfo> _procs = new();
        private readonly string _version;

        public ProcCodeBuilder(string nameSpace, string connectionString, string version = "1.0.0.0")
        {
            _connectionString = connectionString;
            _version = version;
            _nameSpace = nameSpace;

            AddProcParam(GetSpInfos());
        }

        private IEnumerable<StoredProcedureParamterInfo> GetSpInfos()
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var tmpList = conn.Query<StoredProcedureParamterInfo>(Query);
            var result = tmpList.ToList();
            conn.Close();
            return result ?? new List<StoredProcedureParamterInfo>();
        }

        private void AddProcParam(IEnumerable<StoredProcedureParamterInfo> paramList)
        {
            foreach (var spPram in paramList)
                if (!_procs.ContainsKey(spPram.Proc))
                {
                    var inst = new StoredProcedureInfo(spPram);
                    _procs.Add(inst.Name, inst);
                }
                else
                {
                    _procs[spPram.Proc].Add(spPram);
                }
        }

        public string ToCode()
        {
            var code = new StringBuilder();

            var references = new string[] {
               "System",
               "System.Collections.Generic",
               "System.Data",
               "System.Data.SqlClient",
               "System.Runtime.CompilerServices",
               "System.Reflection",
               "System.Runtime.InteropServices",
               "Dapper"
            };
            foreach (var reference in references)
            {
                code.AppendLine($"using {reference};");
            }
            
            try
            {
                // ReSharper disable once FormatStringProblem
                code
                    .AppendLine()
                    .AppendFormat("namespace {0}", _nameSpace)
                    .AppendLine()
                    .AppendLine("{");

                foreach (var proc in _procs) proc.Value.WriteTo(code);

                code.AppendLine("}").AppendLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            return code.ToString();
        }
    }
}