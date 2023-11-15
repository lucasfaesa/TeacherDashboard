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

namespace _GraphsAndCharts.Scripts.Student
{
    public class StudentsCommsController : MonoBehaviour
    {
        [SerializeField] private ApiCommsControllerSO apiCommsController;
        [SerializeField] private StudentsSessionsDataSO studentSessionsDataSo;
        [SerializeField] private TeacherDataSO teacherDataSo;
        [SerializeField] private LevelsDataSO levelsDataSo;
        [Space] 
        [SerializeField] private UnityEvent startedComms;
        [SerializeField] private UnityEvent endedComms;
        private void Awake()
        {
            studentSessionsDataSo.Reset();
        }

        private void OnEnable()
        {
            teacherDataSo.CurrentGroupClassChanged += GetStudentSessionsAndLevels;
        }

        private void OnDisable()
        {
            teacherDataSo.CurrentGroupClassChanged -= GetStudentSessionsAndLevels;
        }

        private void GetStudentSessionsAndLevels()
        {
            StartCoroutine(GetStudentCompleteSession((int)teacherDataSo.CurrentlyChosenGroupClassId));
        }

        private IEnumerator GetStudentCompleteSession(int groupClassId)
        {
            startedComms?.Invoke();
            apiCommsController.StartedComms();
            using var webRequest =
                UnityWebRequest.Get(ApiPaths.STUDENTS_LEVEL_SESSIONS_BY_GROUP_CLASS_ID(apiCommsController.UseCloudPath) +
                                    groupClassId);

            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(webRequest.error);
                Debug.LogError("<color=yellow> Student Sessions Get error </color>");
                apiCommsController.CommsError();
            }
            else
            {
                studentSessionsDataSo.CompleteStudentsSession =
                    JsonConvert.DeserializeObject<List<StudentAndCompleteLevelsSessionsDTO>>(
                        webRequest.downloadHandler.text);
                studentSessionsDataSo.StudentSessionGot();
                StartCoroutine(GetQuizesOfTeacher(teacherDataSo.TeacherDto.Id));
                apiCommsController.CommsSuccess();
                Debug.Log("<color=yellow> Student Sessions Get success </color>");
            }

            apiCommsController.EndedComms();
            webRequest.Dispose();

            StartCoroutine(GetLevelsOfGroupClass(groupClassId));
        }

        private IEnumerator GetLevelsOfGroupClass(int groupClassId)
        {
            apiCommsController.StartedComms();
            using var webRequest =
                UnityWebRequest.Get(ApiPaths.GROUP_CLASS_AND_LEVELS(apiCommsController.UseCloudPath) + groupClassId);

            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(webRequest.error);
                Debug.LogError("<color=yellow> Levels Get error </color>");
                apiCommsController.CommsError();
            }
            else
            {
                var groupClass = JsonConvert.DeserializeObject<GroupClassDTO>(webRequest.downloadHandler.text);

                List<LevelDTO> levelDtos = new();

                if (groupClass != null)
                    foreach (var groupClassSubjectThemeDto in groupClass.GroupClassSubjectThemes)
                    {
                        foreach (var levelDto in groupClassSubjectThemeDto.SubjectTheme.Levels)
                            levelDto.SubjectTheme = groupClassSubjectThemeDto.SubjectTheme;
                        levelDtos.AddRange(groupClassSubjectThemeDto.SubjectTheme.Levels);
                    }

                studentSessionsDataSo.SetLevelsOfGroupClass(levelDtos);

                apiCommsController.CommsSuccess();
                Debug.Log("<color=yellow> Levels Get success </color>");
            }

            StartCoroutine(GetAllLevels());
            apiCommsController.EndedComms();
            webRequest.Dispose();
        }
        
        private IEnumerator GetQuizesOfTeacher(int teacherId)
        {
            apiCommsController.StartedComms();
            using var webRequest =
                UnityWebRequest.Get(ApiPaths.GET_QUIZES_BY_TEACHER(apiCommsController.UseCloudPath) + teacherId);

            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(webRequest.error);
                Debug.LogError("<color=yellow> Quizes Get error </color>");
                apiCommsController.CommsError();
            }
            else
            {
                var quizes = JsonConvert.DeserializeObject<List<QuizDTO>>(webRequest.downloadHandler.text);
                levelsDataSo.AllQuizesOfThisTeacher = quizes;
                levelsDataSo.ActiveQuizesOfThisTeacher = quizes.Where(x => x.Active).ToList();
            
                apiCommsController.CommsSuccess();
                Debug.Log("<color=yellow> Quizes Get success </color>");
            }
        
            apiCommsController.EndedComms();
            webRequest.Dispose();
        }
        
        private IEnumerator GetAllLevels()
        {
            apiCommsController.StartedComms();
            using UnityWebRequest webRequest = UnityWebRequest.Get(ApiPaths.LEVEL_URL(apiCommsController.UseCloudPath));
       
            yield return webRequest.SendWebRequest();
           
            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(webRequest.error);
                Debug.LogError("<color=yellow> Levels Get error </color>");
                apiCommsController.CommsError();
            }
            else
            {
                levelsDataSo.AllLevelsOfGame = JsonConvert.DeserializeObject<List<LevelDTO>>(webRequest.downloadHandler.text);
                apiCommsController.CommsSuccess();
                
                studentSessionsDataSo.SetAllLevels(levelsDataSo.AllLevelsOfGame);
                
                Debug.Log("<color=yellow> Levels Get success </color>");
            }
            endedComms?.Invoke();
            apiCommsController.EndedComms();
            webRequest.Dispose();
        }
    
    }
}