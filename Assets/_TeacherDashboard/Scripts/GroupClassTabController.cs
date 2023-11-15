using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using API_Mestrado_Lucas;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace _TeacherDashboard.Scripts
{
    public class GroupClassTabController : MonoBehaviour
    {
        [SerializeField] private ApiCommsControllerSO apiCommsController;
        [SerializeField] private TeacherDataSO teacherData;

        [Space] [SerializeField] private GameObject content;

        [Space] [SerializeField] private GroupClassListObjectDisplay groupClassListObjectPrefab;

        [SerializeField] private Transform instantiationContent;

        [Space] [SerializeField] private List<GroupClassListObjectDisplay> groupClassListObjectPool;

        [Space] [SerializeField] private AssignLevelsWindowController assignLevelsWindowController;

        [Space] [SerializeField] private UnityEvent deleteSuccess;

        [SerializeField] private UnityEvent deleteError;

        private List<GroupClassDTO> _groupClassesSelected = new();
        private List<Coroutine> deleteRoutines = new();
        private List<bool> deletionRoutinesStatus = new();

        private void OnEnable()
        {
            _groupClassesSelected = new List<GroupClassDTO>();
            GenerateGroupClassesList();
            apiCommsController.teacherDataUpdated += GenerateGroupClassesList;
        }

        private void OnDisable()
        {
            apiCommsController.teacherDataUpdated -= GenerateGroupClassesList;

            foreach (var deleteRoutine in deleteRoutines) StopCoroutine(deleteRoutine);
        }

        public void GenerateGroupClassesList()
        {
            foreach (var groupClassObj in groupClassListObjectPool) groupClassObj.gameObject.SetActive(false);

            if (groupClassListObjectPool.Count < teacherData.TeacherDto.GroupClasses.Count)
                InstantiateMoreGroupClassesObjects(teacherData.TeacherDto.GroupClasses.Count -
                                                   groupClassListObjectPool.Count);

            var groupClasses = teacherData.TeacherDto.GroupClasses.ToList();

            for (var i = 0; i < groupClasses.Count; i++)
            {
                var groupClass = groupClasses[i];
                groupClassListObjectPool[i].SetInfos(groupClass, this);
                groupClassListObjectPool[i].gameObject.SetActive(true);
            }
        }

        private void InstantiateMoreGroupClassesObjects(int quantity)
        {
            for (var i = 0; i < quantity; i++)
            {
                var newObject = Instantiate(groupClassListObjectPrefab, instantiationContent);

                groupClassListObjectPool.Add(newObject);
            }
        }

        public void GroupClassSelected(GroupClassDTO groupClass)
        {
            if (_groupClassesSelected.Find(x => x == groupClass) == null)
                _groupClassesSelected.Add(groupClass);
        }

        public void GroupClassDeselected(GroupClassDTO groupClass)
        {
            if (_groupClassesSelected.Find(x => x == groupClass) == null)
                _groupClassesSelected.Remove(groupClass);
        }

        public void DeleteSelectedGroupClasses()
        {
            if (_groupClassesSelected.Count == 0) return;

            deleteRoutines.Add(StartCoroutine(Delete(_groupClassesSelected)));
        }

        private IEnumerator Delete(List<GroupClassDTO> groupClassesDto)
        {
            apiCommsController.StartedComms();
            using var webRequest =
                UnityWebRequest.Post(ApiPaths.BATCH_DELETE_GROUPCLASSES(apiCommsController.UseCloudPath), "POST");

            var bodyRaw = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(groupClassesDto));
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
            _groupClassesSelected = new List<GroupClassDTO>();
            deleteRoutines = new List<Coroutine>();
            deletionRoutinesStatus = new List<bool>();

            apiCommsController.UpdateTeacherData();
        }

        public void ShowAssignLevelsWindow(int groupClassId)
        {
            assignLevelsWindowController.ShowContent(groupClassId);
        }
    }
}