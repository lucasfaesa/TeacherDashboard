using TMPro;
using UnityEngine;

namespace _TeacherDashboard.Scripts
{
    public class NumberInputValidator : MonoBehaviour
    {
        public TMP_InputField inputField;
        private int minValue = 1;
        private int maxValue = 10;

        public void OnValueChanged()
        {
            // Parse the input field's value as an integer
            int value;
            bool isNumeric = int.TryParse(inputField.text, out value);

            // If the value is outside the range, reset it to the closest limit
            if (isNumeric && (value < minValue || value > maxValue))
            {
                value = Mathf.Clamp(value, minValue, maxValue);
                inputField.text = value.ToString();
            }
        }
    }
}