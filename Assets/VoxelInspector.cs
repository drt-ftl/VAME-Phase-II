using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VoxelInspector : MonoBehaviour
{
    public static VoxelInspector instance;
    public Text scrollViewText;
    public Material standard;
    public Material highlight;
    public Material selected;
    public Slider Min;
    public Slider Max;
    public Slider Resolution;
    public enum FilterMode { None, Min, Median }
    [HideInInspector]
    public FilterMode filterMode;

    void Awake()
    {
        instance = this;
        filterMode = FilterMode.None;
        Resolution.value = 10;
    }

    public void SetScrollText(string txt)
    {
        scrollViewText.text = txt;
    }

    public void SetFilterMode(string _filterMode)
    {
        switch (_filterMode)
        {
            case "Min":
                filterMode = FilterMode.Min;
                break;
            case "Median":
                filterMode = FilterMode.Median;
                break;
            default:
                filterMode = FilterMode.None;
                break;
        }
    }

    public void SetParams()
    {
        Min.maxValue = Max.value;
        Max.GetComponentInChildren<Text>().text = "Max: " + (Max.value * Sloxelizer2.instance.HighestMean).ToString("f4");
        Min.GetComponentInChildren<Text>().text = "Min: " + (Min.value * Sloxelizer2.instance.HighestMean).ToString("f4");
        foreach (var voxel in Sloxelizer2.instance.voxels)
        {
            var info = voxel.Cube.GetComponent<Info>();
            var state = info.state;
            if ((Max.value * Sloxelizer2.instance.HighestMean >= voxel.MeanSeparation && Min.value * Sloxelizer2.instance.HighestMean <= voxel.MeanSeparation) ||
                voxel.MeanSeparation < 0 ||
                state == Info.State.Selected)
            {
                voxel.Cube.GetComponent<Info>().SetActive(true);
            }
            else
            {
                voxel.Cube.GetComponent<Info>().SetActive(false);
            }
        }
    }
}
