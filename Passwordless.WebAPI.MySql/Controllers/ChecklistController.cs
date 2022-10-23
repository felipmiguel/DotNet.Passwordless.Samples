using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.Repository;
using Sample.Repository.Model;

namespace Passwordless.WebAPI.MySql.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChecklistController : ControllerBase
    {
        private readonly ILogger<ChecklistController> _logger;
        private readonly IDbContextFactory<ChecklistContext> contextFactory;

        public ChecklistController(ILogger<ChecklistController> logger, IDbContextFactory<ChecklistContext> contextFactory)
        {
            _logger = logger;
            this.contextFactory = contextFactory;
        }

        [HttpGet]
        public async Task<IEnumerable<Checklist>> Get()
        {
            using (var context = contextFactory.CreateDbContext())
            {
                if (context.Checklists != null)
                {
                    return await context.Checklists.ToListAsync();
                }
                else
                {
                    _logger.LogError("Checklists is null");
                    throw new Exception("database not properly initialized");
                }
            }
            
        }
    }
}