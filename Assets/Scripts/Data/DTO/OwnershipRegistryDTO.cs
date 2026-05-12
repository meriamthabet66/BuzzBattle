using Postgrest.Models;
using TableAttribute = Postgrest.Attributes.TableAttribute;
using ColumnAttribute = Postgrest.Attributes.ColumnAttribute;
using PrimaryKeyAttribute = Postgrest.Attributes.PrimaryKeyAttribute;

namespace Data.DTO {
    [Table("ownership_registry")]
    public class OwnershipRegistryDTO : BaseModel {
        [PrimaryKey("id")] public long id { get; set; }
        [Column("profile_id")] public string profile_id { get; set; }
        [Column("item_id")] public long item_id { get; set; }
        [Column("character_id")] public long? character_id { get; set; } // Nullable if not equipped
        [Column("is_locked")] public bool is_locked { get; set; }
    }
}