using API_Mestrado_Lucas;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _TeacherDashboard.Scripts
{
    public class GroupClassListObjectDisplay : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private TextMeshProUGUI nameText;

        private GroupClassTabController _groupClassTabController;

        public GroupClassDTO GroupClassData { get; set; }

        public void SetInfos(GroupClassDTO groupClassDto, GroupClassTabController groupClassTabController)
        {
            toggle.isOn = false;
            nameText.text = groupClassDto.Name;

            GroupClassData = groupClassDto;
            _groupClassTabController = groupClassTabController;
        }

        public void GroupClassToggle(bool status)
        {
            if (status)
                _groupClassTabController.GroupClassSelected(GroupClassData);
            else
                _groupClassTabController.GroupClassDeselected(GroupClassData);
        }

        public void AddLevelsButton()
        {
            _groupClassTabController.ShowAssignLevelsWindow(GroupClassData.Id);
        }
    }
}