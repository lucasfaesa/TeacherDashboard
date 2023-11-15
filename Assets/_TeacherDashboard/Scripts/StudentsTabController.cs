using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using API_Mestrado_Lucas;
using Mapster;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace _TeacherDashboard.Scripts
{
    public class StudentsTabController : MonoBehaviour
    {
        [SerializeField] private ApiCommsControllerSO apiCommsController;
        [SerializeField] private TeacherDataSO teacherData;

        [Space] [SerializeField] private EditStudentWindowController editStudentWindowController;

        [Space] [SerializeField] private GameObject content;

        [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;

        [Space] [SerializeField] private StudentListObjectDisplay studentListObjectPrefab;

        [SerializeField] private Transform instantiationContent;

        [Space] [SerializeField] private List<StudentListObjectDisplay> studentListObjectPool;

        [Space] [SerializeField] private UnityEvent deleteSuccess;

        [SerializeField] private UnityEvent deleteError;

        private List<StudentCompleteInfoDTO> _studentsSelected = new();
        private List<Coroutine> deleteRoutines = new();
        private List<bool> deletionRoutinesStatus = new();

        private bool initialized;

        private void Start()
        {
            initialized = true;
        }

        private void OnEnable()
        {
            _studentsSelected = new List<StudentCompleteInfoDTO>();
            apiCommsController.teacherDataUpdated += GenerateStudentsList;

            if (initialized)
                GenerateStudentsList();
        }

        private void OnDisable()
        {
            apiCommsController.teacherDataUpdated -= GenerateStudentsList;

            foreach (var deleteRoutine in deleteRoutines) StopCoroutine(deleteRoutine);
        }

        private void GenerateStudentsList()
        {
            foreach (var studentObj in studentListObjectPool) studentObj.gameObject.SetActive(false);

            if (studentListObjectPool.Count < teacherData.TeacherDto.Students.Count)
                InstantiateMoreStudentsObjects(teacherData.TeacherDto.Students.Count - studentListObjectPool.Count);

            var students = teacherData.TeacherDto.Students.ToList();

            for (var i = 0; i < students.Count; i++)
            {
                var student = students[i];
                studentListObjectPool[i].SetInfos(student, this, editStudentWindowController);
                studentListObjectPool[i].gameObject.SetActive(true);
            }

            UpdateCanvas();
        }

        private void InstantiateMoreStudentsObjects(int quantity)
        {
            for (var i = 0; i < quantity; i++)
            {
                var newObject = Instantiate(studentListObjectPrefab, instantiationContent);
                
                studentListObjectPool.Add(newObject);
            }
        }

        public void StudentSelected(StudentCompleteInfoDTO student)
        {
            if (_studentsSelected.Find(x => x.Id == student.Id) == null)
                _studentsSelected.Add(student);
        }

        public void StudentDeselected(StudentCompleteInfoDTO student)
        {
            if (_studentsSelected.Find(x => x.Id == student.Id) != null)
                _studentsSelected.Remove(student);
        }

        public void DeleteSelectedStudents()
        {
            if (_studentsSelected.Count == 0) return;

            deleteRoutines.Add(StartCoroutine(Delete(_studentsSelected)));
        }

        private IEnumerator Delete(List<StudentCompleteInfoDTO> studentsCompleteInfoDto)
        {
            var studentsDto = studentsCompleteInfoDto.Adapt<List<StudentDTO>>();

            apiCommsController.StartedComms();
            using var webRequest =
                UnityWebRequest.Post(ApiPaths.BATCH_DELETE_STUDENT(apiCommsController.UseCloudPath), "POST");

            var bodyRaw = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(studentsDto));
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
            _studentsSelected = new List<StudentCompleteInfoDTO>();
            deleteRoutines = new List<Coroutine>();
            deletionRoutinesStatus = new List<bool>();

            apiCommsController.UpdateTeacherData();
        }

        public void UpdateCanvas()
        {
            StartCoroutine(UpdateCanvasRoutine());
        }

        private IEnumerator UpdateCanvasRoutine()
        {
            //Canvas.ForceUpdateCanvases();
            yield return new WaitForEndOfFrame();
            verticalLayoutGroup.enabled = false;

            //Canvas.ForceUpdateCanvases();
            verticalLayoutGroup.enabled = true;
            Canvas.ForceUpdateCanvases();
        }
    }
}