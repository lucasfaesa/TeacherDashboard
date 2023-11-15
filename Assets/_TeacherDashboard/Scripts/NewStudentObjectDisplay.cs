using System.Collections.Generic;
using System.Linq;
using API_Mestrado_Lucas;
using TMPro;
using UnityEngine;

namespace _TeacherDashboard.Scripts
{
    public class NewStudentObjectDisplay : MonoBehaviour
    {
        [SerializeField] private TeacherDataSO teacherData;

        [Space] [SerializeField] private InputFieldStandaloneInfo nameStudentInputField;

        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private InputFieldStandaloneInfo usernameStudentInputField;
        [SerializeField] private InputFieldStandaloneInfo passwordStudentInputField;
        private List<DropdownAndGroupClass> _dropdownAndGroupClasses = new();

        private List<InputFieldStandaloneInfo> _inputFieldInfosList = new();
        [field: SerializeField] public AddStudentWindowController AddStudentWindowController { get; set; }

        private void Reset()
        {
            if (_inputFieldInfosList.Count == 0) return;

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
            AddStudentWindowController.SetCurrentInputField(selectedInputField.InputFieldInfos);
        }

        public StudentCompleteInfoDTO GetStudentData()
        {
            return new StudentCompleteInfoDTO
            {
                Name = nameStudentInputField.InputFieldInfos.inputField.text,
                GroupClassId = dropdown.options.Count == 0
                    ? null
                    : _dropdownAndGroupClasses.First(x => x.dropdownIndex == dropdown.value)
                        .groupClassIndex, //+1 porque o index começa em zero e no bd começa em 1
                TeacherId = teacherData.TeacherDto.Id,
                Username = usernameStudentInputField.InputFieldInfos.inputField.text,
                Password = passwordStudentInputField.InputFieldInfos.inputField.text
            };
        }

        public void Activate()
        {
            if (teacherData.TeacherDto.GroupClasses == null) return;

            var groupClasses = teacherData.TeacherDto.GroupClasses.OrderBy(x => x.Id);

            var dropdownIndexCount = 0;
            _dropdownAndGroupClasses = new List<DropdownAndGroupClass>();
            foreach (var groupClass in groupClasses)
            {
                _dropdownAndGroupClasses.Add(new DropdownAndGroupClass(dropdownIndexCount, groupClass.Id));
                dropdown.options.Add(new TMP_Dropdown.OptionData { text = groupClass.Name });
                dropdownIndexCount++;
            }

            dropdown.value = -1;

            AddStudentWindowController.AddToList(this);
            gameObject.SetActive(true);
        }

        public void Close()
        {
            AddStudentWindowController.RemoveOfList(this);
            Reset();
            gameObject.SetActive(false);
        }

        public bool FieldsFilled()
        {
            return !string.IsNullOrEmpty(nameStudentInputField.InputFieldInfos.inputField.text)
                   && !string.IsNullOrEmpty(usernameStudentInputField.InputFieldInfos.inputField.text)
                   && !string.IsNullOrEmpty(passwordStudentInputField.InputFieldInfos.inputField.text);
        }
    }
}