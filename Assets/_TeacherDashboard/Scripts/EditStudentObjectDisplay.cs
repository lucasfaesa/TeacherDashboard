using System.Collections.Generic;
using System.Linq;
using API_Mestrado_Lucas;
using TMPro;
using UnityEngine;

namespace _TeacherDashboard.Scripts
{
    public class EditStudentObjectDisplay : MonoBehaviour
    {
        [SerializeField] private TeacherDataSO teacherData;

        [Space] [SerializeField] private InputFieldStandaloneInfo nameStudentInputField;

        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private InputFieldStandaloneInfo usernameStudentInputField;
        [SerializeField] private InputFieldStandaloneInfo passwordStudentInputField;

        private List<DropdownAndGroupClass> _dropdownAndGroupClasses = new();

        private List<InputFieldStandaloneInfo> _inputFieldInfosList = new();

        private StudentCompleteInfoDTO _studentData;
        [field: SerializeField] public EditStudentWindowController EditStudentWindowController { get; set; }

        private void Reset()
        {
            if (_inputFieldInfosList.Count == 0) return;

            _dropdownAndGroupClasses = new List<DropdownAndGroupClass>();
            _studentData = null;
            nameStudentInputField.InputFieldInfos.inputField.text = "";
            dropdown.options = new List<TMP_Dropdown.OptionData>();
            usernameStudentInputField.InputFieldInfos.inputField.text = "";
            passwordStudentInputField.InputFieldInfos.inputField.text = "";
        }

        public List<InputFieldStandaloneInfo> GetInputFieldsStandaloneInfos()
        {
            return _inputFieldInfosList = new List<InputFieldStandaloneInfo>
            {
                nameStudentInputField,
                usernameStudentInputField,
                passwordStudentInputField
            };
        }

        public List<InputFieldInfos> GetInputFieldsInfos()
        {
            return new List<InputFieldInfos>
            {
                nameStudentInputField.InputFieldInfos,
                usernameStudentInputField.InputFieldInfos,
                passwordStudentInputField.InputFieldInfos
            };
        }

        public void InputFieldSelected(InputFieldInfos inputFieldInfos)
        {
            var selectedInputField = GetInputFieldsStandaloneInfos()
                .Find(x => x.InputFieldInfos.inputFieldHash == inputFieldInfos.inputFieldHash);
            EditStudentWindowController.SetCurrentInputField(selectedInputField.InputFieldInfos);
        }

        public void DisplayData(StudentCompleteInfoDTO data)
        {
            Reset();

            _studentData = data;

            var groupClasses = teacherData.TeacherDto.GroupClasses.OrderBy(x => x.Id);

            var dropdownIndexCount = 0;

            foreach (var groupClass in groupClasses)
            {
                _dropdownAndGroupClasses.Add(new DropdownAndGroupClass(dropdownIndexCount, groupClass.Id));

                dropdown.options.Add(new TMP_Dropdown.OptionData { text = groupClass.Name });
                dropdownIndexCount++;
            }

            dropdown.value = -1;

            nameStudentInputField.InputFieldInfos.inputField.text = _studentData.Name;

            if (data.GroupClass != null)
                dropdown.value = _dropdownAndGroupClasses.First(x => x.groupClassIndex == data.GroupClassId)
                    .dropdownIndex;
            else
                dropdown.value = 0;

            usernameStudentInputField.InputFieldInfos.inputField.text = _studentData.Username;
        }

        public StudentEditInfoDTO GetStudentData()
        {
            return new StudentEditInfoDTO
            {
                Id = _studentData.Id,
                Name = nameStudentInputField.InputFieldInfos.inputField.text,
                GroupClassId = _dropdownAndGroupClasses.First(x => x.dropdownIndex == dropdown.value).groupClassIndex,
                TeacherId = teacherData.TeacherDto.Id,
                Username = usernameStudentInputField.InputFieldInfos.inputField.text,
                Password = passwordStudentInputField.InputFieldInfos.inputField.text
            };
        }


        public bool FieldsFilled()
        {
            return !string.IsNullOrEmpty(nameStudentInputField.InputFieldInfos.inputField.text)
                   && !string.IsNullOrEmpty(usernameStudentInputField.InputFieldInfos.inputField.text)
                   && !string.IsNullOrEmpty(passwordStudentInputField.InputFieldInfos.inputField.text);
        }
    }

    public class DropdownAndGroupClass
    {
        public int dropdownIndex;
        public int groupClassIndex;

        public DropdownAndGroupClass(int dropIndex, int groupIndex)
        {
            dropdownIndex = dropIndex;
            groupClassIndex = groupIndex;
        }
    }
}