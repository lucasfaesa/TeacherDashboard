using DG.Tweening;
using UnityEngine;

namespace _GraphsAndCharts.Scripts
{
    public class SideMenuAnimation : MonoBehaviour
    {
        [SerializeField] private Transform allContent;
        [SerializeField] private CanvasGroup canvasGroup;
        [Space]
        [SerializeField] private float shownPosX;
        [SerializeField] private float hiddenPosX;

        
        public void ShowSideMenu()
        {
            allContent.transform.localPosition = new Vector3(hiddenPosX, 0f);
            canvasGroup.alpha = 0f;
            allContent.gameObject.SetActive(true);
            
            Sequence showSequence = DOTween.Sequence();
                
            showSequence.Append(allContent.DOLocalMoveX(shownPosX, 0.2f).SetEase(Ease.InOutSine));
            showSequence.Insert(0, canvasGroup.DOFade(1f, 0.15f).SetEase(Ease.Linear));
        }

        public void HideSideMenu()
        {
            allContent.transform.localPosition = new Vector3(shownPosX, 0f);
            canvasGroup.alpha = 1f;
            
            Sequence hideSequence = DOTween.Sequence();
            
            hideSequence.Append(allContent.DOLocalMoveX(hiddenPosX, 0.2f).SetEase(Ease.InOutSine));
            hideSequence.Insert(0, canvasGroup.DOFade(0f, 0.15f).SetEase(Ease.Linear));

            hideSequence.OnComplete(() =>
            {
                allContent.gameObject.SetActive(false);
            });
            

        }
    }
}
