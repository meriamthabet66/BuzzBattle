using Postgrest.Attributes;

namespace Data.DTO {
    [Table("mcq_options")]
    public class MCQOptionDTO : Postgrest.Models.BaseModel {
        [Postgrest.Attributes.PrimaryKey("id")] public long id { get; set; }
        [Postgrest.Attributes.Column("mcq_id")] public long mcq_id { get; set; }
        [Postgrest.Attributes.Column("answer_text")] public string answer_text { get; set; }
        [Postgrest.Attributes.Column("is_correct")] public bool is_correct { get; set; }
    }
}