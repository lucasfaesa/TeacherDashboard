using System;
using ChartAndGraph;
using UnityEngine;

/// <summary>
///     repsents a chart item text that is billboarded in a unity scene
/// </summary>
public class BillboardText : MonoBehaviour
{
    public TextDirection Direction;
    public RectTransform RectTransformOverride;
    public bool parentSet;
    public RectTransform parent;
    public bool Recycled;
    public bool YMirror;
    private RectTransform mRect;
    private CanvasRenderer[] mRenderers;

    [NonSerialized] public float Scale = 1f;

    public GameObject UIText { get; set; }
    public object UserData { get; set; }

    public RectTransform Rect
    {
        get
        {
            if (UIText == null)
                return null;
            if (RectTransformOverride != null)
                return RectTransformOverride;
            if (mRect == null)
                mRect = UIText.GetComponent<RectTransform>();
            return mRect;
        }
    }

    public void SetVisible(bool visible)
    {
        var cull = !visible;
        var t = Rect;
        if (t == null)
            return;
        if (mRenderers == null)
            mRenderers = t.GetComponentsInChildren<CanvasRenderer>();
        for (var i = 0; i < mRenderers.Length; i++)
            mRenderers[i].cull = cull;
    }
}