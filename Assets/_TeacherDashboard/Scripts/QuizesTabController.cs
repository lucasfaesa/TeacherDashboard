using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using API_Mestrado_Lucas;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace _TeacherDashboard.Scripts
{
    public class QuizesTabController : MonoBehaviour
    {
        [SerializeField] private ApiCommsControllerSO apiCommsController;
        [SerializeField] private TeacherDataSO teacherData;
        [SerializeField] private QuizesSO quizesSo;
        [Space] 
        [SerializeField] private GameObject content;
        [Space] 
        [SerializeField] private QuizListObjectDisplay quizListObjectPrefab;
        [SerializeField] private Transform instantiationContent;
        [Space] 
        [SerializeField] private List<QuizListObjectDisplay> quizListObjectPool;
        [Space] 
        [SerializeField] private EditQuestionsWindowController editQuestionsWindowController;
        [Space] 
        [SerializeField] private UnityEvent deleteSuccess;
        [SerializeField] private UnityEvent deleteError;
        
        private List<QuizDTO> _quizesSelected = new();
        
        private void OnEnable()
        {
            quizesSo.UpdateQuizesData += GetQuizes;
            _quizesSelected = new();
            GetQuizes();
        }

        private void OnDisable()
        {
            quizesSo.UpdateQuizesData -= GetQuizes;
        }

        private void GetQuizes()
        {
            StartCoroutine(GetQuizesRoutine());
        }

        private IEnumerator GetQuizesRoutine()
        {
            apiCommsController.StartedComms();

            using var webRequest =
                UnityWebRequest.Get(ApiPaths.GET_QUIZES_BY_TEACHER(apiCommsController.UseCloudPath) +
                                    teacherData.TeacherDto.Id);
            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(webRequest.error);
                Debug.LogError("<color=yellow> Quizes get error </color>");
                apiCommsController.CommsError();
            }
            else
            {
                apiCommsController.CommsSuccess();
                quizesSo.Quizes =
                    JsonConvert.DeserializeObject<List<QuizDTO>>(webRequest.downloadHandler.text);

                GenerateQuestionList();

                Debug.Log("<color=yellow> Quizes get success </color>");
            }

            apiCommsController.EndedComms();
            webRequest.Dispose();
        }

        public void GenerateQuestionList()
        {
            foreach (var quizObject in quizListObjectPool)
                quizObject.gameObject.SetActive(false);

            var quizes = quizesSo.Quizes;
            
            if (quizListObjectPool.Count < quizes.Count)
                InstantiateMoreSubjectQuestionsObjects(quizes.Count -
                                                       quizListObjectPool.Count);
            
            for (var i = 0; i < quizes.Count; i++)
            {
                quizListObjectPool[i].SetInfos(quizes[i], this);
                quizListObjectPool[i].gameObject.SetActive(true);
            }
        }

        private void InstantiateMoreSubjectQuestionsObjects(int quantity)
        {
            for (var i = 0; i < quantity; i++)
            {
                var newObject = Instantiate(quizListObjectPrefab, instantiationContent);
                newObject.editQuestionsWindowController = editQuestionsWindowController;
                quizListObjectPool.Add(newObject);
            }
        }
        
        public void QuizSelected(QuizDTO quiz)
        {
            if (!_quizesSelected.Contains(quiz))
                _quizesSelected.Add(quiz);
        }

        public void QuizDeselected(QuizDTO quiz)
        {
            if (!_quizesSelected.Contains(quiz))
                _quizesSelected.Remove(quiz);
        }

        public void ChangeQuizActiveStatus(QuizDTO quiz)
        {
            StartCoroutine(ChangeQuizActiveStatusRoutine(quiz));
        }
        
        private IEnumerator ChangeQuizActiveStatusRoutine(QuizDTO quiz)
        {
            apiCommsController.StartedComms();
            using var webRequest =
                UnityWebRequest.Post(ApiPaths.CHANGE_ACTIVE_QUIZ(apiCommsController.UseCloudPath), "POST");

            var bodyRaw = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(quiz));
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(webRequest.error);
                Debug.LogError("<color=yellow> Update Quizes error </color>");
                apiCommsController.CommsError();
            }
            else
            {
                apiCommsController.CommsSuccess();
                Debug.Log("<color=yellow> Update Quizes success </color>");
            }

            apiCommsController.EndedComms();
            webRequest.Dispose();
        }
        
        public void DeleteSelectedQuizes()
        {
            if (_quizesSelected.Count == 0) return;

            StartCoroutine(Delete(_quizesSelected));
        }
        
        private IEnumerator Delete(List<QuizDTO> quizes)
        { 
            apiCommsController.StartedComms();
            using var webRequest =
                UnityWebRequest.Post(ApiPaths.BATCH_DELETE_QUIZES(apiCommsController.UseCloudPath), "POST");

            var bodyRaw = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(quizes));
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(webRequest.error);
                Debug.LogError("<color=yellow> Delete error </color>");
                deleteError?.Invoke();
                apiCommsController.CommsError();
            }
            else
            {
                apiCommsController.CommsSuccess();
                deleteSuccess?.Invoke();
                Debug.Log("<color=yellow> Delete success </color>");
            }

            apiCommsController.EndedComms();
            webRequest.Dispose();

            DeletionCompleted();
        }

        private void DeletionCompleted()
        {
            _quizesSelected = new List<QuizDTO>();
            quizesSo.UpdateQuizes();
        }
    }
}