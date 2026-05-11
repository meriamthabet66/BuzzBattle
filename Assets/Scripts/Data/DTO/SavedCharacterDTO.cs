using Postgrest.Models;
using TableAttribute = Postgrest.Attributes.TableAttribute;
using ColumnAttribute = Postgrest.Attributes.ColumnAttribute;
using PrimaryKeyAttribute = Postgrest.Attributes.PrimaryKeyAttribute;

namespace Data.DTO {
    [Table("saved_characters")]
    public class SavedCharacterDTO : BaseModel {
        [PrimaryKey("id")] public long id { get; set; }
        [Column("profile_id")] public string profile_id { get; set; }
        [Column("nickname")] public string nickname { get; set; }
        [Column("skin_url")] public string skin_url { get; set; }
    }
}