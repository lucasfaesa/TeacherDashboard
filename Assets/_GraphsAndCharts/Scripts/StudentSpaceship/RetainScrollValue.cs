using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GraphsAndCharts.Scripts.StudentSpaceship
{
    public class RetainScrollValue : MonoBehaviour
    {
        public TMP_Dropdown dropdown;

        private float scrollPosition = 1f;
        private Scrollbar currentScrollbar;
        private void Start()
        {
            dropdown.onValueChanged.AddListener(OnDropdownClicked);
        }

        private void OnDropdownClicked(int id)
        {
            if(currentScrollbar != null)
                scrollPosition = currentScrollbar.value;
        }

        public void DropdownClicked()
        {
            currentScrollbar = GetComponentInChildren<Scrollbar>(false);
            currentScrollbar.value = scrollPosition;
        }
    }
}
