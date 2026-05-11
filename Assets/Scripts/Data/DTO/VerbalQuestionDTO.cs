using Postgrest.Attributes;

namespace Data.DTO {
    [Table("verbal_questions")]
    public class VerbalQuestionDTO : Postgrest.Models.BaseModel {
        [Postgrest.Attributes.PrimaryKey("id")] public long id { get; set; }
        [Postgrest.Attributes.Column("category_id")] public long category_id { get; set; }
        [Postgrest.Attributes.Column("correct_answer_text")] public string correct_answer_text { get; set; }
    }
}