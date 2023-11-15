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

public class AddGroupClassWindowController : MonoBehaviour
{
    [SerializeField] private ApiCommsControllerSO apiCommsController;

    [Space] [SerializeField] private GameObject content;

    [Space] [SerializeField] private NewGroupClassObjectDisplay newGroupClassObjectPrefab;

    [SerializeField] private Transform instantiationContent;
    [SerializeField] private List<NewGroupClassObjectDisplay> newGroupClassObjectPool;

    [Space] [SerializeField] private GameObject addNewGroupClassButtonContent;

    [Space] [SerializeField] private UnityEvent fieldsError;

    [SerializeField] private UnityEvent saveError;
    [SerializeField] private UnityEvent success;

    private TMP_InputField _currentInputField;
    private int _currentInputFieldIndex;
    private readonly List<InputFieldInfos> _inputFieldsList = new();


    private List<NewGroupClassObjectDisplay> _newGroupClassObjects;

    private void Reset()
    {
        _newGroupClassObjects = new List<NewGroupClassObjectDisplay>();

        foreach (var newGroupClassObject in newGroupClassObjectPool) newGroupClassObject.Close();

        newGroupClassObjectPool[0].Activate();
        _inputFieldsList.AddRange(newGroupClassObjectPool[0].GetInputFieldsInfos());
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

    public void AddNewGroupClassToScrollView()
    {
        foreach (var groupClassObj in newGroupClassObjectPool)
        {
            if (groupClassObj.isActiveAndEnabled) continue;

            groupClassObj.Activate();
            addNewGroupClassButtonContent.transform.SetAsLastSibling();
            _inputFieldsList.AddRange(groupClassObj.GetInputFieldsInfos());
            return;
        }

        var obj = Instantiate(newGroupClassObjectPrefab, instantiationContent);
        newGroupClassObjectPool.Add(obj);
        obj.AddGroupClassWindowController = this;
        obj.Activate();
        addNewGroupClassButtonContent.transform.SetAsLastSibling();

        _inputFieldsList.AddRange(obj.GetInputFieldsInfos());
    }

    public void AddToList(NewGroupClassObjectDisplay script)
    {
        _newGroupClassObjects.Add(script);
    }

    public void RemoveOfList(NewGroupClassObjectDisplay script)
    {
        if (_newGroupClassObjects.Count == 0) return;

        if (_newGroupClassObjects.Find(x => x == script))
        {
            _newGroupClassObjects.Remove(script);

            foreach (var inputField in script.GetInputFieldsStandaloneInfos())
                _inputFieldsList.Remove(inputField.InputFieldInfos);
            Debug.Log(_inputFieldsList.Count);
        }
    }

    public void SaveGroupClassesOnAPI()
    {
        foreach (var newGroupClassObjectDisplay in _newGroupClassObjects)
            if (!newGroupClassObjectDisplay.FieldsFilled())
            {
                fieldsError?.Invoke();
                return;
            }

        StartCoroutine(SaveStudents());
    }

    private IEnumerator SaveStudents()
    {
        if (_newGroupClassObjects.Count == 0) yield break;

        apiCommsController.StartedComms();
        List<GroupClassDTO> groupClasses = new();

        foreach (var groupClassObj in _newGroupClassObjects) groupClasses.Add(groupClassObj.GetStudentData());

        using var webRequest =
            UnityWebRequest.Post(ApiPaths.BATCH_POST_GROUPCLASS_URL(apiCommsController.UseCloudPath), "POST");

        var bodyRaw = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(groupClasses));
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
            success?.Invoke();
            apiCommsController.UpdateTeacherData();
            Reset();
        }

        apiCommsController.EndedComms();
        webRequest.Dispose();
    }
}