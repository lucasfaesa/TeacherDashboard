using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using _TeacherDashboard.Scripts;
using API_Mestrado_Lucas;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class LoginController : MonoBehaviour
{
    [Header("SO")] [SerializeField] private ApiCommsControllerSO apiCommsController;

    [SerializeField] private TeacherDataSO teacherDataSO;

    [Space] [SerializeField] private TMP_InputField username;

    [SerializeField] private TMP_InputField password;

    [Space] [SerializeField] private UnityEvent usernameOrPasswordEmpty;

    [SerializeField] private UnityEvent tryingToLogIn;
    [SerializeField] private UnityEvent loginError;
    [SerializeField] private UnityEvent loginSuccess;
    private int _currentInputField;

    private List<TMP_InputField> _inputFields = new();
    private bool _loginIn;

    private Coroutine _loginRoutine;

    private void OnEnable()
    {
        #if UNITY_EDITOR
                username.text = "Lucas1234";
                password.text = "Luk123";
        #endif

        _currentInputField = 0;
        _inputFields = new();
        username.ActivateInputField();
        _inputFields.Add(username);
        _inputFields.Add(password);
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Return) && !_loginIn)
            || (Input.GetKeyDown(KeyCode.KeypadEnter) && !_loginIn))
        {
            username.Select();
            username.ActivateInputField();
            Login();
        }

        if (Input.GetKeyDown(KeyCode.Tab)) ChangeInputField();
    }

    private void OnApplicationQuit()
    {
        if (_loginRoutine != null)
            StopCoroutine(_loginRoutine);
    }

    public void Login()
    {
        if (string.IsNullOrEmpty(username.text))
        {
            usernameOrPasswordEmpty?.Invoke();
            return;
        }

        _loginRoutine = StartCoroutine(TeacherLogin());
    }

    private IEnumerator TeacherLogin()
    {
        teacherDataSO.Reset();

        tryingToLogIn?.Invoke();
        _loginIn = true;

        var teacherLoginInfo = new TeacherLoginDTO
        {
            Username = username.text.Trim(),
            Password = password.text
        };

        using (var www = UnityWebRequest.Post(ApiPaths.TEACHER_LOGIN(apiCommsController.UseCloudPath), "POST"))
        {
            var bodyRaw = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(teacherLoginInfo));
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(www.error);
                loginError?.Invoke();
                username.Select();
                username.ActivateInputField();
                _loginIn = false;
            }
            else
            {
                teacherDataSO.TeacherDto = JsonConvert.DeserializeObject<TeacherDTO>(www.downloadHandler.text);

                Debug.Log("Login Success");
                loginSuccess?.Invoke();
            }

            www.Dispose();
        }
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