using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dapper.Sp.Helper;

namespace DST.Builder.Assembly
{
    internal class StoredProcedureInfo
    {
        public readonly string Name;

        public StoredProcedureInfo(StoredProcedureParamterInfo param)
        {
            Name = param.Proc;
            Parameters = new SortedDictionary<int, StoredProcedureParamterInfo>();
            Add(param);
        }

        public SortedDictionary<int, StoredProcedureParamterInfo> Parameters { get; }

        public void Add(StoredProcedureParamterInfo param)
        {
            Parameters[param.ParamterSeq] = param;
        }

        [SuppressMessage("ReSharper", "CA1834")]
        [SuppressMessage("ReSharper", "FormatStringProblem")]
        public void WriteTo(StringBuilder code)
        {
            code.AppendLine($"[Dapper.Sp.Helper.{nameof(StoredProcedureAttribute).Replace("Attribute", "")}(\"{Name}\")]")
                .Append($"public class {Name} : Dapper.Sp.Helper.{nameof(IStoredProcedureInput)}")
                .AppendLine()
                .AppendLine("{")
                .AppendLine();

            foreach (var parameter in Parameters)
            {
                code.AppendLine();
                parameter.Value.WriteTo(code);
            }

            code.AppendLine().Append("}").AppendLine();
        }
    }
}