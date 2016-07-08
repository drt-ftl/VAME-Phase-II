using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Sloxelizer2
{
    int numberOfThem = 0;
    public Dictionary<float, List<Sloxel>> sloxels = new Dictionary<float, List<Sloxel>>();

    public Sloxelizer2(float divs)
    {
        sloxels = new Dictionary<float, List<Sloxel>>();
        var min = VAME_Manager.Min;
        var max = VAME_Manager.Max;
        var biggest = Mathf.Max(Mathf.Abs(max.x - min.x), Mathf.Abs(max.z - min.z));
        var increment = biggest / divs;
        min.x = Mathf.Floor(min.x * divs) / divs;
        min.z = Mathf.Floor(min.z * divs) / divs;
        min.x += increment / 2;
        min.z += increment / 2;
        foreach (var kvp in VAME_Manager.gcdPathLines)
        {
            sloxels.Add(kvp.Key, new List<Sloxel>());
            for (var x = min.x; x <= max.x; x += increment)
            {
                for (var z = min.z; z <= max.z; z += increment)
                {
                    var cfs = CheckForSloxel(new Vector3(x, kvp.Key, z), kvp.Value, increment);
                    if (cfs != null)
                    {
                        if (!sloxels.ContainsKey(kvp.Key))
                        {
                            sloxels.Add(kvp.Key, new List<Sloxel>());
                        }
                        var newSloxel = new Sloxel(new Vector3(x, kvp.Key, z), cfs);
                        if (!sloxels[kvp.Key].Contains(newSloxel))
                        {
                            sloxels[kvp.Key].Add(newSloxel);
                            numberOfThem++;
                        }
                    }
                }
            }
        }
        MonoBehaviour.print("Layers2: " + VAME_Manager.gcdPathLines.Count);
        MonoBehaviour.print("Number: " + numberOfThem.ToString());
    }

    public List<Vector2> CheckForSloxel(Vector3 p, List<PathLine> _list, float _increment)
    {
        var lefts = 0;
        var rights = 0;
        var pathLines = new List<Vector2>();
        var high = p + Vector3.one * (_increment / 2.0f);
        var low = p - Vector3.one * (_increment / 2.0f);
        foreach (var line in _list)
        {
            var p1 = line.p1;
            var p2 = line.p2;
            if (p.z <= p2.z && p.z >= p1.z ||
                p.z <= p1.z && p.z >= p2.z)                 // p.z is between p1.z and p2.z
            {
                var xInt = (p1 + ((p.z - p1.z) / (p2.z - p1.z)) * (p2 - p1)).x;
                if (xInt <= p.x)
                    lefts++;
                if (xInt >= p.x)
                    rights++;
            }
            if (p1.x >= low.x && p1.x <= high.x)
            {
                if (p1.z >= low.z && p1.z <= high.z)
                {
                    if (!pathLines.Contains(new Vector2 (p.y, _list.IndexOf(line))))
                    {
                        pathLines.Add(new Vector2(p.y, _list.IndexOf(line)));
                    }
                }
            }
            if (p2.x >= low.x && p2.x <= high.x)
            {
                if (p2.z >= low.z && p2.z <= high.z)
                {
                    if (!pathLines.Contains(new Vector2(p.y, _list.IndexOf(line))))
                    {
                        pathLines.Add(new Vector2(p.y, _list.IndexOf(line)));
                    }
                }
            }
        }
        if (lefts > 0 && rights > 0)
            return null;
        
        return pathLines;
    }

    void CheckIt()
    {

    }
}

