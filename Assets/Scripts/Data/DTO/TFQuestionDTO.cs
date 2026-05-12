using Postgrest.Attributes;

namespace Data.DTO {
    [Table("tf_questions")]
    public class TFQuestionDTO : Postgrest.Models.BaseModel {
        [Postgrest.Attributes.PrimaryKey("id")] public long id { get; set; }
        [Postgrest.Attributes.Column("category_id")] public long category_id { get; set; }
        [Postgrest.Attributes.Column("is_true")] public bool is_true { get; set; }
    }
}