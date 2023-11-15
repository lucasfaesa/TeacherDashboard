using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class GrowAndShrinkSize : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Transform transform;

    [Space] [SerializeField] private Vector3 growTo;

    private Vector3 _defaultSize;

    private void Start()
    {
        _defaultSize = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Enter");
        transform.localScale = new Vector2(1.2f, 1.2f);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Exit");
        transform.localScale = new Vector2(1f, 1f);
    }

    public void Grow()
    {
        transform.DOScale(growTo, 0.3f).SetEase(Ease.InOutSine);
    }

    public void Shrink()
    {
        transform.DOScale(_defaultSize, 0.3f).SetEase(Ease.InOutSine);
    }
}