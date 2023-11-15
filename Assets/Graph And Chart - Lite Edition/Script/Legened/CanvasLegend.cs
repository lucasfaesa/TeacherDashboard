using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChartAndGraph.Legened
{
    /// <summary>
    ///     class for canvas legned. this class basiically creates the legned prefab for each category in the chart
    /// </summary>
    [ExecuteInEditMode]
    internal class CanvasLegend : MonoBehaviour
    {
        [SerializeField] private int fontSize;

        [SerializeField] public ImageOverride[] CategoryImages;

        [SerializeField] private CanvasLegendItem legendItemPrefab;

        [SerializeField] private AnyChart chart;

        private bool mGenerateNext;

        private readonly List<Object> mToDispose = new();

        public int FontSize
        {
            get => fontSize;
            set
            {
                fontSize = value;
                PropertyChanged();
            }
        }

        public CanvasLegendItem LegenedItemPrefab
        {
            get => legendItemPrefab;
            set
            {
                legendItemPrefab = value;
                PropertyChanged();
            }
        }

        public AnyChart Chart
        {
            get => chart;
            set
            {
                if (chart != null)
                    ((IInternalUse)chart).Generated -= CanvasLegend_Generated;
                chart = value;
                if (chart != null)
                    ((IInternalUse)chart).Generated += CanvasLegend_Generated;
                PropertyChanged();
            }
        }

        private void Start()
        {
            if (chart != null)
                ((IInternalUse)chart).Generated += CanvasLegend_Generated;
            InnerGenerate();
        }

        private void Update()
        {
            if (mGenerateNext)
                InnerGenerate();
        }

        private void OnEnable()
        {
            if (chart != null)
                ((IInternalUse)chart).Generated += CanvasLegend_Generated;
            InnerGenerate();
        }

        private void OnDisable()
        {
            if (chart != null)
                ((IInternalUse)chart).Generated -= CanvasLegend_Generated;
            //    Clear();
        }

        protected void OnValidate()
        {
            if (chart != null)
                ((IInternalUse)chart).Generated += CanvasLegend_Generated;
            Generate();
        }

        private Dictionary<string, Texture2D> CreateimageDictionary()
        {
            var dic = new Dictionary<string, Texture2D>();
            if (CategoryImages == null)
                return dic;
            foreach (var ent in CategoryImages) dic[ent.category] = ent.Image;
            return dic;
        }

        private void OnDestory()
        {
            if (chart != null)
                ((IInternalUse)chart).Generated -= CanvasLegend_Generated;
            Clear();
        }

        private void CanvasLegend_Generated()
        {
            InnerGenerate();
        }

        protected void PropertyChanged()
        {
            Generate();
        }

        public void Clear()
        {
            var items = gameObject.GetComponentsInChildren<CanvasLegendItem>();
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] == null || items[i].gameObject == null)
                    continue;
                ChartCommon.SafeDestroy(items[i].gameObject);
            }

            for (var i = 0; i < mToDispose.Count; i++)
            {
                var obj = mToDispose[i];
                if (obj != null)
                    ChartCommon.SafeDestroy(obj);
            }

            mToDispose.Clear();
        }

        private bool isGradientShader(Material mat)
        {
            if (mat.HasProperty("_ColorFrom") && mat.HasProperty("_ColorTo"))
                return true;
            return false;
        }

        private Sprite CreateSpriteFromTexture(Texture2D t)
        {
            var sp = Sprite.Create(t, new Rect(0f, 0f, t.width, t.height), new Vector2(0.5f, 0.5f));
            sp.hideFlags = HideFlags.DontSave;
            mToDispose.Add(sp);
            return sp;
        }

        private Material CreateCanvasGradient(Material mat)
        {
            var m = (Material)Resources.Load("Chart And Graph/Legend/CanvasGradient");
            if (m == null)
                return null;
            var grad = new Material(m);
            grad.hideFlags = HideFlags.DontSave;
            var from = mat.GetColor("_ColorFrom");
            var to = mat.GetColor("_ColorTo");
            grad.SetColor("_ColorFrom", from);
            grad.SetColor("_ColorTo", to);
            mToDispose.Add(grad);
            return grad;
        }

        public void Generate()
        {
            mGenerateNext = true;
        }

        private void InnerGenerate()
        {
            if (enabled == false || gameObject.activeInHierarchy == false)
                return;
            mGenerateNext = false;
            Clear();
            if (chart == null || legendItemPrefab == null)
                return;
            var inf = ((IInternalUse)chart).InternalLegendInfo;
            if (inf == null)
                return;
            var dic = CreateimageDictionary();
            foreach (var item in inf.Items)
            {
                var prefab = Instantiate(legendItemPrefab.gameObject);
                prefab.transform.SetParent(transform, false);
                prefab.hideFlags = HideFlags.HideAndDontSave;
                foreach (Transform child in prefab.transform)
                    child.gameObject.hideFlags = HideFlags.HideAndDontSave;

                var legendItemData = prefab.GetComponent<CanvasLegendItem>();
                Texture2D overrideImage = null;

                if (dic.TryGetValue(item.Name, out overrideImage) == false)
                    overrideImage = null;

                if (overrideImage != null)
                {
                    legendItemData.Image.material = null;
                    var tex = overrideImage;
                    legendItemData.Image.sprite = CreateSpriteFromTexture(tex);
                }
                else if (legendItemData.Image != null)
                {
                    if (item.Material == null)
                    {
                        legendItemData.Image.material = null;
                    }
                    else
                    {
                        if (isGradientShader(item.Material))
                        {
                            legendItemData.Image.material = CreateCanvasGradient(item.Material);
                        }
                        else
                        {
                            legendItemData.Image.material = null;
                            var tex = item.Material.mainTexture as Texture2D;
                            if (tex != null)
                                legendItemData.Image.sprite = CreateSpriteFromTexture(tex);
                            legendItemData.Image.color = item.Material.color;
                        }
                    }
                }

                if (legendItemData.Text != null)
                {
                    legendItemData.Text.text = item.Name;
                    legendItemData.Text.fontSize = fontSize;
                }
            }
        }


        [Serializable]
        public class ImageOverride
        {
            public Texture2D Image;
            public string category = "";
        }
    }
}