using TMPro;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.Student
{
    public class LevelNameAndNumberDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI number;
        [SerializeField] private TextMeshProUGUI levelName;

        public void SetTexts(string numb, string name)
        {
            number.text = numb;
            levelName.text = name;
        }
    }
}