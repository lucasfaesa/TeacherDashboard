using UnityEngine;
using UnityEngine.UI;

public class AxisCoordinates : MonoBehaviour
{
    public string TextFormat = "{0} : {1}";
    public Text Coordinates;
    public RectTransform Prefab;
    private RectTransform mHorizontal;

    private RectTransform mVertical;

    // Use this for initialization

    private void Start()
    {
        if (Prefab != null)
        {
            mVertical = Instantiate(Prefab);
            mHorizontal = Instantiate(Prefab);
            mVertical.gameObject.SetActive(false);
            mHorizontal.gameObject.SetActive(false);
        }
    }


    private void Update()
    {
    }
}