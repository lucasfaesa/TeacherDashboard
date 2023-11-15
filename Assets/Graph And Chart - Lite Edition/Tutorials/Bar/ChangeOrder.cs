using ChartAndGraph;
using UnityEngine;

public class ChangeOrder : MonoBehaviour
{
    public float slideTime = 5f;
    public float switchTime = 7f;
    private float switchTimeCounter = 0.3f;
    private float timeCounter = 1f;

    // Use this for initialization
    private void Start()
    {
        timeCounter = 0f;
        switchTimeCounter = switchTime;
    }

    // Update is called once per frame
    private void Update()
    {
        timeCounter -= Time.deltaTime;
        switchTimeCounter -= Time.deltaTime;
        if (switchTimeCounter < 0f)
        {
            switchTimeCounter = switchTime;
            var bar = GetComponent<BarChart>();
            bar.DataSource.SwitchCategoryPositions("Category 1", "Category 2");
        }

        if (timeCounter < 0f)
        {
            var bar = GetComponent<BarChart>();
            timeCounter = slideTime;
            for (var i = 1; i <= 3; i++)
            {
                var cat = "Category " + i;
                for (var j = 1; j <= 3; j++)
                {
                    var grp = "Group " + j;
                    bar.DataSource.SlideValue(cat, grp, Random.value * 10f, slideTime);
                }
            }
        }
    }
}