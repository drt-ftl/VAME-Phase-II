using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Voxel
{
    public Voxel (List<Sloxel> _sloxels, Vector3 _origin, Vector3 _scale)
    {
        Sloxels = new List<Sloxel>(_sloxels);
        origin = _origin;
        Scale = _scale;
    }
    public List<Sloxel> Sloxels { get; internal set; }
    public Vector3 origin { get; internal set; }
    public Vector3 Scale { get; internal set; }
    public GameObject Cube { get; set; }
    public int NumIntersections
    {
        get
        {
            var num = 0;
            foreach (var sloxel in Sloxels)
            {
                num++;
            }
            return num;
        }
    }
    public void SetDistances ()
    {
        var running = 0f;
        var i = 0f;
        var distances = new List<float>();
        foreach (var sloxel in Sloxels)
        {
            foreach (var d in sloxel.distances)
            {
                running += d;
                distances.Add(d);
                i++;
            }
        }
        if (i > 0)
        {
            MeanSeparation = running / i;
        }
        else MeanSeparation = -1;
        if (MeanSeparation > Sloxelizer2.instance.HighestMean)
            Sloxelizer2.instance.HighestMean = MeanSeparation;
        MinSeparation = Mathf.Min(distances.ToArray());
        MaxSeparation = Mathf.Max(distances.ToArray());
    }
    public float MinSeparation { get; set; }
    public float MaxSeparation { get; set; }
    public float MeanSeparation { get; set; }
}
