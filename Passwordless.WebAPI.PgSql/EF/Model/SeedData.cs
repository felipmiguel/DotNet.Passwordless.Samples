﻿using Microsoft.EntityFrameworkCore;

namespace Passwordless.WebAPI.PgSql.EF.Model
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            DbContextOptions<ChecklistContext> options = serviceProvider.GetRequiredService<DbContextOptions<ChecklistContext>>();
            using (var context = new ChecklistContext(options, configuration))
            {
                if (context == null || context.Checklists == null)
                {
                    throw new ArgumentNullException("Null Checklists");
                }

                // Look for any checklist.
                if (context.Checklists.Any())
                {
                    return;   // DB has been seeded
                }

                context.Checklists.AddRange(
                    new Checklist
                    {
                        Name = "Checklist 1",
                        Date = DateTime.Now,
                        Description = "Checklist 1 Description",
                        CheckItems = new List<CheckItem>
                        {
                            new CheckItem { Description = "CheckItem 1"},
                            new CheckItem { Description = "CheckItem 3"},
                            new CheckItem { Description = "CheckItem 4"},
                            new CheckItem { Description = "CheckItem 5"},
                        }
                    },
                    new Checklist
                    {
                        Name = "Checklist 2",
                        Date = DateTime.Now,
                        Description = "Checklist 2 Description",
                        CheckItems = new List<CheckItem>
                        {
                            new CheckItem { Description = "CheckItem 1"},
                            new CheckItem { Description = "CheckItem 3"},
                            new CheckItem { Description = "CheckItem 4"},
                            new CheckItem { Description = "CheckItem 5"},
                        }
                    }
                );
                context.SaveChanges();
            }
        }
    }
}

