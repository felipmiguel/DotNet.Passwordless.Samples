using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Passwordless.WebAPI.PgSql.EF;
using Passwordless.WebAPI.PgSql.EF.Model;

namespace Passwordless.WebAPI.PgSql.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChecklistController : ControllerBase
    {
        private readonly ILogger<ChecklistController> _logger;
        private readonly ChecklistContext checklistContext;

        public ChecklistController(ILogger<ChecklistController> logger, ChecklistContext checklistContext)
        {
            _logger = logger;
            this.checklistContext = checklistContext;
        }

        [HttpGet]
        public async Task<IEnumerable<Checklist>> Get()
        {
            return await checklistContext.Checklists.ToListAsync();
        }
    }
}