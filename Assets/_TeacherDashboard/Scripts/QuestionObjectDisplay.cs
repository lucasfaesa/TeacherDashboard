using System;
using System.Collections.Generic;
using System.Linq;
using API_Mestrado_Lucas;
using UnityEngine;
using UnityEngine.UI;

namespace _TeacherDashboard.Scripts
{
    public class QuestionObjectDisplay : MonoBehaviour
    {
        [Header("SO")] [SerializeField] private TeacherDataSO teacherData;
        [Space] 
        [SerializeField] private InputFieldStandaloneInfo questionField;
        [SerializeField] private InputFieldStandaloneInfo aAnswer;
        [SerializeField] private InputFieldStandaloneInfo bAnswer;
        [SerializeField] private InputFieldStandaloneInfo cAnswer;
        [SerializeField] private InputFieldStandaloneInfo dAnswer;
        [SerializeField] private InputFieldStandaloneInfo timeLimit;
        [SerializeField] private InputFieldStandaloneInfo scoreValue;
        [Header("Toggles")] 
        [SerializeField] private Toggle aToggle;
        [SerializeField] private Toggle bToggle;
        [SerializeField] private Toggle cToggle;
        [SerializeField] private Toggle dToggle;

        public EditQuestionsWindowController editQuestionsWindowController;

        private List<InputFieldStandaloneInfo> _inputFieldInfosList = new();

        private QuestionDTO questionDto;

        private void Reset()
        {
            if (_inputFieldInfosList.Count == 0)
            {
                GetInputFieldsStandaloneInfos();
            }

            aToggle.isOn = true;
            
            questionDto = null;
            questionField.InputFieldInfos.inputField.text = "";
            aAnswer.InputFieldInfos.inputField.text = "";
            bAnswer.InputFieldInfos.inputField.text = "";
            cAnswer.InputFieldInfos.inputField.text = "";
            dAnswer.InputFieldInfos.inputField.text = "";
            timeLimit.InputFieldInfos.inputField.text = "";
            scoreValue.InputFieldInfos.inputField.text = "";

        }

        public void SetData(QuestionDTO question)
        {
            questionDto = question;

            questionField.InputFieldInfos.inputField.text = question.QuestionTitle;
            timeLimit.InputFieldInfos.inputField.text = Convert.ToInt32(question.QuestionTimeLimit).ToString();
            var answers = question.QuestionAnswers.ToList();
            aAnswer.InputFieldInfos.inputField.text = answers[0].AnswerString;
            bAnswer.InputFieldInfos.inputField.text = answers[1].AnswerString;
            cAnswer.InputFieldInfos.inputField.text = answers[2].AnswerString;
            dAnswer.InputFieldInfos.inputField.text = answers[3].AnswerString;
            scoreValue.InputFieldInfos.inputField.text = question.QuestionScoreValue.ToString();

            List<Toggle> toggles = new()
            {
                aToggle, bToggle, cToggle, dToggle
            };

            for (var i = 0; i < answers.Count; i++)
                if (answers[i].IsCorrectAnswer)
                    toggles[i].isOn = true;
        }

        public List<InputFieldStandaloneInfo> GetInputFieldsStandaloneInfos()
        {
            return _inputFieldInfosList = new List<InputFieldStandaloneInfo>
            {
                questionField,
                aAnswer,
                bAnswer,
                cAnswer,
                dAnswer,
                timeLimit,
                scoreValue
            };
        }

        public List<InputFieldInfos> GetInputFieldsInfos()
        {
            return new List<InputFieldInfos>
            {
                questionField.InputFieldInfos,
                aAnswer.InputFieldInfos,
                bAnswer.InputFieldInfos,
                cAnswer.InputFieldInfos,
                dAnswer.InputFieldInfos,
                timeLimit.InputFieldInfos,
                scoreValue.InputFieldInfos
            };
        }

        public void InputFieldSelected(InputFieldInfos inputFieldInfos)
        {
            var selectedInputField = GetInputFieldsStandaloneInfos()
                .Find(x => x.InputFieldInfos.inputFieldHash == inputFieldInfos.inputFieldHash);
            editQuestionsWindowController.SetCurrentInputField(selectedInputField.InputFieldInfos);
        }

        public QuestionDTO GetQuestionData()
        {
            return new QuestionDTO
            {
                QuizId = questionDto.QuizId,
                QuestionTitle = questionField.InputFieldInfos.inputField.text,
                QuestionTimeLimit = Convert.ToInt32(timeLimit.InputFieldInfos.inputField.text),
                QuestionScoreValue = Convert.ToInt32(scoreValue.InputFieldInfos.inputField.text),
                
                QuestionAnswers = new List<QuestionAnswerDTO>
                {
                    new()
                    {
                        AnswerString = aAnswer.InputFieldInfos.inputField.text,
                        IsCorrectAnswer = aToggle.isOn
                    },
                    new()
                    {
                        AnswerString = bAnswer.InputFieldInfos.inputField.text,
                        IsCorrectAnswer = bToggle.isOn
                    },
                    new()
                    {
                        AnswerString = cAnswer.InputFieldInfos.inputField.text,
                        IsCorrectAnswer = cToggle.isOn
                    },
                    new()
                    {
                        AnswerString = dAnswer.InputFieldInfos.inputField.text,
                        IsCorrectAnswer = dToggle.isOn
                    }
                }
            };
        }

        public void Activate()
        {
            editQuestionsWindowController.AddToListOfQuestions(this);
            gameObject.SetActive(true);
            
            List<Toggle> toggles = new()
            {
                aToggle, bToggle, cToggle, dToggle
            };
            
            for (var i = 0; i < questionDto.QuestionAnswers.Count; i++)
                if (questionDto.QuestionAnswers.ToList()[i].IsCorrectAnswer)
                    toggles[i].isOn = true;
        }

        public void Close()
        {
            editQuestionsWindowController.RemoveOfListOfQuestions(this);
            Reset();
            gameObject.SetActive(false);
        }

        public bool FieldsFilled()
        {
            return !string.IsNullOrEmpty(questionField.InputFieldInfos.inputField.text)
                   && !string.IsNullOrEmpty(aAnswer.InputFieldInfos.inputField.text)
                   && !string.IsNullOrEmpty(bAnswer.InputFieldInfos.inputField.text)
                   && !string.IsNullOrEmpty(cAnswer.InputFieldInfos.inputField.text)
                   && !string.IsNullOrEmpty(dAnswer.InputFieldInfos.inputField.text)
                   && !string.IsNullOrEmpty(timeLimit.InputFieldInfos.inputField.text);
        }
    }
}