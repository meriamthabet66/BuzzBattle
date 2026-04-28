namespace Core.Enums {
    public static class ScoreRules
    {
        // --- Multiple Choice & True/False ---
        public const int McqCorrect = 3;
        public const int McqWrong = -2;

        // --- Verbal (Original Player) ---
        public const int VerbalCorrect = 4;
        public const int VerbalAlmost = 2;
        public const int VerbalWrong = -2;

        // --- Verbal (Steal Mode) ---
        public const int StealCorrect = 6;
        public const int StealAlmost = 0;
        public const int StealWrong = -4;
    }
}