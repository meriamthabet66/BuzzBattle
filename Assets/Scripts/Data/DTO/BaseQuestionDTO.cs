using Postgrest.Models;
using TableAttribute = Postgrest.Attributes.TableAttribute;
using ColumnAttribute = Postgrest.Attributes.ColumnAttribute;
using PrimaryKeyAttribute = Postgrest.Attributes.PrimaryKeyAttribute;

namespace Data.DTO {
    [Table("base_questions")]
    public class BaseQuestionDTO : BaseModel {
        [PrimaryKey("id")] public long id { get; set; }
        [Column("question_text")] public string question_text { get; set; }
        [Column("audio_clip_url")] public string audio_clip_url { get; set; }
        [Column("difficulty")] public int difficulty { get; set; }
    }
}