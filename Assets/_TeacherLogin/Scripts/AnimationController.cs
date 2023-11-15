using DG.Tweening;
using UnityEngine;

namespace _TeacherLogin.Scripts
{
    public class AnimationController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup loginBox;
        [SerializeField] private CanvasGroup sceneChoiceBox;

        public void ShowSceneChoiceBox()
        {
            var hideLoginBox = DOTween.Sequence()
                .Append(loginBox.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack)).Pause();
            var showChoiceBox = DOTween.Sequence()
                .Append(sceneChoiceBox.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack)).Pause();

            loginBox.interactable = false;
            hideLoginBox.Play();
            hideLoginBox.OnComplete(() =>
            {
                loginBox.gameObject.SetActive(false);

                sceneChoiceBox.gameObject.SetActive(true);
                sceneChoiceBox.transform.localScale = Vector3.zero;
                showChoiceBox.Play();
                sceneChoiceBox.interactable = true;
            });
        }

        public void ShowLoginBox()
        {
            var showLoginBox = DOTween.Sequence()
                .Append(loginBox.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack)).Pause();
            ;
            var hideChoiceBox = DOTween.Sequence()
                .Append(sceneChoiceBox.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack));

            sceneChoiceBox.interactable = false;
            hideChoiceBox.Play();
            hideChoiceBox.OnComplete(() =>
            {
                sceneChoiceBox.gameObject.SetActive(false);

                loginBox.gameObject.SetActive(true);
                loginBox.transform.localScale = Vector3.zero;
                showLoginBox.Play();
                loginBox.interactable = true;
            });
        }
    }
}