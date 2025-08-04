using System.Reflection;
using ClickHouse.Facades.Migrations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ClickHouse.Migrate;

public class RoslynMigrationsLocator : ClickHouseAssemblyMigrationsLocator
{
	protected override Assembly TargetAssembly => _assembly;

	private readonly Assembly _assembly;

	public RoslynMigrationsLocator(MigrationOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);

		var migrationsDirectory = options.MigrationsDirectory
			?? throw new InvalidOperationException("Invalid options.");

		_assembly = CompileSources(migrationsDirectory);
	}

	private static Assembly CompileSources(string migrationsDirectory)
	{
		const string assemblyName = "RoslynMigrations";

		var sourceFiles = Directory.GetFiles(
			migrationsDirectory,
			"*.cs",
			SearchOption.TopDirectoryOnly);

		var syntaxTrees = sourceFiles.Select(file =>
			CSharpSyntaxTree.ParseText(WrapSource(File.ReadAllText(file))));

		var references = AppDomain.CurrentDomain
			.GetAssemblies()
			.Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
			.Select(a => MetadataReference.CreateFromFile(a.Location));

		var compilation = CSharpCompilation.Create(
			assemblyName,
			syntaxTrees,
			references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		using var ms = new MemoryStream();
		var result = compilation.Emit(ms);

		if (!result.Success)
		{
			var failures = string.Join("\n", result.Diagnostics
				.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

			throw new InvalidOperationException($"Compilation failed:\n{failures}");
		}

		ms.Seek(0, SeekOrigin.Begin);

		return Assembly.Load(ms.ToArray());
	}

	private static string WrapSource(string source) => """
	using ClickHouse.Facades.Migrations;

	""" + source;
}
