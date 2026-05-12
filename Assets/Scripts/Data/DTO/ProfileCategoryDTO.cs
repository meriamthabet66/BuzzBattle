using Postgrest.Models;
using Postgrest.Attributes;

namespace Data.DTO {
    [Table("profile_categories")]
    public class ProfileCategoryDTO : BaseModel {
        [Column("profile_id")] public string profile_id { get; set; }
        [Column("category_id")] public long category_id { get; set; }
        [Column("is_locked")] public bool is_locked { get; set; }
    }
}