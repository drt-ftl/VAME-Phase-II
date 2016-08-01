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

    void Awake()
    {
        instance = this;
    }

    public void SetScrollText(string txt)
    {
        scrollViewText.text = txt;
    }

    public void SetParams()
    {
        Min.maxValue = Max.value;
        Max.GetComponentInChildren<Text>().text = "Max: " + (Max.value * Sloxelizer2.instance.HighestMean).ToString("f4");
        Min.GetComponentInChildren<Text>().text = "Min: " + (Min.value * Sloxelizer2.instance.HighestMean).ToString("f4");

        foreach (var voxel in Sloxelizer2.instance.voxels)
        {
            if (Max.value * Sloxelizer2.instance.HighestMean > voxel.MeanSeparation && Min.value * Sloxelizer2.instance.HighestMean < voxel.MeanSeparation)
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
