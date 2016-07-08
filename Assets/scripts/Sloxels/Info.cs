using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Info : MonoBehaviour
{
    private Sloxel sloxel;
    private Sloxelizer2 sl2;
    bool activated = false;

	void Start ()
    {
	
	}
	void Update ()
    {
	
	}

    public void Activate (Sloxel _sloxel, Sloxelizer2 _sl2)
    {
        sloxel = _sloxel;
        sl2 = _sl2;
        activated = true;
    }

    void OnMouseDown()
    {
        if (!activated) return;
        foreach (var v2 in sloxel.PathLines)
        {
            print("Line[" + v2.x.ToString("f4") + "][" + v2.y.ToString("f0") + "]");
        }
    }
}
