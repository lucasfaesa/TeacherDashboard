using UnityEngine;

namespace _GraphsAndCharts.Scripts.StudentSpaceship
{
    public class ContentChanger : MonoBehaviour
    {
        [SerializeField] private GameObject listContent;
        [SerializeField] private GameObject graphContent;
        [Space] 
        [SerializeField] private GameObject listIcon;
        [SerializeField] private GameObject graphIcon;
        
        private bool listActive = true;

        public void Toggle()
        {
            listActive = !listActive;

            listContent.SetActive(listActive);
            listIcon.SetActive(listActive);
            graphContent.SetActive(!listActive);
            graphIcon.SetActive(!listActive);
        }
    }
}
