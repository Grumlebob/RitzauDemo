using System.Reflection;
using RitzauDemo.Client.Features.Books;
using RitzauDemo.Core.Features.Books.Contracts;

namespace RitzauDemo.Test.Architecture;

public sealed class DependencyDirectionTests
{
    [Fact]
    public void CompiledProjectReferencesFollowTheFeatureDependencyDirection()
    {
        var coreAssembly = typeof(IBooksApi).Assembly;
        var clientAssembly = typeof(BooksClientService).Assembly;
        var serverAssembly = typeof(Program).Assembly;
        var coreReferences = ReadReferences(coreAssembly);
        var clientReferences = ReadReferences(clientAssembly);

        AssertNoForbiddenReferences(coreAssembly.GetName().Name!, coreReferences, IsFrameworkOrPersistence);
        Assert.Contains(coreAssembly.GetName().Name, clientReferences.Select(reference => reference.Name));
        AssertNoForbiddenReferences(clientAssembly.GetName().Name!, clientReferences, reference =>
            string.Equals(reference.Name, serverAssembly.GetName().Name, StringComparison.Ordinal)
            || IsEntityFrameworkOrDatabase(reference.Name));
    }

    private static AssemblyName[] ReadReferences(Assembly assembly)
    {
        var references = assembly.GetReferencedAssemblies();
        Assert.NotEmpty(references);
        return references;
    }

    private static void AssertNoForbiddenReferences(
        string projectName,
        IEnumerable<AssemblyName> references,
        Func<AssemblyName, bool> isForbidden)
    {
        var offending = references.Where(isForbidden).Select(reference => reference.FullName).Order(StringComparer.Ordinal).ToArray();
        Assert.True(offending.Length == 0,
            $"{projectName} references forbidden assemblies: {string.Join(", ", offending)}");
    }

    private static bool IsFrameworkOrPersistence(AssemblyName reference) =>
        reference.Name?.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal) == true
        || IsEntityFrameworkOrDatabase(reference.Name);

    private static bool IsEntityFrameworkOrDatabase(string? assemblyName) =>
        assemblyName?.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) == true
        || string.Equals(assemblyName, "Npgsql", StringComparison.Ordinal)
        || assemblyName?.StartsWith("Npgsql.", StringComparison.Ordinal) == true;
}
