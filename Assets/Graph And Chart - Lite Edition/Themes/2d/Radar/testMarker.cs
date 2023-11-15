using ChartAndGraph;
using UnityEngine;

public class testMarker : MonoBehaviour
{
    public GameObject Place;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        var radar = GetComponent<RadarChart>();
        string group;
        double amount;
        if (radar.SnapWorldPointToPosition(Input.mousePosition, out group, out amount))
        {
            Vector3 position;
            if (radar.ItemToWorldPosition(group, amount, out position)) Place.transform.position = position;
        }
    }
}