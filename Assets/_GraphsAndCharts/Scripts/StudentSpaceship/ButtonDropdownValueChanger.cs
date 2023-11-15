using TMPro;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.StudentSpaceship
{
    public class ButtonDropdownValueChanger : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown targetDropdown;

        public void GoToNextOption()
        {
            int currentIndex = targetDropdown.value;
            int nextIndex = (currentIndex + 1) % targetDropdown.options.Count;
            targetDropdown.value = nextIndex;
        }

        public void GoToPreviousOption()
        {
            int currentIndex = targetDropdown.value;
            int previousIndex = (currentIndex - 1 + targetDropdown.options.Count) % targetDropdown.options.Count;
            targetDropdown.value = previousIndex;
        }
    }
}
