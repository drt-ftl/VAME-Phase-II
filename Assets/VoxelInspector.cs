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

    void Awake()
    {
        instance = this;
    }

    public void SetScrollText(string txt)
    {
        scrollViewText.text = txt;
    }
}
