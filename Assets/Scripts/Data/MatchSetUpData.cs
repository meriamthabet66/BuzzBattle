using System.Collections.Generic;
using GamePlay.Questions;

namespace Data {

    namespace Data {
        // A simple static container to hold data while the players navigate the UI
        public static class MatchSetupData {
            public static int PlayerCount = 4;
            public static GameMode Mode = GameMode.Normal;
            public static bool IsTeamMode = false;

            public static int Rounds = 3;
            public static int QuestionsPerRound = 10;

            public static QuestionType QType = QuestionType.MultipleChoice;
            public static List<Category> SelectedCategories = new List<Category>();

            public static void ResetData()
            {
                PlayerCount = 4;
                Mode = GameMode.Normal;
                Rounds = 3;
                QuestionsPerRound = 10;
                QType = QuestionType.MultipleChoice;
                SelectedCategories.Clear();
            }
        }
    }
}