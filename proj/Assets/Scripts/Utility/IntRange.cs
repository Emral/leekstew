using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntRange
{
    [SerializeField]
    private int _maxValue;
    [SerializeField]
    private int _minValue;

    [SerializeField]
    private int _totalMaxValue;
    [SerializeField]
    private int _totalMinValue;


    public IntRange(int totalMin, int totalMax, int min, int max)
    {
        Init(totalMin, totalMax, min, max);
    }
    public IntRange(int totalMin, int totalMax)
    {
        Init(totalMin, totalMax, totalMin, totalMax);
    }
    public IntRange()
    {
        Init(0,10,0,10);
    }

    private void Init(int totalMin, int totalMax, int min, int max)
    {
        _totalMinValue = totalMin;
        _totalMaxValue = totalMax;
        _minValue = min;
        _maxValue = max;
    }

    public int MinValue
    {
        get
        { return _minValue; }
        set
        { _minValue = Mathf.Clamp(value, _totalMinValue, _maxValue); }
    }
    public int MaxValue
    {
        get
        { return _maxValue; }
        set
        { _maxValue = Mathf.Clamp(value, _minValue, _totalMaxValue); }
    }

    public int TotalMinValue
    {
        get
        { return _totalMinValue; }
        set
        {
            _totalMinValue = value;
            //_totalMaxValue = Mathf.Max(_totalMaxValue, value);

            //_maxValue = Mathf.Clamp(_maxValue, _totalMinValue, _totalMaxValue);
            //_minValue = Mathf.Clamp(_minValue, _totalMinValue, _maxValue);
        }
    }
    public int TotalMaxValue
    {
        get
        { return _totalMaxValue; }
        set
        {
            _totalMaxValue = value;
            //_totalMinValue = Mathf.Min(_totalMinValue, value);

            //_minValue = Mathf.Clamp(_maxValue, _totalMinValue, _totalMaxValue);
            //_maxValue = Mathf.Clamp(_minValue, _minValue, _totalMaxValue);
        }
    }

    public bool WithinRange(int value)
    {
        return (value >= _minValue && value <= _maxValue);
    }
}