using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class VoxelInspector : MonoBehaviour
{
    public static VoxelInspector instance;
    public Text scrollViewText;
    public Material standard;
    public Material highlight;
    public Material selected;
    public Slider MinVoxelsSlider;
    public Slider MaxVoxelsSlider;
    public Slider MinPointsSlider;
    public Slider MaxPointsSlider;
    public Slider ResolutionSlider;
    public Slider VisibilitySlider;
    public Dropdown voxelDropdown;
    public Dropdown pointsDropdown;
    public Text voxelMinText;
    public Text voxelMaxText;
    public Text pointsMinText;
    public Text pointsMaxText;
    public Text debugText;

    public enum FilterMode { None, MinSepPaths, MedianPaths, NumIntersectionsPaths, DistanceErrorPoints, TimeErrorPoints, TemperaturePoints }
    [HideInInspector]
    public VoxelInspector.FilterMode voxelFilterMode;
    public VoxelInspector.FilterMode pointsFilterMode;
    public Dictionary<float, Vector3> j;
    public Dictionary <FilterMode, FilterModeClass> modes = new Dictionary<FilterMode, FilterModeClass>();
    public Color baseColor;
    public Color baseColorStandard;

    void Awake()
    {
        instance = this;
        voxelFilterMode = FilterMode.None;
        pointsFilterMode = FilterMode.None;
        ResolutionSlider.value = 10;
        baseColor = Color.gray;
        baseColorStandard = baseColor;
        modes.Add(FilterMode.None, new FilterModeClass());
        modes.Add(FilterMode.MinSepPaths, new FilterModeClass());
        modes.Add(FilterMode.MedianPaths, new FilterModeClass());
        modes.Add(FilterMode.NumIntersectionsPaths, new FilterModeClass());
        modes.Add(FilterMode.DistanceErrorPoints, new FilterModeClass());
        modes.Add(FilterMode.TimeErrorPoints, new FilterModeClass());
        modes.Add(FilterMode.TemperaturePoints, new FilterModeClass());
    }


    public void SetScrollText(string txt)
    {
        scrollViewText.text = txt;
    }

    public void SetFilterMode(string _filterMode)
    {
        FilterModeClass thisMode = modes[FilterMode.None];
        Slider sMin = MinVoxelsSlider;
        Slider sMax = MaxVoxelsSlider;
        Text tMin = MinVoxelsSlider.GetComponentInChildren<Text>();
        Text tMax = MaxVoxelsSlider.GetComponentInChildren<Text>();
        string type = "Voxels";
        switch (_filterMode)
        {
            case "Voxels":
                type = "Voxels";
                switch (voxelDropdown.value)
                {
                    case (1):
                        voxelFilterMode = FilterMode.MinSepPaths;
                        break;
                    case (2):
                        voxelFilterMode = FilterMode.MedianPaths;
                        break;
                    case (3):
                        voxelFilterMode = FilterMode.NumIntersectionsPaths;
                        break;
                    default:
                        voxelFilterMode = FilterMode.None;
                        break;
                }

                thisMode = modes[voxelFilterMode];
                break;
            case "Points":
                type = "Points";
                switch (pointsDropdown.value)
                {
                    case (1):
                        pointsFilterMode = FilterMode.DistanceErrorPoints;
                        break;
                    case (2):
                        pointsFilterMode = FilterMode.TimeErrorPoints;
                        break;
                    case (3):
                        pointsFilterMode = FilterMode.TemperaturePoints;
                        break;
                    default:
                        pointsFilterMode = FilterMode.None;
                        break;
                }
                thisMode = modes[pointsFilterMode];
                break;
            default:
                //voxelFilterMode = FilterMode.None;
                //pointsFilterMode = FilterMode.None;
                break;
        }
        var min = thisMode.Min;
        var max = thisMode.Max;
        var cMin = thisMode.CurrentValueMin;
        var cMax = thisMode.CurrentValueMax;
        switch (type)
        {
            case "Points":
                MinPointsSlider.maxValue = cMax;
                MinPointsSlider.minValue = min;
                MinPointsSlider.value = cMin;
                pointsMinText.text = "Min: " + cMin.ToString();
                MaxPointsSlider.maxValue = max;
                MaxPointsSlider.minValue = min;
                MaxPointsSlider.value = cMax;
                pointsMaxText.text = "Max: " + cMax.ToString();
                SetParams("Points");
                break;
            default:
                MinVoxelsSlider.maxValue = cMax;
                MinVoxelsSlider.minValue = min;
                MinVoxelsSlider.value = cMin;
                voxelMinText.text = "Min: " + cMin.ToString();
                MaxVoxelsSlider.maxValue = max;
                MaxVoxelsSlider.minValue = min;
                MaxVoxelsSlider.value = cMax;
                voxelMaxText.text = "Max: " + cMax.ToString();
                SetParams("Voxels");
                break;
        }
    }

    public void InitializeParams()
    {
        if (Sloxelizer2.instance != null)
        {
            modes[FilterMode.MinSepPaths] = new FilterModeClass(FilterMode.MinSepPaths, 0, Sloxelizer2.instance.HighestSeparation, 1);
            modes[FilterMode.MedianPaths] = new FilterModeClass(FilterMode.MedianPaths, 0, Sloxelizer2.instance.HighestMean, 1);
            modes[FilterMode.NumIntersectionsPaths] = new FilterModeClass(FilterMode.NumIntersectionsPaths, 0, Sloxelizer2.instance.HighestNumIntersects, 1);
        }
        if (ccatInterpreter.instance != null)
        {
            modes[FilterMode.DistanceErrorPoints] = new FilterModeClass(FilterMode.DistanceErrorPoints, 0, ccatInterpreter.instance.MaxDistanceError, 1);
            modes[FilterMode.TimeErrorPoints] = new FilterModeClass(FilterMode.TimeErrorPoints, 0, ccatInterpreter.instance.MaxTimeError, 1);
            modes[FilterMode.TemperaturePoints] = new FilterModeClass(FilterMode.TemperaturePoints, 0, ccatInterpreter.instance.MaxTemperature, 1);
        }
    }

    public void SetParams(string type)
    {
        switch (type)
        {
            #region Voxels            
            case "Voxels":
                SetSliders("Voxels");
                if (Sloxelizer2.instance == null) break;

                foreach (var voxel in Sloxelizer2.instance.voxels)
                {
                    var info = voxel.Cube.GetComponent<Info>();
                    var state = info.state;
                    float vValue;
                    switch (voxelFilterMode)
                    {
                        case FilterMode.MedianPaths:
                            vValue = voxel.MeanSeparation;
                            break;
                        case FilterMode.MinSepPaths:
                            vValue = voxel.MinSeparation;
                            break;
                        case FilterMode.NumIntersectionsPaths:
                            vValue = voxel.NumIntersections;
                            break;
                        default:
                            vValue = 1f;
                            break;
                    }
                    if (MaxVoxelsSlider.value>= vValue && MinVoxelsSlider.value <= vValue)
                    {
                        if (info.state != Info.State.Selected)
                        {
                            info.state = Info.State.PartOfSet;
                            info.SetActive(true);
                        }
                    }
                    else
                    {
                        if (info.state != Info.State.Selected)
                        {
                            info.state = Info.State.Standard;
                            info.SetActive(false);
                        }
                    }
                }
                #endregion
                break;
            #region Points
            case "Points":
                SetSliders("Points");
                if (ccatInterpreter.instance == null) break;
                foreach (var ball in VAME_Manager.crazyBalls)
                {
                    var cb = ball.GetComponent<ccatBall>();
                    float pValue = 0;
                    switch (pointsFilterMode)
                    {
                        case FilterMode.DistanceErrorPoints:
                            pValue = cb.Distance;
                            break;
                        case FilterMode.TimeErrorPoints:
                            pValue = cb.TimeError;
                            break;
                        case FilterMode.TemperaturePoints:
                            pValue = cb.Temp;
                            break;
                        default:
                            pValue = 1f;
                            break;
                    }
                    
                    if (MaxPointsSlider.value >= pValue && MinPointsSlider.value <= pValue)
                    {
                        if (cb.ballState != ccatInterpreter.BallState.Selected)
                        {
                            cb.ballState = ccatInterpreter.BallState.PartOfSet;
                            cb.SetActive(true);
                        }
                    }
                    else
                    {
                        if (cb.ballState != ccatInterpreter.BallState.Selected)
                        {
                            cb.ballState = ccatInterpreter.BallState.Default;
                            cb.SetActive(false);
                        }
                    }
                }
                #endregion
                break;
            default:
                break;
        }        
    }

    private void SetValues()
    {

    }

    private void SetSliders(string type)
    {
        FilterModeClass thisMode = modes[FilterMode.None];
        Slider sMin = MinVoxelsSlider;
        Slider sMax = MaxVoxelsSlider;
        Text tMin = MinVoxelsSlider.GetComponentInChildren<Text>();
        Text tMax = MaxVoxelsSlider.GetComponentInChildren<Text>();
        switch (type)
        {
            case "Points":
                thisMode = modes[pointsFilterMode];
                sMin = MinPointsSlider;
                sMax = MaxPointsSlider;
                pointsMinText.text = sMin.value.ToString();
                pointsMaxText.text = sMax.value.ToString();
                break;
            default:
                thisMode = modes[voxelFilterMode];
                sMin = MinVoxelsSlider;
                sMax = MaxVoxelsSlider;
                voxelMinText.text = sMin.value.ToString();
                voxelMaxText.text = sMax.value.ToString();
                break;
        }
        sMin.maxValue = sMax.value;
        thisMode.CurrentValueMin = sMin.value;
        thisMode.CurrentValueMax = sMax.value;
    }
}

public class FilterModeClass
{
    public FilterModeClass ()
    {
        filterMode = VoxelInspector.FilterMode.None;
        Min = 0;
        Max = 1;
        Visibilty = 1;
        CurrentValueMax = 1;
        CurrentValueMin = 0;
    }
    public FilterModeClass (VoxelInspector.FilterMode mode, float min, float max, float vis)
    {
        filterMode = mode;
        Min = min;
        Max = max;
        Visibilty = vis;
        CurrentValueMax = Max;
        CurrentValueMin = Min;
    }
    public VoxelInspector.FilterMode filterMode { get; set; }
    public float Min { get; set; }
    public float Max { get; set; }
    public float Visibilty { get; set; }
    public float CurrentValueMax { get; set; }
    public float CurrentValueMin { get; set; }
}