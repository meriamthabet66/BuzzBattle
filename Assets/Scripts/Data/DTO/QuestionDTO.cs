namespace Data.DTO {
    public class QuestionDTO 
    {
        public long id { get; set; }
        public string question_text { get; set; }
        public int difficulty { get; set; }
        public string voice_clip_url { get; set; }
    }
}