using System;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.GroupClassSpaceship
{
    public class CanvasInteractivityToggler : MonoBehaviour
    {
        [SerializeField] private ApiCommsControllerSO apiCommsController;

        [SerializeField] private CanvasGroup canvasGroup;

        private void OnEnable()
        {
            apiCommsController.startedComms += SetInteractivityOff;
            apiCommsController.endedComms += SetInteractivityOn;
        }

        private void OnDisable()
        {
            apiCommsController.startedComms -= SetInteractivityOff;
            apiCommsController.endedComms -= SetInteractivityOn;
        }

        private void SetInteractivityOff()
        {
            canvasGroup.interactable = false;
        }
        
        private void SetInteractivityOn()
        {
            canvasGroup.interactable = true;
        }
    }
}
