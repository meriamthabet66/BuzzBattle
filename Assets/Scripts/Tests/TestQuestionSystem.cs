using System.Collections.Generic;
using UnityEngine;
using GamePlay.Questions;
using GamePlay.Systems;
using Managers;

public class TestQuestionSystem : MonoBehaviour
{
    [SerializeField] private Category testCategory;

    private int questionNumber = 0;

    private void Start()
    {
        Debug.Log("TEST STARTED");

        // Subscribe to question events
        QuestionLoader.OnQuestionLoaded += OnQuestionLoaded;

        // Start a fake match
        MatchManager.Instance.StartMatch(GameMode.Normal, 1, 5);

        // Start the round through MatchManager
        MatchManager.Instance.StartRound(
            new List<Category> { testCategory },
            QuestionType.MultipleChoice
        );
    }

    private void OnQuestionLoaded(BaseQuestion question)
    {
        questionNumber++;

        Debug.Log($"Question #{questionNumber}: {question.questionText}");

        switch (question.GetQuestionType())
        {
            case QuestionType.MultipleChoice:
                if (question is MultipleChoiceQuestion mcq)
                    TestMultipleChoice(mcq);
                break;

            case QuestionType.TrueOrFalse:
                if (question is TrueOrFalseQuestion tf)
                    TestTrueOrFalse(tf);
                break;

            case QuestionType.Verbal:
                if (question is VerbalQuestion vq)
                    TestVerbal(vq);
                break;
        }
        
    }

  
    void TestMultipleChoice(MultipleChoiceQuestion mcq)
    {
        Debug.Log("TYPE: Multiple Choice");

        var answers = mcq.GetShuffledAnswers();

        foreach (var answer in answers)
        {
            Debug.Log($"Answer: {answer.text} | Correct: {answer.isCorrect}");
        }
    }

    void TestTrueOrFalse(TrueOrFalseQuestion tf)
    {
        Debug.Log("TYPE: True or False");

        Debug.Log("Correct Answer: " + (tf.isTrue ? "True" : "False"));
    }

    void TestVerbal(VerbalQuestion verbal)
    {
        Debug.Log("TYPE: Verbal");

        Debug.Log("Correct Answer: " + verbal.correctAnswer);
    }

    private void OnDestroy()
    {
        QuestionLoader.OnQuestionLoaded -= OnQuestionLoaded;
    }
}