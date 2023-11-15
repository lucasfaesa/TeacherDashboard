using System;
using System.Collections.Generic;
using ChartAndGraph;
using UnityEngine;

public class pieLengthModifier : MonoBehaviour
{
    private static readonly Dictionary<string, float> LengthDictionary = new();

    static pieLengthModifier()
    {
        LengthDictionary["Category 1"] = 50f;
        LengthDictionary["Category 2"] = 100f;
        LengthDictionary["Category 3"] = 150f;
    }

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        var pie = GetComponent<PieInfo>();
        try
        {
            pie.pieObject.ItemLabel.Direction.Length = LengthDictionary[pie.Category];
        }
        catch (Exception)
        {
        }
    }
}