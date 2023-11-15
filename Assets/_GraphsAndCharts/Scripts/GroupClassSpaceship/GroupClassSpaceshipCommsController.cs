using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _GraphsAndCharts.Scripts.General;
using _TeacherDashboard.Scripts;
using API_Mestrado_Lucas;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace _GraphsAndCharts.Scripts.GroupClassSpaceship
{
    public class GroupClassSpaceshipCommsController : MonoBehaviour
    {
        [SerializeField] private ApiCommsControllerSO apiCommsController;
        [SerializeField] private GroupClassSpaceshipDataSO groupClassSpaceshipData;
        [SerializeField] private TeacherDataSO teacherDataSo;
        [Space] 
        [SerializeField] private UnityEvent startedComms;
        [SerializeField] private UnityEvent endedComms;
        
        private bool gotQuizes;
        private bool gotStudentWrongAnswers;
        
        private void Awake()
        {
            groupClassSpaceshipData.Reset();
            StartCoroutine(GetAllQuizesOfTeacher(teacherDataSo.TeacherDto.Id));
            StartCoroutine(GetsAwaiter());
        }
        
        private void OnEnable()
        {
            teacherDataSo.CurrentGroupClassChanged += GetStudentsWrongAnswers;
            groupClassSpaceshipData.quizSelected += GetStudentsWrongAnswers;
        }

        private void OnDisable()
        {
            teacherDataSo.CurrentGroupClassChanged -= GetStudentsWrongAnswers;
            groupClassSpaceshipData.quizSelected -= GetStudentsWrongAnswers;
        }
        
        private void GetStudentsWrongAnswers()
        {
            StartCoroutine(GetStudentsWrongAnswers((int)teacherDataSo.CurrentlyChosenGroupClassId));
        }

        private void GetStudentsWrongAnswers(QuizDTO _)
        {
            if(teacherDataSo.CurrentlyChosenGroupClassId != null)
                StartCoroutine(GetStudentsWrongAnswers((int)teacherDataSo.CurrentlyChosenGroupClassId));
        }
        
        private IEnumerator GetAllQuizesOfTeacher(int teacherId)
        {
            startedComms?.Invoke();
            apiCommsController.StartedComms();
            using var webRequest =
                UnityWebRequest.Get(ApiPaths.GET_QUIZES_BY_TEACHER(apiCommsController.UseCloudPath) + teacherId);

            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(webRequest.error);
                Debug.LogError("<color=yellow> All Questions Get error </color>");
                apiCommsController.CommsError();
            }
            else
            {
                groupClassSpaceshipData.AllQuizesOfTeacher = JsonConvert.DeserializeObject<List<QuizDTO>>(
                    webRequest.downloadHandler.text);

                gotQuizes = true;
                apiCommsController.CommsSuccess();
                Debug.Log("<color=yellow> All Questions Get success </color>");
            }

            endedComms?.Invoke();
            apiCommsController.EndedComms();
            webRequest.Dispose();
        }
        
        private IEnumerator GetStudentsWrongAnswers(int groupClassId)
        {
            if (groupClassSpaceshipData.CurrentSelectedQuiz == null) yield break;
            
            startedComms?.Invoke();
            apiCommsController.StartedComms();
            
            using var webRequest =
                UnityWebRequest.Get(ApiPaths.GET_STUDENTS_WRONG_ANSWERS_BY_GROUPCLASS(apiCommsController.UseCloudPath) + groupClassId);

            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(webRequest.error);
                Debug.LogError("<color=yellow> Student Wrong Answers Get error </color>");
                apiCommsController.CommsError();
            }
            else
            {
                var allStudentWrongAnswers = JsonConvert.DeserializeObject<List<StudentWrongAnswersDTO>>(
                        webRequest.downloadHandler.text);

                groupClassSpaceshipData.AllStudentWrongAnswersOfQuiz = allStudentWrongAnswers
                    .Where(x => x.QuizId == groupClassSpaceshipData.CurrentSelectedQuiz.Id).ToList();
                
                groupClassSpaceshipData.StudentWrongAnswersGot();
                
                gotStudentWrongAnswers = true;
                apiCommsController.CommsSuccess();

                Debug.Log("<color=yellow> Student Wrong Answers Get success </color>");
            }

            endedComms?.Invoke();
            apiCommsController.EndedComms();
            webRequest.Dispose();
        }
        
        private IEnumerator GetsAwaiter()
        {
            while (!gotQuizes || !gotStudentWrongAnswers)
                yield return null;
            
            groupClassSpaceshipData.AllQuizesOfTeacherGot();
        }

    }
}
