using Postgrest.Models;
using TableAttribute = Postgrest.Attributes.TableAttribute;
using ColumnAttribute = Postgrest.Attributes.ColumnAttribute;
using PrimaryKeyAttribute = Postgrest.Attributes.PrimaryKeyAttribute;

namespace Data.DTO {
    [Table("categories")]
    public class CategoryDTO : BaseModel {
        [PrimaryKey("id")] public long id { get; set; }
        [Column("category_name")] public string category_name { get; set; }
        [Column("category_icon_url")] public string category_icon_url { get; set; }
        [Column("price")] public int price { get; set; }
        [Column("version")] public int version { get; set; }

    }
}