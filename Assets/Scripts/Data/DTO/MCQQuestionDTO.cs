using Postgrest.Attributes;

namespace Data.DTO {
    [Table("mcq_questions")]
    public class MCQQuestionDTO : Postgrest.Models.BaseModel {
        [Postgrest.Attributes.PrimaryKey("id")] public long id { get; set; }
        [Postgrest.Attributes.Column("category_id")] public long category_id { get; set; }
    }
}