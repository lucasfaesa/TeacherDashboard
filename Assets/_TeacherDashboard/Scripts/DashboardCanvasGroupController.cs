using UnityEngine;

namespace _TeacherDashboard.Scripts
{
    public class DashboardCanvasGroupController : MonoBehaviour
    {
        [SerializeField] private ApiCommsControllerSO apiCommsController;
        [SerializeField] private CanvasGroup canvasGroup;

        private void OnEnable()
        {
            apiCommsController.startedComms += SetNotInteractable;
            apiCommsController.endedComms += SetInteractable;
        }

        private void OnDisable()
        {
            apiCommsController.startedComms -= SetNotInteractable;
            apiCommsController.endedComms -= SetInteractable;
        }

        private void SetInteractable()
        {
            canvasGroup.interactable = true;
        }

        private void SetNotInteractable()
        {
            canvasGroup.interactable = false;
        }
    }
}