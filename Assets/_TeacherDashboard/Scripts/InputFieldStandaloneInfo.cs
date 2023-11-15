using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _TeacherDashboard.Scripts
{
    public class InputFieldStandaloneInfo : MonoBehaviour
    {
        [Header("Choose betwen one")] [SerializeField]
        private NewStudentObjectDisplay newStudentObjectDisplay;

        [SerializeField] private NewGroupClassObjectDisplay newGroupClassObjectDisplay;
        [SerializeField] private QuestionObjectDisplay questionObjectDisplay;
        [SerializeField] private EditStudentObjectDisplay editStudentObjectDisplay;

        [Space] [SerializeField] private TMP_InputField inputFieldComponent;

        private InputFieldInfos _inputFieldInfos;

        public InputFieldInfos InputFieldInfos
        {
            get
            {
                if (_inputFieldInfos.inputField == null)
                {
                    var hash = new Hash128();
                    hash.Append(Random.Range(0.1f, 10000f));

                    _inputFieldInfos = new InputFieldInfos
                    {
                        inputFieldHash = hash,
                        inputField = inputFieldComponent
                    };
                }

                return _inputFieldInfos;
            }
            set => _inputFieldInfos = value;
        }


        public void OnInputFieldSelect()
        {
            if (newStudentObjectDisplay != null)
                newStudentObjectDisplay.InputFieldSelected(InputFieldInfos);
            if (newGroupClassObjectDisplay != null)
                newGroupClassObjectDisplay.InputFieldSelected(InputFieldInfos);
            if (questionObjectDisplay != null)
                questionObjectDisplay.InputFieldSelected(InputFieldInfos);
            if (editStudentObjectDisplay != null)
                editStudentObjectDisplay.InputFieldSelected(InputFieldInfos);
        }
    }

    [Serializable]
    public struct InputFieldInfos
    {
        public Hash128 inputFieldHash;
        public TMP_InputField inputField;
    }
}