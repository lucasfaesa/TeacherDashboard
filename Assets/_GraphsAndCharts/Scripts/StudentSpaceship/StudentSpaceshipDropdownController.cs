using System.Collections.Generic;
using System.Linq;
using _TeacherDashboard.Scripts;
using API_Mestrado_Lucas;
using Mapster;
using TMPro;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.StudentSpaceship
{
    public class StudentSpaceshipDropdownController : MonoBehaviour
    {
        [SerializeField] private TeacherDataSO teacherDto;
        [SerializeField] private StudentSpaceshipDataSO studentSpaceshipData;
        
        [Space] [SerializeField] private TMP_Dropdown dropdown;

        private List<StudentSpaceshipDropdownData> _studentDropdownData = new();

        private void OnEnable()
        {
            teacherDto.CurrentGroupClassChanged += FillDropdown;
        }

        private void OnDisable()
        {
            teacherDto.CurrentGroupClassChanged -= FillDropdown;
        }

        private void FillDropdown()
        {
            if (teacherDto.TeacherDto.Students.Count == 0)
            {
                dropdown.options.Clear();
                dropdown.captionText.text = "";
                studentSpaceshipData.InsufficientData();

                return;
            }

            List<StudentCompleteInfoDTO> students = new (teacherDto.TeacherDto.Students.Where(x=>x.GroupClassId == teacherDto.CurrentlyChosenGroupClassId));

            students = students.OrderBy(x => x.Name).ToList();

            dropdown.options.Clear();
            _studentDropdownData = new List<StudentSpaceshipDropdownData>();
            var count = 0;

            foreach (var student in students)
            {
                _studentDropdownData.Add(new StudentSpaceshipDropdownData(count, student));
                count++;
            }

            foreach (var student in _studentDropdownData)
                dropdown.options.Add(new TMP_Dropdown.OptionData { text = student.StudentDto.Name });

            dropdown.value = -1;
            dropdown.value = 0;

            //StudentSelected(0);
        }

        public void StudentSelected(int index)
        {
            studentSpaceshipData.StudentSelected(_studentDropdownData.Find(x => x.DropdownIndex == index).StudentDto);
        }
    }

    public class StudentSpaceshipDropdownData
    {
        public int DropdownIndex;
        public StudentCompleteInfoDTO StudentDto;

        public StudentSpaceshipDropdownData(int index, StudentCompleteInfoDTO student)
        {
            DropdownIndex = index;
            StudentDto = student;
        }
    }
}

