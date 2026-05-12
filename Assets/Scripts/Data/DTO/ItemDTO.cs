using Postgrest.Models;
using TableAttribute = Postgrest.Attributes.TableAttribute;
using ColumnAttribute = Postgrest.Attributes.ColumnAttribute;
using PrimaryKeyAttribute = Postgrest.Attributes.PrimaryKeyAttribute;

namespace Data.DTO {
    [Table("items")]
    public class ItemDTO : BaseModel {
        [PrimaryKey("id")] public long id { get; set; }
        [Column("item_name")] public string item_name { get; set; }
        [Column("item_type")] public string item_type { get; set; }
        [Column("price")] public int price { get; set; }
    }
}