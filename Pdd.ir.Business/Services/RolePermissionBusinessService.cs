using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Data;
using Pdd.ir.Data.Queries;

namespace Pdd.ir.Business.Services
{
    public class RoleBusinessService
    {
        private readonly IDbService _db;

        public RoleBusinessService(IDbService db) => _db = db;

        public async Task<IEnumerable<RoleDto>> GetAllAsync()
        {
            return await _db.QueryAsync<RoleDto>(RoleQueries.GetAll);
        }

        public async Task<RoleDetailDto?> GetByIdAsync(int id)
        {
            var role = await _db.QueryFirstOrDefaultAsync<RoleDto>(RoleQueries.GetById, new { Id = id });
            if (role == null) return null;

            var permIds = await _db.QueryAsync<int>(RolePermissionQueries.GetByRoleId, new { RoleId = id });

            return new RoleDetailDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                PermissionIds = permIds.ToList()
            };
        }

        public async Task<int> CreateAsync(RoleCreateRequest request)
        {
            var id = await _db.ExecuteScalarAsync<int>(RoleQueries.Insert, new
            {
                request.Name,
                request.Description
            });

            foreach (var permId in request.PermissionIds)
            {
                await _db.ExecuteAsync(RolePermissionQueries.Insert, new { RoleId = id, PermissionId = permId });
            }

            return id;
        }

        public async Task<bool> UpdateAsync(int id, RoleUpdateRequest request)
        {
            var rows = await _db.ExecuteAsync(RoleQueries.Update, new { Id = id, request.Name, request.Description });
            if (rows == 0) return false;

            await _db.ExecuteAsync(RolePermissionQueries.DeleteByRoleId, new { RoleId = id });

            foreach (var permId in request.PermissionIds)
            {
                await _db.ExecuteAsync(RolePermissionQueries.Insert, new { RoleId = id, PermissionId = permId });
            }

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var rows = await _db.ExecuteAsync(RoleQueries.Delete, new { Id = id });
            return rows > 0;
        }

        public async Task<int> CountUsersAsync(string roleName)
        {
            return await _db.ExecuteScalarAsync<int>(RoleQueries.CountUsers, new { RoleName = roleName });
        }
    }

    public class PermissionBusinessService
    {
        private readonly IDbService _db;

        public PermissionBusinessService(IDbService db) => _db = db;

        public async Task<IEnumerable<PermissionDto>> GetAllAsync()
        {
            return await _db.QueryAsync<PermissionDto>(PermissionQueries.GetAll);
        }

        public async Task<IEnumerable<PermissionDto>> GetByRoleIdAsync(int roleId)
        {
            return await _db.QueryAsync<PermissionDto>(PermissionQueries.GetByRoleId, new { RoleId = roleId });
        }

        public async Task<int> CreateAsync(PermissionDto request)
        {
            return await _db.ExecuteScalarAsync<int>(PermissionQueries.Insert, new
            {
                request.Name,
                request.Label,
                request.Category
            });
        }

        public async Task<bool> UpdateAsync(int id, PermissionDto request)
        {
            var rows = await _db.ExecuteAsync(PermissionQueries.Update, new
            {
                Id = id,
                request.Name,
                request.Label,
                request.Category
            });
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var rows = await _db.ExecuteAsync(PermissionQueries.Delete, new { Id = id });
            return rows > 0;
        }
    }
}
