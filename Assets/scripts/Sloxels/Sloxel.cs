using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sloxel
{
    public Sloxel(Vector3 _origin, List<Vector2> _pathLines)
    {
        origin = _origin;
        PathLines = new List<Vector2>(_pathLines);
    }

    public Vector3 origin           { get; internal set; }
    public List<Vector2> PathLines { get; internal set; }
}
