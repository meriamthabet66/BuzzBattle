using System;
using Postgrest.Models;
// Explicitly map the attributes to avoid conflicts with Unity's internal libraries
using TableAttribute = Postgrest.Attributes.TableAttribute;
using ColumnAttribute = Postgrest.Attributes.ColumnAttribute;
using PrimaryKeyAttribute = Postgrest.Attributes.PrimaryKeyAttribute;

namespace Data.DTO 
{
    [Table("profiles")]
    public class ProfileDTO : BaseModel // This MUST inherit BaseModel
    {
        [PrimaryKey("id")] 
        public string id { get; set; }
        
        [Column("email")] 
        public string email { get; set; }
        
        [Column("username")] 
        public string username { get; set; }
        
        [Column("stars")] 
        public int stars { get; set; }
        
        [Column("score")] 
        public int score { get; set; }
        
        [Column("correct_steals")] 
        public int correct_steals { get; set; }
        
        [Column("total_steals")] 
        public int total_steals { get; set; }
        
        [Column("match_wins")] 
        public int match_wins { get; set; }
        
        [Column("matches_played")] 
        public int matches_played { get; set; }
        
        [Column("tournament_wins")] 
        public int tournament_wins { get; set; }
        
        [Column("last_updated")] 
        public DateTime last_updated { get; set; }
    }
}