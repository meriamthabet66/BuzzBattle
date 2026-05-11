using UnityEngine;
using GamePlay.Questions;
using Data.DTO;
using System.Collections.Generic;

namespace GamePlay.Systems {
    public static class QuestionFactory
    {
        public static MultipleChoiceQuestion CreateMCQ(BaseQuestionDTO baseDto, List<MCQOptionDTO> optionDtos)
        {
            var so = ScriptableObject.CreateInstance<MultipleChoiceQuestion>();
            so.id = baseDto.id;
            so.questionText = baseDto.question_text;
            so.difficulty = baseDto.difficulty;
        
            so.answers = new List<AnswerOption>();
            foreach (var opt in optionDtos)
            {
                so.answers.Add(new AnswerOption {
                    text = opt.answer_text,
                    isCorrect = opt.is_correct
                });
            }
            return so;
        }

        public static TrueOrFalseQuestion CreateTF(BaseQuestionDTO baseDto, TFQuestionDTO tfDto)
        {
            var so = ScriptableObject.CreateInstance<TrueOrFalseQuestion>();
            so.id = baseDto.id;
            so.questionText = baseDto.question_text;
            so.difficulty = baseDto.difficulty;
            so.isTrue = tfDto.is_true;
            return so;
        }

        public static VerbalQuestion CreateVerbal(BaseQuestionDTO baseDto, VerbalQuestionDTO vDto)
        {
            var so = ScriptableObject.CreateInstance<VerbalQuestion>();
            so.id = baseDto.id;
            so.questionText = baseDto.question_text;
            so.difficulty = baseDto.difficulty;
            so.correctAnswer = vDto.correct_answer_text;
            return so;
        }
    }
}