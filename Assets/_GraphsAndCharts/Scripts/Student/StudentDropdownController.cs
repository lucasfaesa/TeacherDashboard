using System.Collections.Generic;
using System.Linq;
using API_Mestrado_Lucas;
using TMPro;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.Student
{
    public class StudentDropdownController : MonoBehaviour
    {
        [SerializeField] private StudentsSessionsDataSO studentsSessionsData;

        [Space] [SerializeField] private TMP_Dropdown dropdown;

        private List<StudentDropdownData> _studentDropdownData = new();

        private void OnEnable()
        {
            studentsSessionsData.GotSessionsAndLevels += FillDropdown;
        }

        private void OnDisable()
        {
            studentsSessionsData.GotSessionsAndLevels -= FillDropdown;
        }

        private void FillDropdown()
        {
            if (studentsSessionsData.CompleteStudentsSession.Count == 0)
            {
                dropdown.options.Clear();
                dropdown.captionText.text = "";
                studentsSessionsData.InsufficientData();

                return;
            }

            List<StudentDTO> students = new();

            foreach (var studentAndCompleteLevelsSessionsDto in studentsSessionsData.CompleteStudentsSession)
                students.Add(studentAndCompleteLevelsSessionsDto.StudentDto);

            students = students.OrderBy(x => x.Id).ToList();

            dropdown.options.Clear();
            _studentDropdownData = new List<StudentDropdownData>();
            var count = 0;

            foreach (var student in students)
            {
                _studentDropdownData.Add(new StudentDropdownData(count, student));
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
            studentsSessionsData.StudentSelected(_studentDropdownData.Find(x => x.DropdownIndex == index).StudentDto);
        }
    }

    public class StudentDropdownData
    {
        public int DropdownIndex;
        public StudentDTO StudentDto;

        public StudentDropdownData(int index, StudentDTO student)
        {
            DropdownIndex = index;
            StudentDto = student;
        }
    }
}