namespace GamePlay.Questions
{
    public static class QuestionRules
    {
        // Change from 'const' to 'public static int' so they can be modified
        public static int MultipleChoiceTime = 5;
        public static int VerbalTime = 10;
        public static int BuzzTimeLimit = 5;
        public static int VerbalStealTime = 5; 

        // Helper to reset to factory settings if needed
        public static void ResetToDefaults()
        {
            MultipleChoiceTime = 5;
            VerbalTime = 10;
            BuzzTimeLimit = 5;
            VerbalStealTime = 5;
        }
    }
}