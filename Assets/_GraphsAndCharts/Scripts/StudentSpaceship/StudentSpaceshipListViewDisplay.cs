using System;
using TMPro;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.StudentSpaceship
{
    public class StudentSpaceshipListViewDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI questionTitleText;
        [SerializeField] private TextMeshProUGUI percentageTotalText;
        [SerializeField] private TextMeshProUGUI commonWrongResponseText;

        public void SetData(string title, string percentageTotal, string commonResponse)
        {
            questionTitleText.text = title;
            percentageTotalText.text = $"{percentageTotal}%";

            commonWrongResponseText.text = commonResponse;
            
            var rectValues = commonWrongResponseText.rectTransform;
            var sizeDelta = rectValues.sizeDelta;

            commonWrongResponseText.rectTransform.sizeDelta = commonWrongResponseText.text.Length switch
            {
                <= 27 => new Vector2(sizeDelta.x, 35.3f),
                <= 55 => new Vector2(sizeDelta.x, 80.2f),
                _ => new Vector2(sizeDelta.x, 115f),
            };

            percentageTotalText.color = Convert.ToInt32(percentageTotal) switch
            {
                > 50 => Color.red,
                > 31 => Color.yellow,
                _ => Color.green
            };

        }
    }
}
