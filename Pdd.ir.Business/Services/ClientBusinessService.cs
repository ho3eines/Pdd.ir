using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Models.Entities;
using Pdd.ir.Data;
using Pdd.ir.Data.Queries;

namespace Pdd.ir.Business.Services
{
    public class ClientBusinessService
    {
        private readonly IDbService _db;

        public ClientBusinessService(IDbService db)
        {
            _db = db;
        }

        public async Task<IEnumerable<ClientDto>> GetAllAsync()
        {
            var clients = await _db.QueryAsync<Client>(ClientQueries.GetAll);
            return clients.Select(MapToDto);
        }

        public async Task<IEnumerable<ClientDto>> GetAllAdminAsync()
        {
            var clients = await _db.QueryAsync<Client>(ClientQueries.GetAllAdmin);
            return clients.Select(MapToDto);
        }

        public async Task<ClientDto?> GetByIdAsync(int id)
        {
            var client = await _db.QueryFirstOrDefaultAsync<Client>(ClientQueries.GetById, new { Id = id });
            return client != null ? MapToDto(client) : null;
        }

        public async Task<int> InsertAsync(ClientCreateRequest request)
        {
            return await _db.ExecuteScalarAsync<int>(ClientQueries.Insert, new
            {
                request.Name,
                request.NameEn,
                request.Icon,
                request.Color,
                request.SortOrder,
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });
        }

        public async Task<bool> UpdateAsync(ClientDto dto)
        {
            var rows = await _db.ExecuteAsync(ClientQueries.Update, dto);
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var rows = await _db.ExecuteAsync(ClientQueries.SoftDelete, new { Id = id });
            return rows > 0;
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            var client = await _db.QueryFirstOrDefaultAsync<Client>(ClientQueries.GetById, new { Id = id });
            if (client == null) return false;
            
            var newStatus = !client.IsActive;
            var rows = await _db.ExecuteAsync(ClientQueries.SetActive, new { Id = id, IsActive = newStatus });
            return rows > 0;
        }

        private static ClientDto MapToDto(Client c)
        {
            return new ClientDto
            {
                Id = c.Id,
                Name = c.Name,
                NameEn = c.NameEn,
                Icon = c.Icon,
                Color = c.Color,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive
            };
        }
    }
}
