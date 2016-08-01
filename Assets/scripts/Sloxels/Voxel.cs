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
    public void SetDistances ()
    {
        var running = 0f;
        var i = 0f;
        foreach (var sloxel in Sloxels)
        {
            foreach (var d in sloxel.distances)
            {
                running += d;
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
    }
    public float MaxSeaparation { get; set; }
    public float MeanSeparation { get; set; }
}
