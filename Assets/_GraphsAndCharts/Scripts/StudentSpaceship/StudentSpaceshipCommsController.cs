using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _TeacherDashboard.Scripts;
using API_Mestrado_Lucas;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace _GraphsAndCharts.Scripts.StudentSpaceship
{
    public class StudentSpaceshipCommsController : MonoBehaviour
    {
        [SerializeField] private ApiCommsControllerSO apiCommsController;
        [SerializeField] private StudentSpaceshipDataSO studentSpaceshipData;
        [SerializeField] private TeacherDataSO teacherDataSo;
        [Space] 
        [SerializeField] private UnityEvent startedComms;
        [SerializeField] private UnityEvent endedComms;

        private bool gotSpaceshipSessions;
        private bool gotStudentWrongAnswers;
        
        private void Awake()
        {
            studentSpaceshipData.Reset();
            StartCoroutine(GetAllQuizesOfTeacher(teacherDataSo.TeacherDto.Id));
            StartCoroutine(GetSpaceshipSessions(teacherDataSo.TeacherDto.Id));
            StartCoroutine(GetsAwaiter());
        }
        
        private void OnEnable()
        {
            teacherDataSo.CurrentGroupClassChanged += GetStudentsWrongAnswers;
            studentSpaceshipData.quizSelected += GetStudentsWrongAnswers;
        }

        private void OnDisable()
        {
            teacherDataSo.CurrentGroupClassChanged -= GetStudentsWrongAnswers;
            studentSpaceshipData.quizSelected -= GetStudentsWrongAnswers;
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
                studentSpaceshipData.AllQuizesOfTeacher = JsonConvert.DeserializeObject<List<QuizDTO>>(
                    webRequest.downloadHandler.text);
                
                apiCommsController.CommsSuccess();
                
                Debug.Log("<color=yellow> All Questions Get success </color>");
            }
            
            apiCommsController.EndedComms();
            webRequest.Dispose();
        }

        private IEnumerator GetStudentsWrongAnswers(int groupClassId)
        {
            if (studentSpaceshipData.CurrentSelectedQuiz == null) yield break;

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
                var allStudentsWrongAnswers = JsonConvert.DeserializeObject<List<StudentWrongAnswersDTO>>(
                        webRequest.downloadHandler.text);

                studentSpaceshipData.AllStudentWrongAnswersOfQuiz = allStudentsWrongAnswers
                    .Where(x => x.QuizId == studentSpaceshipData.CurrentSelectedQuiz.Id).ToList();
                
                apiCommsController.CommsSuccess();
                gotStudentWrongAnswers = true;
                studentSpaceshipData.StudentWrongAnswersGot();
                
                Debug.Log("<color=yellow> Student Wrong Answers Get success </color>");
            }

            
            apiCommsController.EndedComms();
            webRequest.Dispose();
        }
        
        private IEnumerator GetSpaceshipSessions(int teacherId)
        {
            startedComms?.Invoke();
            apiCommsController.StartedComms();
            using var webRequest =
                UnityWebRequest.Get(ApiPaths.GET_SESSIONS_OF_SPACESHIP_LEVELS(apiCommsController.UseCloudPath) + teacherId);

            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(webRequest.error);
                Debug.LogError("<color=yellow> Spaceship Sessions Score Get error </color>");
                apiCommsController.CommsError();
            }
            else
            {
                studentSpaceshipData.AllSpaceshipSessionsOfTeacher = JsonConvert.DeserializeObject<List<SessionDTO>>(
                    webRequest.downloadHandler.text);
                
                apiCommsController.CommsSuccess();
                gotSpaceshipSessions = true;
                Debug.Log("<color=yellow> Spaceship Sessions Score Get success </color>");
            }

            endedComms?.Invoke();
            apiCommsController.EndedComms();
            webRequest.Dispose();
        }

        private IEnumerator GetsAwaiter()
        {
            while (!gotSpaceshipSessions || !gotStudentWrongAnswers)
                yield return null;
            
            studentSpaceshipData.AllQuestionsGot();
        }

        
    }
}
