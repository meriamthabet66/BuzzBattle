using System.Collections.Generic;
using UnityEngine;
using GamePlay.Questions;
using GamePlay.Systems;
using GamePlay.Configs;


//this is just a test class to test questionLoader 
public class TestQuestionSystem : MonoBehaviour
{
    [SerializeField] private QuestionLoader questionLoader;
    [SerializeField] private Category testCategory;
    private int questionNumber = 0;

    private void Start()
    {
        Debug.Log("TEST STARTED");

        // Create a fake round configuration
        RoundConfig round = new RoundConfig
        {
            categories = new List<Category> { testCategory },
            questionType = QuestionType.MultipleChoice, // change this to test other types
            questionCount = 5,
            questionTimerSeconds = 20
        };

        // Subscribe to question event
        QuestionLoader.OnQuestionLoaded += OnQuestionLoaded;

        // Initialize loader
        questionLoader.Initialize(round);

        // Start round
        questionLoader.LoadNextQuestion();
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
        
        
        // Load next question automatically
        Invoke(nameof(LoadNext), 1f);
        
        
    }

    void LoadNext()
    {
        if (questionLoader.HasMoreQuestions())
        {
            questionLoader.LoadNextQuestion();
        }
        else
        {
            Debug.Log("ROUND FINISHED");
            OnRoundFinished();
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
    
    void OnRoundFinished()
    {
        Debug.Log("Round Complete");
    }
}