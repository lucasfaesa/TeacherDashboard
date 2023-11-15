using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GraphsAndCharts.Scripts
{
    public class GraphTextWorkaround : MonoBehaviour
    {
        [SerializeField] private Text parentTextComponent;
        [SerializeField] private TextMeshProUGUI thisTextComponent;
        [SerializeField] private bool copyParentFontSize = true;

        private void Start()
        {
            thisTextComponent.text = parentTextComponent.text;
        }

        private void FixedUpdate()
        {
            if (copyParentFontSize)
                thisTextComponent.fontSize = parentTextComponent.fontSize;
        }
    }
}