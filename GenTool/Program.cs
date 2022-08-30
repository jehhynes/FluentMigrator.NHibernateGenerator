using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace FluentMigrator.NHibernateGenerator
{
    internal class Program
    {
        static Regex classRegex = new Regex(@"public partial class (\w+)", RegexOptions.Compiled);
        private static int Main(string[] args)
        {
            string operation = args[0]; //"add" or "update"
            string migrationName = args[1];

            Dictionary<string, string> arguments = new Dictionary<string, string>();
            for (int i = 2; i + 1 < args.Length; i += 2)
            {
                arguments.Add(args[i].TrimStart('-'), args[i + 1]);
            }

            string assemblyPath = arguments["assembly"];
            string startupAssemblyPath = arguments["startup-assembly"];
            string projectDir = arguments["project-dir"];
            string language = arguments["language"];
            string workingDir = arguments["working-dir"];
            string rootNamespace = arguments["root-namespace"];

            string startupAssemblyDir = Path.GetDirectoryName(startupAssemblyPath);
            var assemblyResolver = new AssemblyResolver(startupAssemblyDir);

            //foreach (var kv in arguments)
            //    Console.WriteLine($"Argument '{kv.Key}': {kv.Value}");

            GeneratedMigration m;
            try
            {
                m = (GeneratedMigration)NugetTooling.Generate(assemblyPath, migrationName);
            }
            catch (ReflectionTypeLoadException rtle)
            {
                foreach (var ex in rtle.LoaderExceptions)
                {
                    Console.WriteLine(ex.Message);
                }
                return 1;
            }

            if (m.ErrorMessage != null)
            {
                Console.WriteLine("Error generating migration: " + m.ErrorMessage);
                return 1;
            }
            else
            {
                string migrationFilePath;
                string designerFilePath;

                if (operation == "add")
                {
                    migrationFilePath = Path.Combine(projectDir, m.MigrationsDirectory, m.FileNamePrefix + migrationName + ".cs");
                    designerFilePath = Path.Combine(projectDir, m.MigrationsDirectory, m.FileNamePrefix + migrationName + ".Designer.cs");

                    File.WriteAllText(migrationFilePath, m.Code);
                    File.WriteAllText(designerFilePath, m.Designer);
                }
                else if (operation == "update")
                {
                    var designerFiles = Directory.GetFiles(Path.Combine(projectDir, m.MigrationsDirectory)).Where(x => x.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase));
                    string selectedDesignerFile;
                    if (migrationName == "LATEST")
                    {
                        selectedDesignerFile = designerFiles.OrderBy(x => x).LastOrDefault();
                    }
                    else
                    {
                        selectedDesignerFile = designerFiles.Where(x => x.EndsWith(migrationName + ".designer.cs", StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
                    }

                    if (selectedDesignerFile == null)
                    {
                        Console.WriteLine("Could not find designer file.");
                        return 1;
                    }

                    designerFilePath = selectedDesignerFile;
                    migrationFilePath = designerFilePath.ToLower().Replace(".designer.cs", ".cs");

                    string designerCode = m.Designer;
                    if (migrationName == "LATEST" && File.Exists(migrationFilePath))
                    {
                        var code = File.ReadAllText(migrationFilePath);

                        //Derive the migration class name from the existing code file
                        var match = classRegex.Match(code);
                        if (match.Success)
                        {
                            designerCode = designerCode.Replace("public partial class LATEST", "public partial class " + match.Groups[1].Value);
                        }
                    }
                    File.WriteAllText(designerFilePath, designerCode);
                }
                else
                {
                    Console.WriteLine("Unknown operation");
                    return 1;
                }

                Console.WriteLine("data:    {");
                Console.WriteLine("data:    \"migrationFile\": " + Json.Literal(migrationFilePath) + ",");
                Console.WriteLine("data:    \"metadataFile\": " + Json.Literal(designerFilePath));
                Console.WriteLine("data:    }");

                return 0;
            }
        }
    }

    internal static class Json
    {
        //public static CommandOption ConfigureOption(CommandLineApplication command)
        //    => command.Option("--json", Resources.JsonDescription);

        public static string Literal(string text)
            => text != null
                ? "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\""
                : "null";

        public static string Literal(bool? value)
            => value.HasValue
                ? value.Value
                    ? "true"
                    : "false"
                : "null";
    }

    public class AssemblyResolver
    {
        private readonly string folder;

        public AssemblyResolver(string folder)
        {
            this.folder = folder;

            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }

        private Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);

            foreach (var extension in new[] { ".dll", ".exe" })
            {
                var path = Path.Combine(folder, assemblyName.Name + extension);
                if (File.Exists(path))
                {
                    try
                    {
                        return Assembly.LoadFrom(path);
                    }
                    catch
                    {
                    }
                }
            }

            return null;
        }
    }
}
