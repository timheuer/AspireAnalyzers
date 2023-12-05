using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = AspireAnalyzers.Test.CSharpAnalyzerVerifier<
       AspireAnalyzers.PostgresDatabaseNamingAnalyzer>;

namespace AspireAnalyzers.Test;

[TestClass]
public class AspireAnalyzersUnitTest
{
    [TestMethod]
    public async Task Container_And_Resource_Named_Same()
    {
        var testCode = @"
public class C {
    internal void M()
    {
        var builder = Aspire.Hosting.DistributedApplication.CreateBuilder(null);
        var db = builder.AddPostgresContainer(""db"").AddDatabase([|""db""|]);
    }
}";

        var expectedDiagnostic = VerifyCS.Diagnostic(PostgresDatabaseNamingAnalyzer.DiagnosticId);
        await VerifyCS.VerifyAnalyzerAsync(testCode, expectedDiagnostic);
    }

    [TestMethod]
    public async Task AzureStorage_Same_Name()
    {
        var testCode = @"
public class C {
    internal void M()
    {
        var builder = Aspire.Hosting.DistributedApplication.CreateBuilder(null);
        var photos = builder.AddAzureStorage(""psstorage"").AddBlobs([|""psstorage""|]);
    }
}";

        var expectedDiagnostic = VerifyCS.Diagnostic(AzureStorageNamingAnalyzer.DiagnosticId);
        await VerifyCS.VerifyAnalyzerAsync(testCode, expectedDiagnostic);
    }
}
