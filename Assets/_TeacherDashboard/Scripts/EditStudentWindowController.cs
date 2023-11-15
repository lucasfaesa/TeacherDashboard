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
    public class EditStudentWindowController : MonoBehaviour
    {
        [SerializeField] private ApiCommsControllerSO apiCommsController;

        [Space] [SerializeField] private GameObject content;

        [SerializeField] private EditStudentObjectDisplay editStudentObjectDisplayObject;

        [Space] [SerializeField] private UnityEvent saveError;

        [SerializeField] private UnityEvent fieldsNotFilled;
        [SerializeField] private UnityEvent saveSuccess;

        private TMP_InputField _currentInputField;
        private int _currentInputFieldIndex;
        private readonly List<InputFieldInfos> _inputFieldsList = new();

        private void Reset()
        {
            _currentInputField = _inputFieldsList[0].inputField;
            _currentInputFieldIndex = 0;
            _inputFieldsList[0].inputField.ActivateInputField();
        }

        private void Start()
        {
            _inputFieldsList.AddRange(editStudentObjectDisplayObject.GetInputFieldsInfos());
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

        public void ActivateContent(StudentCompleteInfoDTO studentData)
        {
            editStudentObjectDisplayObject.DisplayData(studentData);
            content.SetActive(true);

            Reset();
        }

        public void CloseContent()
        {
            content.SetActive(false);
        }

        public void SaveEditedStudentOnAPI()
        {
            if (!editStudentObjectDisplayObject.FieldsFilled())
            {
                fieldsNotFilled?.Invoke();
                return;
            }

            StartCoroutine(SaveEditedStudent());
        }

        private IEnumerator SaveEditedStudent()
        {
            apiCommsController.StartedComms();

            using var webRequest = UnityWebRequest.Post(ApiPaths.EDIT_STUDENT(apiCommsController.UseCloudPath), "POST");

            var bodyRaw =
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(editStudentObjectDisplayObject.GetStudentData()));
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