using System.Collections.Generic;
using System.Linq;
using _TeacherDashboard.Scripts;
using API_Mestrado_Lucas;
using TMPro;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.Student
{
    public class GroupClassDropdownController : MonoBehaviour
    {
        [SerializeField] private TeacherDataSO teacherDataSo;

        [Space] [SerializeField] private TMP_Dropdown dropdown;

        private List<GroupClassDropdownData> _groupClassDropdownData = new();

        private void Start()
        {
            FillDropdown();
        }

        private void FillDropdown()
        {
            if (!teacherDataSo.UseDebug)
                if (teacherDataSo.TeacherDto.GroupClasses.Count == 0)
                    return;

            var groupClasses = teacherDataSo.TeacherDto.GroupClasses.ToList();

            groupClasses = groupClasses.OrderBy(x => x.Id).ToList();

            dropdown.options.Clear();

            _groupClassDropdownData = new List<GroupClassDropdownData>();
            var count = 0;

            foreach (var groupClass in groupClasses)
            {
                _groupClassDropdownData.Add(new GroupClassDropdownData(count, groupClass));
                count++;
            }

            foreach (var groupClass in _groupClassDropdownData)
                dropdown.options.Add(new TMP_Dropdown.OptionData { text = groupClass.GroupClassDto.Name });

            dropdown.value = -1;
            dropdown.value = 0;
        }

        public void GroupClassSelected(int index)
        {
            Debug.Log("Group Class Changed");

            teacherDataSo.ChangeCurrentGroupClass(_groupClassDropdownData.Find(x => x.DropdownIndex == index)
                .GroupClassDto.Id);
        }
    }

    public class GroupClassDropdownData
    {
        public int DropdownIndex;
        public GroupClassDTO GroupClassDto;

        public GroupClassDropdownData(int index, GroupClassDTO groupClass)
        {
            DropdownIndex = index;
            GroupClassDto = groupClass;
        }
    }
}