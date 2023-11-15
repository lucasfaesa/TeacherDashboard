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
    public class AssignLevelsWindowController : MonoBehaviour
    {
        [SerializeField] private TeacherDataSO teacherData;
        [SerializeField] private ApiCommsControllerSO apiCommsController;
        [SerializeField] private SubjectsDataSO subjectsData;

        [Space] [SerializeField] private GameObject content;

        [Header("Child Scripts")] [SerializeField]
        private LevelsListDisplay AllLevelsAndQuizesListDisplay;

        [SerializeField] private LevelsListDisplay AssignedLevelsAndQuizesListDisplay;

        [Space] [SerializeField] private UnityEvent saveError;

        [SerializeField] private UnityEvent success;
        private List<SubjectThemeDTO> _assignedSubjectsThemeList = new();
        private List<SubjectThemeDTO> _filteredSubjectsThemesList = new();
        private List<QuizDTO> _assignedQuizes = new(); //not used right now
        private List<QuizDTO> _filteredQuizes = new(); //not used right now

        private List<QuizDTO> quizesOfTeacher = new();
        
        private int currentGroupClassId;

        public void ShowContent(int selectedGroupClassId)
        {
            content.SetActive(true);
            StartCoroutine(GetAllSubjects(selectedGroupClassId));
        }

        private IEnumerator GetAllSubjects(int selectedGroupClassId)
        {
            apiCommsController.StartedComms();

            using var webRequest = UnityWebRequest.Get(ApiPaths.GET_SUBJECTS(apiCommsController.UseCloudPath));
            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(webRequest.error);
                Debug.LogError("<color=yellow> Get Subjects error </color>");
                apiCommsController.CommsError();
            }
            else
            {
                apiCommsController.CommsSuccess();
                subjectsData.SubjectInfo =
                    JsonConvert.DeserializeObject<List<SubjectDTO>>(webRequest.downloadHandler.text);
                StartCoroutine(GetGroupClassAssignedSubjects(selectedGroupClassId));

                Debug.Log("<color=yellow> Get Subjects success </color>");
            }

            apiCommsController.EndedComms();
            webRequest.Dispose();
        }

        private IEnumerator GetGroupClassAssignedSubjects(int selectedGroupClassId)
        {
            apiCommsController.StartedComms();

            currentGroupClassId = selectedGroupClassId;

            using var webRequest =
                UnityWebRequest.Get(ApiPaths.GET_GROUP_CLASS_LEVELS(apiCommsController.UseCloudPath) +
                                    selectedGroupClassId);
            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(webRequest.error);
                Debug.LogError("<color=yellow> Get Assigned Subjects error </color>");
                apiCommsController.CommsError();
            }
            else
            {
                apiCommsController.CommsSuccess();

                _assignedSubjectsThemeList = new List<SubjectThemeDTO>();
                var assignedGroupClass = JsonConvert.DeserializeObject<GroupClassDTO>(webRequest.downloadHandler.text);

                if (assignedGroupClass != null)
                {
                    foreach (var groupClassSubjectThemeDto in assignedGroupClass.GroupClassSubjectThemes)
                        _assignedSubjectsThemeList.Add(groupClassSubjectThemeDto.SubjectTheme);

                    //StartCoroutine(GetQuizesRoutine());
                    InstantiateSubjectsThemesAndQuizes();
                }

                Debug.Log("<color=yellow> Get Assigned Subjects success </color>");
            }

            apiCommsController.EndedComms();
            webRequest.Dispose();
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
                quizesOfTeacher = JsonConvert.DeserializeObject<List<QuizDTO>>(webRequest.downloadHandler.text);
                
                //InstantiateSubjectsThemesAndQuizes();

                Debug.Log("<color=yellow> Quizes get success </color>");
            }

            apiCommsController.EndedComms();
            webRequest.Dispose();
        }

        private void InstantiateSubjectsThemesAndQuizes()
        {
            _filteredSubjectsThemesList = new List<SubjectThemeDTO>();

            foreach (var subject in subjectsData.SubjectInfo)
                _filteredSubjectsThemesList.AddRange(subject.SubjectThemes);

            //_assignedQuizes = quizesOfTeacher.Where(x => x.Active).ToList();
            //_filteredQuizes = quizesOfTeacher.Where(x=> !x.Active).ToList();
            
            if (_filteredSubjectsThemesList.Count == 0 /*&& _filteredQuizes.Count == 0*/) return;

            //filtering all subject themes to remove the already assigned subjectthemes
            if (_assignedSubjectsThemeList.Count > 0)
                for (var i = 0; i < _assignedSubjectsThemeList.Count; i++)
                for (var j = 0; j < _filteredSubjectsThemesList.Count; j++)
                    if (_filteredSubjectsThemesList[j].Id == _assignedSubjectsThemeList[i].Id)
                        _filteredSubjectsThemesList.Remove(_filteredSubjectsThemesList[j]);

            //atrelando de volta os subjects ao theme (na filtragem ele acaba se perdendo [na verdade ele nunca existiu])
            foreach (var subjectThemeDto in _filteredSubjectsThemesList)
                subjectThemeDto.Subject =
                    subjectsData.SubjectInfo.FirstOrDefault(x => x.Id == subjectThemeDto.SubjectId);

            AllLevelsAndQuizesListDisplay.GenerateSubjectList(_filteredSubjectsThemesList, _filteredQuizes, this);
            AssignedLevelsAndQuizesListDisplay.GenerateSubjectList(_assignedSubjectsThemeList, _assignedQuizes, this);
        }
        
        public void ReGenerateLists(LevelsListDisplay.ListCategory category, SubjectThemeDTO subjectTheme, QuizDTO quizDto)
        {
            if (category == LevelsListDisplay.ListCategory.AllLevels)
            {
                if (subjectTheme != null)
                {
                    _assignedSubjectsThemeList.Add(subjectTheme);
                    _filteredSubjectsThemesList.Remove(_assignedSubjectsThemeList.Find(x => x.Id == subjectTheme.Id));
                }

                /*if (quizDto != null)
                {
                    _assignedQuizes.Add(quizDto);
                    _filteredQuizes.Remove(quizDto);
                }*/
            }

            if (category == LevelsListDisplay.ListCategory.AssignedLevels)
            {
                if (subjectTheme != null)
                {
                    _filteredSubjectsThemesList.Add(subjectTheme);
                    _assignedSubjectsThemeList.Remove(_assignedSubjectsThemeList.Find(x => x.Id == subjectTheme.Id));
                }
                /*if (quizDto != null)
                {
                    _filteredQuizes.Add(quizDto);
                    _assignedQuizes.Remove(quizDto);
                }*/
            }

            AllLevelsAndQuizesListDisplay.GenerateSubjectList(_filteredSubjectsThemesList, _filteredQuizes,this);
            AssignedLevelsAndQuizesListDisplay.GenerateSubjectList(_assignedSubjectsThemeList,_assignedQuizes, this);
        }

        public void SaveChanges()
        {
            StartCoroutine(SaveGroupClassLevels());
            //StartCoroutine(SaveAssignedQuizes());
        }

        private IEnumerator SaveGroupClassLevels()
        {
            var groupClassSubjectThemeMaker = new GroupClassSubjectThemeMakerDTO
            {
                GroupClassId = currentGroupClassId,
                SubjectThemesIds = new List<int>()
            };

            foreach (var subjectThemeDto in _assignedSubjectsThemeList)
                groupClassSubjectThemeMaker.SubjectThemesIds.Add(subjectThemeDto.Id);

            apiCommsController.StartedComms();
            using var webRequest =
                UnityWebRequest.Post(ApiPaths.POST_GROUP_CLASS_LEVELS(apiCommsController.UseCloudPath), "POST");

            var bodyRaw = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(groupClassSubjectThemeMaker));
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(webRequest.error);
                Debug.LogError("<color=yellow> Save Group Classes Subject Themes error </color>");
                saveError?.Invoke();
                apiCommsController.CommsError();
            }
            else
            {
                apiCommsController.CommsSuccess();
                success?.Invoke();
                Debug.Log("<color=yellow> Save Group Classes Subject Themes success </color>");
            }

            apiCommsController.EndedComms();
            webRequest.Dispose();
        }
        /*
        private IEnumerator SaveAssignedQuizes()
        {
            _assignedQuizes.ForEach(x=>x.Active = true);
            
            List<QuizDTO> quizesList = new List<QuizDTO>(_assignedQuizes);
            quizesList.AddRange(_filteredQuizes);

            if (quizesList.Count == 0) yield break;

            apiCommsController.StartedComms();
            using var webRequest =
                UnityWebRequest.Post(ApiPaths.CHANGE_ACTIVE_QUIZES(apiCommsController.UseCloudPath), "POST");

            var bodyRaw = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(quizesList));
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(webRequest.error);
                Debug.LogError("<color=yellow> Update Quizes error </color>");
                saveError?.Invoke();
                apiCommsController.CommsError();
            }
            else
            {
                apiCommsController.CommsSuccess();
                success?.Invoke();
                Debug.Log("<color=yellow> Update Quizes success </color>");
            }

            apiCommsController.EndedComms();
            webRequest.Dispose();
        }*/
    }
}