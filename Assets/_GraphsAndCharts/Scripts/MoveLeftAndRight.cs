using DG.Tweening;
using UnityEngine;

namespace _GraphsAndCharts.Scripts
{
    public class MoveLeftAndRight : MonoBehaviour
    {
        [SerializeField] private Transform targetObject;
        [SerializeField] private float targetPos;
        [SerializeField] private float time;

        private float initialTime;
        
        void Start()
        {
            initialTime = Time.time;
            targetObject.DOLocalMoveX(targetPos, time).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        public void Deactivate()
        {
            if(Time.time - initialTime > 1.2f)
                targetObject.gameObject.SetActive(false);
        }
    }
}
