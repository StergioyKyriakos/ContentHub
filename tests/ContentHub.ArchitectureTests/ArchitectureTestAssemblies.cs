using System.Reflection;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Persistence;
using ContentHub.Infrastructure;

namespace ContentHub.ArchitectureTests;

public static class ArchitectureTestAssemblies
{
    public static readonly Assembly Api = typeof(Program).Assembly;

    public static readonly Assembly Application = typeof(Roles).Assembly;

    public static readonly Assembly Data = typeof(ContentHubDbContext).Assembly;

    public static readonly Assembly Infrastructure = typeof(DependencyInjection).Assembly;
}