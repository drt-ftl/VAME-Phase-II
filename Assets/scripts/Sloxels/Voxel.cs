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
}
