using Microsoft.AspNetCore.Mvc;
using Pdd.ir.Data;

namespace Pdd.ir.Server.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly IDbService _db;

        public SearchController(IDbService db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] string entity,
            [FromQuery] string? q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var allowedEntities = new Dictionary<string, string>
            {
                ["role"] = "SELECT Id, Name FROM Roles WHERE IsActive=1",
                ["user"] = "SELECT Id, Username, FullName FROM Users WHERE IsActive=1",
                ["product"] = "SELECT Id, Title FROM Products WHERE IsActive=1",
                ["blog"] = "SELECT Id, Title FROM Blogs WHERE IsActive=1",
                ["client"] = "SELECT Id, Name FROM Clients WHERE IsActive=1",
                ["portfolio"] = "SELECT Id, Title FROM Portfolios WHERE IsActive=1",
                ["contact"] = "SELECT Id, Name FROM Contacts WHERE IsActive=1",
            };

            if (!allowedEntities.ContainsKey(entity.ToLower()))
                return BadRequest(new { message = $"Unknown entity: {entity}" });

            var baseSql = allowedEntities[entity.ToLower()];
            var countSql = baseSql.Replace("SELECT ", "SELECT COUNT(1) ");

            if (!string.IsNullOrWhiteSpace(q))
            {
                var conditions = GetSearchColumns(entity.ToLower());
                var whereClause = string.Join(" OR ", conditions.Select(c => $"{c} LIKE @Q"));
                baseSql += $" AND ({whereClause})";
                countSql += $" AND ({whereClause})";
            }

            baseSql += " ORDER BY Id DESC OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";

            int skip = (page - 1) * pageSize;
            var total = await _db.QueryFirstOrDefaultAsync<int>(countSql, new { Q = $"%{q}%" });
            var items = (await _db.QueryAsync<dynamic>(baseSql, new { Q = $"%{q}%", Skip = skip, Take = pageSize })).ToList();

            return Ok(new
            {
                items,
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)total / pageSize)
            });
        }

        private static string[] GetSearchColumns(string entity) => entity switch
        {
            "role" => new[] { "Name" },
            "user" => new[] { "Username", "FullName" },
            "product" => new[] { "Title" },
            "blog" => new[] { "Title" },
            "client" => new[] { "Name" },
            "portfolio" => new[] { "Title" },
            "contact" => new[] { "Name" },
            _ => new[] { "Name" }
        };
    }
}
