using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _GraphsAndCharts.Scripts.General;
using _TeacherDashboard.Scripts;
using API_Mestrado_Lucas;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace _GraphsAndCharts.Scripts.GroupClassSpaceship
{
    public class QuizDropdownController : MonoBehaviour
    {
        [SerializeField] private ApiCommsControllerSO apiCommsController;
        [SerializeField] private GroupClassSpaceshipDataSO groupClassSpaceshipData;
        [Space]
        [SerializeField] private TMP_Dropdown dropdown;
        
        private List<QuizDropdownData> _quizDropdownData = new();
        
        private void Awake()
        {
            //StartCoroutine(GetSubjects());
        }

        private IEnumerator Start()
        {
            while (groupClassSpaceshipData.AllQuizesOfTeacher.Count == 0)
                yield return null;
            
            FillDropdown();
        }

        /*private IEnumerator GetSubjects()
        {
            apiCommsController.StartedComms();
            using var webRequest =
                UnityWebRequest.Get(ApiPaths.GET_SUBJECTS(apiCommsController.UseCloudPath));

            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(webRequest.error);
                Debug.LogError("<color=yellow> Subjects Get error </color>");
                apiCommsController.CommsError();
            }
            else
            {
                subjects = JsonConvert.DeserializeObject<List<SubjectDTO>>(webRequest.downloadHandler.text);
                
                apiCommsController.CommsSuccess();
                FillDropdown();
                
                Debug.Log("<color=yellow> Subjects Get success </color>");
            }
        
            apiCommsController.EndedComms();
            webRequest.Dispose();
        }
        */
        private void FillDropdown()
        {
            if (groupClassSpaceshipData.AllQuizesOfTeacher.Count == 0)
            {
                dropdown.options.Clear();
                dropdown.captionText.text = "";
                groupClassSpaceshipData.InsufficientData();

                return;
            }

            var quizes = groupClassSpaceshipData.AllQuizesOfTeacher.OrderBy(x => x.Id).ToList();

            dropdown.options.Clear();
            _quizDropdownData = new List<QuizDropdownData>();
            var count = 0;

            foreach (var quiz in quizes)
            {
                _quizDropdownData.Add(new QuizDropdownData(count, quiz));
                count++;
            }

            foreach (var subject in _quizDropdownData)
                dropdown.options.Add(new TMP_Dropdown.OptionData { text = subject.QuizDto.Name });

            dropdown.value = -1;
            dropdown.value = 0;

            //StudentSelected(0);
        }
        
        public void QuizSelected(int index)
        {
            groupClassSpaceshipData.QuizSelected(_quizDropdownData.Find(x => x.DropdownIndex == index).QuizDto);
        }
    }
    
    
    
    public class QuizDropdownData
    {
        public int DropdownIndex;
        public QuizDTO QuizDto;

        public QuizDropdownData(int index, QuizDTO quiz)
        {
            DropdownIndex = index;
            QuizDto = quiz;
        }
    }
}
