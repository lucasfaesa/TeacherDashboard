using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using API_Mestrado_Lucas;
using Mapster;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace _TeacherDashboard.Scripts
{
    public class EditQuestionsWindowController : MonoBehaviour
    {
        [SerializeField] private TeacherDataSO teacherDataSo;
        [SerializeField] private ApiCommsControllerSO apiCommsController;
        [SerializeField] private QuizesSO quizesSo;
        [Space] 
        [SerializeField] private TMP_InputField titleText;
        [Space] 
        [SerializeField] private GameObject content;
        [Space] 
        [SerializeField] private QuestionObjectDisplay questionObjectPrefab;
        [SerializeField] private Transform instantiationContent;
        [SerializeField] private List<QuestionObjectDisplay> questionObjectsPool;
        [Space] 
        [SerializeField] private GameObject addNewQuestionObjectButton;
        [Space]
        [SerializeField] private UnityEvent fieldsError;
        [SerializeField] private UnityEvent saveError;
        [SerializeField] private UnityEvent saveErrorTitle;
        [SerializeField] private UnityEvent notEnoughQuestionsError;
        [SerializeField] private UnityEvent saveSuccess;
        


        private TMP_InputField _currentInputField;
        private int _currentInputFieldIndex;
        private readonly List<InputFieldInfos> _inputFieldsList = new();

        private List<QuestionObjectDisplay> _questionObjects = new();

        private List<QuestionDTO> _questions = new();

        private int _currentQuizId;

        private bool creatingNewQuiz;
        
        private void Reset()
        {
            _questionObjects = new List<QuestionObjectDisplay>();

            foreach (var questionObjectDisplay in questionObjectsPool) questionObjectDisplay.Close();

            _currentQuizId = 0;
            questionObjectsPool[0].SetData(BoilerplateQuestionWithQuizId());
            questionObjectsPool[0].Activate();
            _inputFieldsList.AddRange(questionObjectsPool[0].GetInputFieldsInfos());
            _currentInputField = _inputFieldsList[0].inputField;
            _currentInputFieldIndex = 0;
            _inputFieldsList[0].inputField.ActivateInputField();
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Tab)) JumpToNextInputField();
        }

        public void SetCurrentInputField(InputFieldInfos inputField)
        {
            _currentInputFieldIndex = _inputFieldsList.FindIndex(x => x.inputFieldHash == inputField.inputFieldHash);
        }

        private void JumpToNextInputField()
        {
            Debug.Log(_inputFieldsList.Count);
            if (_inputFieldsList.Count == 0 || !content.activeInHierarchy) return;

            _currentInputFieldIndex++;

            if (_currentInputFieldIndex > _inputFieldsList.Count - 1)
                _currentInputFieldIndex = 0;

            _inputFieldsList[_currentInputFieldIndex].inputField.ActivateInputField();
        }

        public void CreateNewQuiz()
        {
            creatingNewQuiz = true;
            Reset();
            _currentQuizId = 0;
            content.SetActive(true);
            titleText.text = "";
            foreach (var questionObjectDisplay in questionObjectsPool) questionObjectDisplay.Close();
            questionObjectsPool[0].SetData(new QuestionDTO
            {
                QuizId = 0,
                QuestionAnswers = new List<QuestionAnswerDTO>
                {
                    new()
                    {
                        AnswerString = "",
                        IsCorrectAnswer = true
                    },
                    new()
                    {
                        AnswerString = "",
                        IsCorrectAnswer = false
                    },
                    new()
                    {
                        AnswerString = "",
                        IsCorrectAnswer = false
                    },
                    new()
                    {
                        AnswerString = "",
                        IsCorrectAnswer = false
                    }
                },
                QuestionTitle = "",
                QuestionScoreValue = 5,
                QuestionTimeLimit = 60,
            });
            
            questionObjectsPool[0].Activate();
        }
        
        public void EditQuizQuestions(QuizDTO quiz)
        {
            creatingNewQuiz = false;
            Reset();
            _currentQuizId = quiz.Id;
            content.SetActive(true);
            GenerateQuestionsList(quiz);
        }

        public void CloseContent()
        {
            content.SetActive(false);
        }

        private void GenerateQuestionsList(QuizDTO quiz)
        {
            titleText.text = quiz.Name;
            
            foreach (var questionObjectDisplay in questionObjectsPool) questionObjectDisplay.Close();

            _questions = quiz.Questions.ToList();

            if (_questions.Count == 0) return;

            if (_questions.Count > questionObjectsPool.Count)
                InstantiateMorePrefabs(_questions.Count - questionObjectsPool.Count);

            for (var i = 0; i < _questions.Count; i++)
            {
                questionObjectsPool[i].SetData(_questions[i]);
                questionObjectsPool[i].Activate();
            }
        }

        private void InstantiateMorePrefabs(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var obj = Instantiate(questionObjectPrefab, instantiationContent);
                questionObjectsPool.Add(obj);
                obj.editQuestionsWindowController = this;

                _inputFieldsList.AddRange(obj.GetInputFieldsInfos());
            }

            addNewQuestionObjectButton.transform.SetAsLastSibling();
        }

        public void AddQuestionDisplayToScrollView()
        {
            foreach (var questionObjectDisplay in questionObjectsPool)
            {
                if (questionObjectDisplay.isActiveAndEnabled) continue;

                questionObjectDisplay.SetData(BoilerplateQuestionWithQuizId());
                questionObjectDisplay.Activate();
                addNewQuestionObjectButton.transform.SetAsLastSibling();
                _inputFieldsList.AddRange(questionObjectDisplay.GetInputFieldsInfos());
                return;
            }

            var obj = Instantiate(questionObjectPrefab, instantiationContent);
            questionObjectsPool.Add(obj);
            obj.editQuestionsWindowController = this;
            obj.SetData(BoilerplateQuestionWithQuizId());
            obj.Activate();
            addNewQuestionObjectButton.transform.SetAsLastSibling();

            _inputFieldsList.AddRange(obj.GetInputFieldsInfos());
        }

        private QuestionDTO BoilerplateQuestionWithQuizId()
        {
            return new QuestionDTO
            {
                QuizId = _currentQuizId,
                QuestionAnswers = new List<QuestionAnswerDTO>
                {
                    new()
                    {
                        AnswerString = "",
                        IsCorrectAnswer = true
                    },
                    new()
                    {
                        AnswerString = "",
                        IsCorrectAnswer = false
                    },
                    new()
                    {
                        AnswerString = "",
                        IsCorrectAnswer = false
                    },
                    new()
                    {
                        AnswerString = "",
                        IsCorrectAnswer = false
                    }
                },
                QuestionTitle = "",
                QuestionScoreValue = 5,
                QuestionTimeLimit = 60,
            };
        }

        public void AddToListOfQuestions(QuestionObjectDisplay script)
        {
            _questionObjects.Add(script);
        }

        public void RemoveOfListOfQuestions(QuestionObjectDisplay script)
        {
            if (_questionObjects.Count == 0) return;

            if (_questionObjects.Find(x => x == script))
            {
                _questionObjects.Remove(script);

                foreach (var inputField in script.GetInputFieldsStandaloneInfos())
                    _inputFieldsList.Remove(inputField.InputFieldInfos);
                Debug.Log(_inputFieldsList.Count);
            }
        }

        public void SaveQuestionsOnAPI()
        {
            foreach (var questionObjectDisplay in _questionObjects)
                if (!questionObjectDisplay.FieldsFilled())
                {
                    fieldsError?.Invoke();
                    return;
                }

            StartCoroutine(SaveQuiz());
        }

        private IEnumerator SaveQuiz()
        {
            if (_questionObjects.Count == 0)
            {
                notEnoughQuestionsError?.Invoke();
                yield break;
            }

            if (string.IsNullOrEmpty(titleText.text) || string.IsNullOrWhiteSpace(titleText.text))
            {
                saveErrorTitle?.Invoke();
                yield break;
            }

            apiCommsController.StartedComms();

            List<QuestionDTO> questions = new();
            
            foreach (var questionObjectDisplay in _questionObjects)
                questions.Add(questionObjectDisplay.GetQuestionData());
            
            QuizDTO quiz = new QuizDTO
            {
                Id = _currentQuizId,
                Name = titleText.text,
                TeacherId = teacherDataSo.TeacherDto.Id,
                Questions = questions
            };
            
            using var webRequest =
                UnityWebRequest.Post(ApiPaths.POST_QUIZ(apiCommsController.UseCloudPath), "POST");

            var bodyRaw = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(quiz));
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(webRequest.error);
                Debug.LogError("<color=yellow> Batch post error </color>");
                saveError?.Invoke();
                apiCommsController.CommsError();
            }
            else
            {
                apiCommsController.CommsSuccess();
                quizesSo.UpdateQuizes();
                Debug.Log("<color=yellow> Batch post success </color>");
                saveSuccess?.Invoke();
                
                if (creatingNewQuiz)
                {
                    creatingNewQuiz = false;
                    CloseContent();
                }
                //Reset();
            }

            apiCommsController.EndedComms();
            webRequest.Dispose();
        }
    }
}