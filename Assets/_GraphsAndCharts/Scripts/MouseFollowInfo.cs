using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GraphsAndCharts.Scripts
{
    public class MouseFollowInfo : MonoBehaviour
    {
        [SerializeField] private HoverChartsDataSO hoverChartsData;

        [Space] [SerializeField] private GameObject panel;

        [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
        [SerializeField] private TextMeshProUGUI infoText;
        [SerializeField] private TextMeshProUGUI valueText;

        private void Update()
        {
            transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        }

        private void OnEnable()
        {
            hoverChartsData.OnHoverEnter += HoverEntered;
            hoverChartsData.OnHoverExit += HoverExited;
        }

        private void OnDisable()
        {
            hoverChartsData.OnHoverEnter -= HoverEntered;
            hoverChartsData.OnHoverExit -= HoverExited;
        }

        private void HoverEntered(HoverDataDisplay data)
        {
            panel.SetActive(true);
            infoText.gameObject.SetActive(data.info.Length > 0);
            infoText.text = data.info;
            valueText.text = data.value;

            UpdateCanvas();
        }

        private void HoverExited()
        {
            panel.SetActive(false);
        }

        public void UpdateCanvas()
        {
            StartCoroutine(UpdateCanvasRoutine());
        }

        private IEnumerator UpdateCanvasRoutine()
        {
            //Canvas.ForceUpdateCanvases();
            yield return new WaitForEndOfFrame();
            verticalLayoutGroup.enabled = false;

            //Canvas.ForceUpdateCanvases();
            verticalLayoutGroup.enabled = true;
            Canvas.ForceUpdateCanvases();
        }
    }
}