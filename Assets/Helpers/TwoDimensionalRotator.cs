using DG.Tweening;
using UnityEngine;

namespace Helpers.Tools
{
    public class TwoDimensionalRotator : MonoBehaviour
    {
        [SerializeField] private Transform objectToRotate;
        [SerializeField] private float duration = 2f;
        [SerializeField] private bool rotateRight = true;
        [SerializeField] private Ease easeType = Ease.InOutSine;
        private void Start()
        {
            objectToRotate.DOLocalRotate(new Vector3(0, 0, rotateRight ? -360 : 360), duration, RotateMode.LocalAxisAdd)
                .SetEase(easeType).SetLoops(-1, LoopType.Restart);
        }

    }
}
