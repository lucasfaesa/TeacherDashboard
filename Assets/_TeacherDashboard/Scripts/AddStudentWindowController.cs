using System.Collections;
using System.Collections.Generic;
using System.Text;
using API_Mestrado_Lucas;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace _TeacherDashboard.Scripts
{
    public class AddStudentWindowController : MonoBehaviour
    {
        [SerializeField] private ApiCommsControllerSO apiCommsController;

        [Space] [SerializeField] private GameObject content;

        [Space] [SerializeField] private NewStudentObjectDisplay newStudentObjectPrefab;

        [SerializeField] private Transform instantiationContent;
        [SerializeField] private List<NewStudentObjectDisplay> newStudentsObjectPool;

        [Space] [SerializeField] private GameObject addNewStudentButtonContent;

        [Space] [SerializeField] private UnityEvent saveError;

        [SerializeField] private UnityEvent fieldsNotFilled;
        [SerializeField] private UnityEvent saveSuccess;

        private TMP_InputField _currentInputField;
        private int _currentInputFieldIndex;
        private readonly List<InputFieldInfos> _inputFieldsList = new();

        private List<NewStudentObjectDisplay> _newStudentObjects;

        private void Reset()
        {
            _newStudentObjects = new List<NewStudentObjectDisplay>();

            foreach (var newStudentObj in newStudentsObjectPool) newStudentObj.Close();

            newStudentsObjectPool[0].Activate();
            _inputFieldsList.AddRange(newStudentsObjectPool[0].GetInputFieldsInfos());
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

        public void ActivateContent()
        {
            content.SetActive(true);
            Reset();
        }

        public void CloseContent()
        {
            content.SetActive(false);
        }

        public void AddNewStudentToScrollView()
        {
            foreach (var studentObj in newStudentsObjectPool)
            {
                if (studentObj.isActiveAndEnabled) continue;

                studentObj.Activate();
                addNewStudentButtonContent.transform.SetAsLastSibling();
                _inputFieldsList.AddRange(studentObj.GetInputFieldsInfos());
                return;
            }

            var obj = Instantiate(newStudentObjectPrefab, instantiationContent);
            newStudentsObjectPool.Add(obj);
            obj.AddStudentWindowController = this;
            obj.Activate();
            addNewStudentButtonContent.transform.SetAsLastSibling();

            _inputFieldsList.AddRange(obj.GetInputFieldsInfos());
        }

        public void AddToList(NewStudentObjectDisplay script)
        {
            _newStudentObjects.Add(script);
        }

        public void RemoveOfList(NewStudentObjectDisplay script)
        {
            if (_newStudentObjects.Count == 0) return;

            if (_newStudentObjects.Find(x => x == script))
            {
                _newStudentObjects.Remove(script);


                foreach (var inputField in script.GetInputFieldsStandaloneInfos())
                    _inputFieldsList.Remove(inputField.InputFieldInfos);
                Debug.Log(_inputFieldsList.Count);
            }
        }

        public void SaveStudentsOnAPI()
        {
            foreach (var newStudentObjectDisplay in _newStudentObjects)
                if (!newStudentObjectDisplay.FieldsFilled())
                {
                    fieldsNotFilled?.Invoke();
                    return;
                }

            StartCoroutine(SaveStudents());
        }

        private IEnumerator SaveStudents()
        {
            if (_newStudentObjects.Count == 0) yield break;

            apiCommsController.StartedComms();
            List<StudentCompleteInfoDTO> students = new();

            foreach (var studentObj in _newStudentObjects) students.Add(studentObj.GetStudentData());

            using var webRequest =
                UnityWebRequest.Post(ApiPaths.BATCH_POST_STUDENT_URL(apiCommsController.UseCloudPath), "POST");

            var bodyRaw = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(students));
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
                Debug.Log("<color=yellow> Batch post success </color>");
                apiCommsController.UpdateTeacherData();
                saveSuccess?.Invoke();
                Reset();
            }

            apiCommsController.EndedComms();
            webRequest.Dispose();
        }
    }
}