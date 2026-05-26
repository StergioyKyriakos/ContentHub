using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Infrastructure.Authentication;

public sealed class PermissionService : IPermissionService
{
    private readonly ContentHubDbContext _db;

    public PermissionService(ContentHubDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default)
    {
        return await _db.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == userId)
            .SelectMany(userRole => userRole.Role.Permissions)
            .AnyAsync(rolePermission => rolePermission.Name == permission, cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _db.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == userId)
            .Select(userRole => userRole.Role.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _db.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == userId)
            .SelectMany(userRole => userRole.Role.Permissions)
            .Select(permission => permission.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}