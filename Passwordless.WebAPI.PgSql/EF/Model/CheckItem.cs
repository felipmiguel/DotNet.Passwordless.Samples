using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Passwordless.WebAPI.PgSql.EF.Model
{
    public class CheckItem
    {
        public int ID { get; set; }
        public string? Description { get; set; }

        public int ChecklistID { get; set; }
        public virtual Checklist? Checklist { get; set; }
    }
}
