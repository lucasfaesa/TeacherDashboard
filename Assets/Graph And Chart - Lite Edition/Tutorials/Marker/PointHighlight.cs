using System.Collections;
using System.Collections.Generic;
using ChartAndGraph;
using UnityEngine;
using UnityEngine.UI;

public class PointHighlight : MonoBehaviour
{
    public Text TextPrefab;
    public ChartItemEffect PointHoverPrefab;

    public int FontSize = 5;
    public Vector3 TextOffset;
    private int fractionDigits;
    private GraphChart mChart;
    private readonly List<HighLight> mItems = new();
    private readonly List<HighLight> mRemoved = new();

    private void Start()
    {
        var labels = GetComponent<ItemLabels>();
        if (labels != null)
            fractionDigits = labels.FractionDigits;
        else
            fractionDigits = 2;

        var graph = GetComponent<GraphChart>();
        if (graph != null)
            mChart = graph;
    }


    private void Update()
    {
        mRemoved.RemoveAll(x =>
        {
            if (!x.mControl.enabled)
            {
                ChartCommon.SafeDestroy(x.mText.gameObject);
                ChartCommon.SafeDestroy(x.mPoint.gameObject);
                return true;
            }

            return false;
        });
    }

    private IEnumerator SelectText(Text text, GameObject point)
    {
        yield return new WaitForEndOfFrame();
        if (text != null)
        {
            var e = text.GetComponent<ChartItemEvents>();
            if (e != null) e.OnMouseHover.Invoke(e.gameObject);
            e = point.GetComponent<ChartItemEvents>();
            if (e != null) e.OnMouseHover.Invoke(e.gameObject);
        }
    }

    private void ClearHighLight()
    {
        for (var i = 0; i < mItems.Count; i++) RemoveText(mItems[i]);
        mItems.Clear();
    }

    private void RemoveText(HighLight h)
    {
        if (h.mText != null)
        {
            var e = h.mText.GetComponent<ChartItemEvents>();
            h.mControl = h.mText.GetComponent<CharItemEffectController>();
            if (e != null && h.mControl != null)
            {
                e.OnMouseLeave.Invoke(e.gameObject);
                mRemoved.Add(h);
            }
            else
            {
                ChartCommon.SafeDestroy(h.mText);
            }

            e = h.mPoint.GetComponent<ChartItemEvents>();
            if (e != null)
                e.OnMouseLeave.Invoke(e.gameObject);
            else
                ChartCommon.SafeDestroy(h.mPoint);
        }
    }

    private void PopText(string data, Vector3 position, bool worldPositionStays)
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null || TextPrefab == null)
            return;
        ClearHighLight();
        var highlight = Instantiate(PointHoverPrefab.gameObject, position, Quaternion.identity);
        var obj = Instantiate(TextPrefab.gameObject, position + TextOffset, Quaternion.identity);

        var text = obj.GetComponent<Text>();
        text.maskable = false;
        text.text = data;
        text.fontSize = FontSize;
        obj.transform.SetParent(transform, false);
        highlight.transform.SetParent(transform, false);
        if (worldPositionStays)
        {
            obj.transform.position = position + TextOffset;
            highlight.transform.position = position;
        }
        else
        {
            obj.transform.localPosition = position + TextOffset;
            highlight.transform.localPosition = position;
        }

        var local = obj.transform.localPosition;
        local.z = 0f;
        obj.transform.localPosition = local;

        local = highlight.transform.localPosition;
        local.z = 0f;
        highlight.transform.localPosition = local;
        mItems.Add(new HighLight(text, highlight.GetComponent<ChartItemEffect>()));
        StartCoroutine(SelectText(text, highlight));
    }

    public void HighlightPoint(string category, int index)
    {
        var point = mChart.DataSource.GetPoint(category, index);
        var text = mChart.FormatItem(point.x, point.y);

        Vector3 position;
        if (mChart.PointToWorldSpace(out position, point.x, point.y, category))
            PopText(text, position, true);
    }

    private class HighLight
    {
        public CharItemEffectController mControl;
        public readonly ChartItemEffect mPoint;
        public readonly Text mText;

        public HighLight(Text t, ChartItemEffect p)
        {
            mText = t;
            mPoint = p;
        }
    }
}