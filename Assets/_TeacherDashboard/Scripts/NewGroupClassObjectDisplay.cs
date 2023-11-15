using System.Collections.Generic;
using API_Mestrado_Lucas;
using UnityEngine;

namespace _TeacherDashboard.Scripts
{
    public class NewGroupClassObjectDisplay : MonoBehaviour
    {
        [SerializeField] private TeacherDataSO teacherData;

        [Space] [SerializeField] private InputFieldStandaloneInfo nameInputField;

        private List<InputFieldStandaloneInfo> _inputFieldInfosList = new();
        [field: SerializeField] public AddGroupClassWindowController AddGroupClassWindowController { get; set; }

        private void Reset()
        {
            if (_inputFieldInfosList.Count == 0) return;

            nameInputField.InputFieldInfos.inputField.text = "";
        }

        public List<InputFieldStandaloneInfo> GetInputFieldsStandaloneInfos()
        {
            return _inputFieldInfosList = new List<InputFieldStandaloneInfo>
            {
                nameInputField
            };
        }

        public List<InputFieldInfos> GetInputFieldsInfos()
        {
            return new List<InputFieldInfos>
            {
                nameInputField.InputFieldInfos
            };
        }

        public void InputFieldSelected(InputFieldInfos inputFieldInfos)
        {
            var selectedInputField = GetInputFieldsStandaloneInfos()
                .Find(x => x.InputFieldInfos.inputFieldHash == inputFieldInfos.inputFieldHash);
            AddGroupClassWindowController.SetCurrentInputField(selectedInputField.InputFieldInfos);
        }

        public GroupClassDTO GetStudentData()
        {
            return new GroupClassDTO
            {
                Name = nameInputField.InputFieldInfos.inputField.text,
                TeacherId = teacherData.TeacherDto.Id
            };
        }

        public void Activate()
        {
            if (teacherData.TeacherDto.GroupClasses == null) return;

            AddGroupClassWindowController.AddToList(this);
            gameObject.SetActive(true);
        }

        public void Close()
        {
            AddGroupClassWindowController.RemoveOfList(this);
            Reset();
            gameObject.SetActive(false);
        }

        public bool FieldsFilled()
        {
            return !string.IsNullOrEmpty(nameInputField.InputFieldInfos.inputField.text);
        }
    }
}