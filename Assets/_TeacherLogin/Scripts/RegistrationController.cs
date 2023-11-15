using System.Collections;
using System.Collections.Generic;
using System.Text;
using API_Mestrado_Lucas;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace _TeacherLogin.Scripts
{
    public class RegistrationController : MonoBehaviour
    {
        [Header("SO")] 
        [SerializeField] private ApiCommsControllerSO apiCommsController;
        [Space] 
        [SerializeField] private CanvasGroup canvasGroup;
        [Space]
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private TMP_InputField usernameInputField;
        [SerializeField] private TMP_InputField passwordInputField;

        [SerializeField] private UnityEvent tryingToCreate;
        [SerializeField] private UnityEvent fieldsIncomplete;
        [SerializeField] private UnityEvent creationError;
        [SerializeField] private UnityEvent creationComplete;
        [SerializeField] private UnityEvent delayedClosing;
        
        private int _currentInputField;

        private List<TMP_InputField> _inputFields = new();
        private bool _creating;

        private Coroutine _createRoutine;
        
        public void CreateTeacherAccount()
        {
            string name = nameInputField.text.Trim();
            string username = usernameInputField.text.Trim();
            string password = passwordInputField.text;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                fieldsIncomplete?.Invoke();
                return;
            }

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                fieldsIncomplete?.Invoke();
                return;
            }

            _createRoutine = StartCoroutine(CreateAccountOnDatabase());
        }

        private void OnEnable()
        {
            canvasGroup.interactable = true;
            _currentInputField = 0;
            _inputFields = new();
            nameInputField.ActivateInputField();
            _inputFields.Add(nameInputField);
            _inputFields.Add(usernameInputField);
            _inputFields.Add(passwordInputField);
        }

        private void Update()
        {
            if ((Input.GetKeyDown(KeyCode.Return) && !_creating)
                || (Input.GetKeyDown(KeyCode.KeypadEnter) && !_creating))
            {
                usernameInputField.Select();
                usernameInputField.ActivateInputField();
                CreateTeacherAccount();
            }

            if (Input.GetKeyDown(KeyCode.Tab)) ChangeInputField();
        }

        private void OnApplicationQuit()
        {
            if (_createRoutine != null)
                StopCoroutine(_createRoutine);
        }

        private IEnumerator CreateAccountOnDatabase()
        {
            tryingToCreate?.Invoke();
            _creating = true;

            canvasGroup.interactable = false;
            
            var teacherLoginInfo = new TeacherCompleteInfoDTO()
            {
                Name = nameInputField.text,
                Username = usernameInputField.text,
                Password = passwordInputField.text,
            };

            using (var www = UnityWebRequest.Post(ApiPaths.TEACHER_REGISTRATION(apiCommsController.UseCloudPath), "POST"))
            {
                var bodyRaw = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(teacherLoginInfo));
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                if (www.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
                    or UnityWebRequest.Result.DataProcessingError)
                {
                    Debug.Log(www.error);
                    creationError?.Invoke();
                    usernameInputField.Select();
                    usernameInputField.ActivateInputField();
                    _creating = false;
                    canvasGroup.interactable = true;
                }
                else
                {
                    Debug.Log("Login Success");
                    creationComplete?.Invoke();
                    StartCoroutine(DelayedClosing());
                }

                
                
                www.Dispose();
            }
        }
        
        private IEnumerator DelayedClosing()
        {
            yield return new WaitForSeconds(2.2f);
            delayedClosing?.Invoke();
        }

        private void ChangeInputField()
        {
            _currentInputField++;
            if (_currentInputField > _inputFields.Count) _currentInputField = 0;

            _inputFields[_currentInputField].ActivateInputField();
        }

        public void InputFieldSelected(int index)
        {
            _currentInputField = index;
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
