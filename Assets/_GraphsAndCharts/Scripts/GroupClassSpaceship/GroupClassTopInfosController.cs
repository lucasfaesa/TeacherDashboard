using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.GroupClassSpaceship
{
    public class GroupClassTopInfosController : MonoBehaviour
    {
        [SerializeField] private GroupClassSpaceshipDataSO groupClassSpaceshipData;

        [SerializeField] private TextMeshProUGUI mostErrorsText;
        [SerializeField] private TextMeshProUGUI leastErrorsText;

        private void OnEnable()
        {
            mostErrorsText.text = "";
            leastErrorsText.text = "";
            groupClassSpaceshipData.allQuizesOfTeacherGot += FillTexts;
        }

        private void OnDisable()
        {
            groupClassSpaceshipData.allQuizesOfTeacherGot -= FillTexts;
        }

        private void FillTexts()
        {
            if (groupClassSpaceshipData.AllStudentWrongAnswersOfQuiz.Count > 0)
            {
                var groups = groupClassSpaceshipData.AllStudentWrongAnswersOfQuiz.GroupBy(x => x.QuestionTitle)
                    .OrderByDescending(x => x.Count());
                mostErrorsText.text = groups.First().Key;
                leastErrorsText.text = groups.Last().Key;    
            }
            else
            {
                mostErrorsText.text = "Sem dados suficientes";
                leastErrorsText.text = "Sem dados suficientes";
            }
        }
    }
}
