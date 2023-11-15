using System.Collections;
using API_Mestrado_Lucas;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace _TeacherDashboard.Scripts
{
    public class TeacherDataController : MonoBehaviour
    {
        [SerializeField] private ApiCommsControllerSO apiCommsController;
        [SerializeField] private TeacherDataSO teacherData;

        private void Start()
        {
            StartCoroutine(GetTeacherData());
        }

        private void OnEnable()
        {
            apiCommsController.updateTeacherData += UpdateTeacherData;
        }

        private void OnDisable()
        {
            apiCommsController.updateTeacherData -= UpdateTeacherData;
        }

        private IEnumerator GetTeacherData()
        {
            apiCommsController.StartedComms();

            using var webRequest = UnityWebRequest.Get(ApiPaths.GET_TEACHER_INFOS(apiCommsController.UseCloudPath) +
                                                       (teacherData.UseDebug
                                                           ? teacherData.DebugData.teacherId
                                                           : teacherData.TeacherDto.Id));
            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(webRequest.error);
                Debug.LogError("<color=yellow> Teacher get error </color>");
                apiCommsController.CommsError();
            }
            else
            {
                apiCommsController.CommsSuccess();
                teacherData.TeacherDto = JsonConvert.DeserializeObject<TeacherDTO>(webRequest.downloadHandler.text);
                apiCommsController.TeacherDataUpdated();

                Debug.Log("<color=yellow> Teacher get success </color>");
            }

            apiCommsController.EndedComms();
            webRequest.Dispose();
        }

        public void UpdateTeacherData()
        {
            StartCoroutine(GetTeacherData());
        }
    }
}