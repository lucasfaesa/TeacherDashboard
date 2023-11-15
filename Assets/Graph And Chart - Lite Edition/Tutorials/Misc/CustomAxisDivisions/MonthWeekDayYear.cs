using System;
using ChartAndGraph;
using UnityEngine;

public class MonthWeekDayYear : MonoBehaviour
{
    public GapEnum Gap;
    private GapEnum? mCurrent;
    private double mEndPostion;
    private double mStartPosition;

    private void Start()
    {
    }

    private void Update()
    {
        CheckGenerate();
    }

    private void OnValidate()
    {
    }

    private void SetConstantGap(int days)
    {
        var axis = GetComponent<HorizontalAxis>();
        if (axis == null)
            return;
        if (axis.SubDivisions.Total != 0)
            axis.SubDivisions.Total = 0;
        if (axis.MainDivisions.Messure != ChartDivisionInfo.DivisionMessure.DataUnits)
            axis.MainDivisions.Messure = ChartDivisionInfo.DivisionMessure.DataUnits;
        var val = (float)(days * TimeSpan.FromDays(1).TotalSeconds);
        if (axis.MainDivisions.UnitsPerDivision != val)
            axis.MainDivisions.UnitsPerDivision = val;
    }

    private void FixDivisions()
    {
    }

    private void Regenrate()
    {
        var chart = GetComponent<ScrollableAxisChart>();
        var axis = GetComponent<HorizontalAxis>();
        if (chart == null || axis == null)
            return;
        if (axis.SubDivisions.Total != 0)
            axis.SubDivisions.Total = 0;
        if (axis.MainDivisions.Messure != ChartDivisionInfo.DivisionMessure.TotalDivisions)
            axis.MainDivisions.Messure = ChartDivisionInfo.DivisionMessure.TotalDivisions;
        if (axis.MainDivisions.Total != 1)
            axis.MainDivisions.Total = 1;
        chart.ScrollableData.RestoreDataValues(0);
        var startPosition = chart.ScrollableData.HorizontalViewOrigin + chart.HorizontalScrolling;
        var endPosition = chart.ScrollableData.HorizontalViewSize + startPosition;

        if (endPosition < startPosition)
        {
            var tmp = startPosition;
            startPosition = endPosition;
            endPosition = tmp;
        }

        var half = Math.Abs(chart.ScrollableData.HorizontalViewSize * 0.5f);
        mStartPosition = startPosition - half;
        mEndPostion = endPosition + half;
        if (Gap == GapEnum.Month)
            RegenrateMonth();
        else if (Gap == GapEnum.Year)
            RegenarateYear();
    }

    private void RegenarateYear()
    {
        var chart = GetComponent<ScrollableAxisChart>();
        if (chart == null)
            return;
        chart.ClearHorizontalCustomDivisions();
        var startDate = ChartDateUtility.ValueToDate(mStartPosition);
        var endDate = ChartDateUtility.ValueToDate(mEndPostion);
        var origin = ChartDateUtility.ValueToDate(chart.ScrollableData.HorizontalViewOrigin);
        var yearGap = startDate.Year - origin.Year;
        var current = origin.AddYears(yearGap);
        while (current < endDate)
        {
            chart.AddHorizontalAxisDivision(ChartDateUtility.DateToValue(current));
            yearGap++;
            current = origin.AddYears(yearGap);
        }
    }

    private void RegenrateMonth()
    {
        var chart = GetComponent<ScrollableAxisChart>();
        if (chart == null)
            return;

        chart.ClearHorizontalCustomDivisions();
        var startDate = ChartDateUtility.ValueToDate(mStartPosition);
        var endDate = ChartDateUtility.ValueToDate(mEndPostion);
        var origin = ChartDateUtility.ValueToDate(chart.ScrollableData.HorizontalViewOrigin);
        var yearGap = startDate.Year - origin.Year;
        var monthGap = startDate.AddYears(yearGap).Month - origin.Month;
        var current = origin.AddYears(yearGap).AddMonths(monthGap);
        while (current < endDate)
        {
            chart.AddHorizontalAxisDivision(ChartDateUtility.DateToValue(current));
            monthGap++;
            current = origin.AddYears(yearGap).AddMonths(monthGap);
        }
    }

    private bool IsViewInside()
    {
        var chart = GetComponent<ScrollableAxisChart>();
        if (chart == null)
            return false;
        var startPosition = chart.ScrollableData.HorizontalViewOrigin + chart.HorizontalScrolling;
        var endPosition = chart.ScrollableData.HorizontalViewSize + startPosition;

        if (endPosition < startPosition)
        {
            var tmp = startPosition;
            startPosition = endPosition;
            endPosition = tmp;
        }

        if (startPosition < mStartPosition)
            return false;
        if (endPosition > mEndPostion)
            return false;
        return true;
    }

    private void CheckGenerate()
    {
        switch (Gap)
        {
            case GapEnum.Day:
                SetConstantGap(1);
                break;
            case GapEnum.Week:
                SetConstantGap(7);
                break;
            case GapEnum.Month:
                if (IsViewInside() == false || mCurrent.HasValue == false || mCurrent.Value != GapEnum.Month)
                    Regenrate();
                break;
            case GapEnum.Year:
                if (IsViewInside() == false || mCurrent.HasValue == false || mCurrent.Value != GapEnum.Year)
                    Regenrate();
                break;
        }

        mCurrent = Gap;
    }
}