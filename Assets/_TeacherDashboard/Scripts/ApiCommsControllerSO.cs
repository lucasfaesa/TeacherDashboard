using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ApiCommsController", menuName = "ScriptableObjects/APIComms/ApiCommsController")]
public class ApiCommsControllerSO : ScriptableObject
{
    [SerializeField] private bool useCloudPath;

    
    public bool UseCloudPath
    {
        get
        {
            #if UNITY_EDITOR
                return useCloudPath;
            #else
                return true;
            #endif
        }
        set => useCloudPath = value;
    }

    public bool UpdateStudentData { get; set; }

    public event Action startedComms;
    public event Action endedComms;

    public event Action commsError;
    public event Action commsSuccess;

    public event Action teacherDataUpdated;
    public event Action updateTeacherData;

    public void TeacherDataUpdated()
    {
        teacherDataUpdated?.Invoke();

        if (teacherDataUpdated == null)
            Debug.Log("nignuem ouviu");
        else
            Debug.Log("Agleum ouviu");
    }

    public void UpdateTeacherData()
    {
        updateTeacherData?.Invoke();
    }

    public void StartedComms()
    {
        startedComms?.Invoke();
    }

    public void EndedComms()
    {
        endedComms?.Invoke();
    }

    public void CommsSuccess()
    {
        commsSuccess?.Invoke();
    }

    public void CommsError()
    {
        commsError?.Invoke();
    }
}