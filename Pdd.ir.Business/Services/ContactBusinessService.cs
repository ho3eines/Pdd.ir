using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Models.Entities;
using Pdd.ir.Data;
using Pdd.ir.Data.Queries;

namespace Pdd.ir.Business.Services
{
    public class ContactBusinessService
    {
        private readonly IDbService _db;

        public ContactBusinessService(IDbService db)
        {
            _db = db;
        }

        public async Task<int> SubmitAsync(ContactRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("نام الزامی است");
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("ایمیل الزامی است");
            if (string.IsNullOrWhiteSpace(request.Message))
                throw new ArgumentException("پیام الزامی است");

            return await _db.ExecuteScalarAsync<int>(ContactQueries.Insert, new
            {
                request.Name,
                request.Email,
                request.Phone,
                request.Subject,
                request.Message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task<IEnumerable<ContactDto>> GetAllAsync()
        {
            var messages = await _db.QueryAsync<ContactMessage>(ContactQueries.GetAll);
            return messages.Select(MapToDto);
        }

        public async Task<IEnumerable<ContactDto>> GetUnreadAsync()
        {
            var messages = await _db.QueryAsync<ContactMessage>(ContactQueries.GetUnread);
            return messages.Select(MapToDto);
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            var affected = await _db.ExecuteAsync(ContactQueries.MarkAsRead, new { Id = id });
            return affected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var affected = await _db.ExecuteAsync(ContactQueries.Delete, new { Id = id });
            return affected > 0;
        }

        public async Task<(int total, int unread)> CountAsync()
        {
            var total = await _db.ExecuteScalarAsync<int>(ContactQueries.CountAll);
            var unread = await _db.ExecuteScalarAsync<int>(ContactQueries.CountUnread);
            return (total, unread);
        }

        private static ContactDto MapToDto(ContactMessage m) => new()
        {
            Id = m.Id,
            Name = m.Name,
            Email = m.Email,
            Phone = m.Phone,
            Subject = m.Subject,
            Message = m.Message,
            IsRead = m.IsRead,
            CreatedAt = m.CreatedAt
        };
    }
}
