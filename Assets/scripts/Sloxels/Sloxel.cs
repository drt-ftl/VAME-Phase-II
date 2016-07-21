using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sloxel
{
    public Sloxel(Vector3 _origin, List<Vector2> _pathLines, float increment)
    {
        origin = _origin;
        PathLines = new List<Vector2>(_pathLines);
        Scale = new Vector3(increment, 0.1f, increment);
    }

    public Vector3 origin           { get; internal set; }
    public List<Vector2> PathLines  { get; internal set; }
    public Vector3 Scale            { get; internal set; }
    public Voxel Voxel { get; set; }
}
