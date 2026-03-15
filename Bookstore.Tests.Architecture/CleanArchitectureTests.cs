using Bookstore.API.Controllers;
using Bookstore.Application.Interfaces;
using Bookstore.Domain.Entities;
using Bookstore.Infrastructure.Database;
using NetArchTest.Rules;
using System.Reflection;

namespace Bookstore.Tests.Architecture;

public class CleanArchitectureTests
{
    private const string DomainNamespace = "Bookstore.Domain";
    private const string ApplicationNamespace = "Bookstore.Application";
    private const string InfrastructureNamespace = "Bookstore.Infrastructure";
    private const string ApiNamespace = "Bookstore.API";

    private static readonly Assembly DomainAssembly = typeof(Book).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(IBookService).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(AppDbContext).Assembly;
    private static readonly Assembly ApiAssembly = typeof(BooksController).Assembly;

    [Fact]
    public void Domain_Should_Not_Depend_On_Application_Infrastructure_Or_Api()
    {
        var result = Types
            .InAssembly(DomainAssembly)
            .That()
            .ResideInNamespace(DomainNamespace)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailure(result));
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_Api()
    {
        var result = Types
            .InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespace(ApplicationNamespace)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailure(result));
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Api()
    {
        var result = Types
            .InAssembly(InfrastructureAssembly)
            .That()
            .ResideInNamespace(InfrastructureNamespace)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailure(result));
    }

    [Fact]
    public void Api_Controllers_And_Middleware_Should_Not_Depend_On_Infrastructure()
    {
        // The API project references Infrastructure only for DI wiring in Program.cs or Extensions/.
        // Controllers, middleware, jobs, Swagger config etc.must not leak that dependency.
        var result = Types
            .InAssembly(ApiAssembly)
            .That()
            .ResideInNamespaceStartingWith(ApiNamespace)
            .And()
            .DoNotResideInNamespace($"{ApiNamespace}.Extensions")
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailure(result));
    }

    private static string FormatFailure(TestResult result)
    {
        if (result.FailingTypes is null || !result.FailingTypes.Any())
        {
            return "Architecture rule failed.";
        }

        return "Architecture rule failed. Offending types: " +
            string.Join(", ", result.FailingTypeNames ?? []);
    }
}
