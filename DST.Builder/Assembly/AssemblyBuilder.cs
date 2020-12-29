using System.IO;
using System.Text;
using Dapper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

// ReSharper disable HeapView.BoxingAllocation
// ReSharper disable HeapView.ObjectAllocation.Evident
// ReSharper disable HeapView.ObjectAllocation

namespace DST.Builder.Assembly
{
    public static class AssemblyBuilder
    {
        public static void GenerateAssembly(string code, string fileName)
        {
            var currentFolder = Directory.GetCurrentDirectory();

            var dapperExtensionPath = Path.Combine(currentFolder, "Dapper.Sp.Helper.cs");
            code = code + "\n" + File.ReadAllText(dapperExtensionPath);

            var tree = SyntaxFactory.ParseSyntaxTree(code);
            var outputPath = Path.Combine(currentFolder,"Out");
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            var dllFilePath = Path.Combine(outputPath, fileName);
            
            var frameworkRoot = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1";
            // Create a reference to the library
            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile($@"{frameworkRoot}\mscorlib.dll"),
                MetadataReference.CreateFromFile($@"{frameworkRoot}\System.dll"),
                MetadataReference.CreateFromFile($@"{frameworkRoot}\System.Data.dll"),
                MetadataReference.CreateFromFile(typeof(DynamicParameters).Assembly.Location), 
                
            };
            var cSharpCompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithPlatform(Platform.AnyCpu)
                .WithOutputKind(OutputKind.DynamicallyLinkedLibrary)
                .WithModuleName(fileName.Replace(".dll", ""));


            // A single, immutable invocation to the compiler
            // to produce a library
            var compilation = CSharpCompilation.Create(fileName)
                .WithOptions(cSharpCompilationOptions)
                .WithAssemblyName(fileName.Replace(".dll", ""))
                .AddReferences(references)
                .AddSyntaxTrees(tree);


            var compilationResult = compilation.Emit(dllFilePath);
            
            if (!compilationResult.Success)
            {
                var errorMessages = new StringBuilder();
                foreach (var codeIssue in compilationResult.Diagnostics)
                    errorMessages
                        .AppendLine($"ID: {codeIssue.Id}")
                        .AppendLine($"Message: {codeIssue.GetMessage()},")
                        .AppendLine($"Location: {codeIssue.Location.GetLineSpan()},")
                        .AppendLine($"Severity: {codeIssue.Severity}")
                        .AppendLine();
                File.WriteAllText(dllFilePath.Replace(".dll", ".cs"), code);
                File.WriteAllText(dllFilePath.Replace(".dll", ".txt"), errorMessages.ToString());
            }
        }
    }
}