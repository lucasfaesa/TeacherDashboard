using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _GraphsAndCharts.Scripts.General;
using _TeacherDashboard.Scripts;
using API_Mestrado_Lucas;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class GroupClassCommsController : MonoBehaviour
{
    [SerializeField] private ApiCommsControllerSO apiCommsController;
    [SerializeField] private GroupClassSessionsDataSO groupClassSessionsDataSo;
    [SerializeField] private TeacherDataSO teacherDataSo;
    [SerializeField] private LevelsDataSO levelsDataSo;
    [Space] 
    [SerializeField] private UnityEvent startedComms;
    [SerializeField] private UnityEvent endedComms;
    private void Awake()
    {
        groupClassSessionsDataSo.Reset();

        //GetSessions();
    }

    private void OnEnable()
    {
        teacherDataSo.CurrentGroupClassChanged += GetSessions;
    }

    private void OnDisable()
    {
        teacherDataSo.CurrentGroupClassChanged -= GetSessions;
    }

    private void GetSessions()
    {
        if (teacherDataSo.UseDebug)
        {
            StartCoroutine(GetStudentCompleteSession(new TeacherIdAndGroupClassIdDTO
            {
                TeacherId = teacherDataSo.DebugData.teacherId,
                GroupClassId = teacherDataSo.DebugData.currentlyChosenGroupClass
            }));
        }
        else
        {
            StartCoroutine(GetStudentCompleteSession(new TeacherIdAndGroupClassIdDTO
            {
                TeacherId = teacherDataSo.TeacherDto.Id,
                GroupClassId = teacherDataSo.CurrentlyChosenGroupClassId != null
                    ? teacherDataSo.CurrentlyChosenGroupClassId.Value
                    : teacherDataSo.TeacherDto.GroupClasses.ToList()[0].Id
            }));

            Debug.Log("Chosen Group Class ID: " + teacherDataSo.CurrentlyChosenGroupClassId != null
                ? teacherDataSo.CurrentlyChosenGroupClassId.Value
                : teacherDataSo.TeacherDto.GroupClasses.ToList()[0].Id);
        }
        
    }


    private IEnumerator GetStudentCompleteSession(TeacherIdAndGroupClassIdDTO dto)
    {
        
        startedComms?.Invoke();
        apiCommsController.StartedComms();
        using var webRequest =
            UnityWebRequest.Post(ApiPaths.COMPLETE_STUDENT_SESSION_TEACHER_GROUPCLASS(apiCommsController.UseCloudPath),
                "POST");

        var bodyRaw = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dto));
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
            or UnityWebRequest.Result.DataProcessingError)
        {
            Debug.Log(webRequest.error);
            Debug.LogError("<color=yellow> Sessions Get error </color>");
            apiCommsController.CommsError();
        }
        else
        {
            groupClassSessionsDataSo.CompleteGroupClassSession =
                JsonConvert.DeserializeObject<List<CompleteStudentSessionDTO>>(webRequest.downloadHandler.text);
            groupClassSessionsDataSo.GotSessions();

            apiCommsController.CommsSuccess();
            Debug.Log("<color=yellow> Sessions Get success </color>");
            
            StartCoroutine(GetLevelsOfGroupClass(teacherDataSo.CurrentlyChosenGroupClassId != null
                ? teacherDataSo.CurrentlyChosenGroupClassId.Value
                : teacherDataSo.TeacherDto.GroupClasses.ToList()[0].Id));

            StartCoroutine(GetQuizesOfTeacher(teacherDataSo.TeacherDto.Id));
        }
        
        apiCommsController.EndedComms();
        webRequest.Dispose();
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

            levelsDataSo.LevelsOfThisGroupClass = levelDtos;
            StartCoroutine(GetAllLevels());
            apiCommsController.CommsSuccess();
            Debug.Log("<color=yellow> Levels Get success </color>");
        }
        
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
            Debug.Log("<color=yellow> Levels Get success </color>");
        }
        groupClassSessionsDataSo.GotLevels();
        groupClassSessionsDataSo.GotSessionsAndLevels();
            
        endedComms?.Invoke();
        apiCommsController.EndedComms();
        webRequest.Dispose();
    }
}