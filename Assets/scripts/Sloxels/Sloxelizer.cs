using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Sloxelizer
{
    int numberOfThem = 0;
    public Dictionary<float, List<Sloxel>> sloxels = new Dictionary<float, List<Sloxel>>();
    public List<Voxel> voxels = new List<Voxel>();

    public Sloxelizer(float divs)
    {
        var min = VAME_Manager.PathsMin;
        var max = VAME_Manager.PathsMax;
        var biggest = Mathf.Max(Mathf.Abs(max.x - min.x), Mathf.Abs(max.z - min.z));
        var increment = 4.0f / divs;
        min.x = Mathf.Floor(min.x * divs) / divs;
        min.y = Mathf.Floor(min.y * divs) / divs;
        min.z = Mathf.Floor(min.z * divs) / divs;
        min -= Vector3.one * increment;

        max.x = Mathf.Ceil(max.x * divs) / divs;
        max.y = Mathf.Ceil(max.y * divs) / divs;
        max.z = Mathf.Ceil(max.z * divs) / divs;
        max += Vector3.one * increment;
        foreach (var kvp in VAME_Manager.pathLines)
        {
            for (var x = min.x; x <= max.x; x += increment)
            {
                for (var z = min.z; z <= max.z; z += increment)
                {
                    if (CheckForSloxel(kvp.Key, new Vector2(x, z)))
                    {
                        if (!sloxels.ContainsKey(kvp.Key))
                        {
                            sloxels.Add(kvp.Key, new List<Sloxel>());
                        }
                        var newPt = new Vector3(x, kvp.Key, z);
                        var _pathLines = GatherPathLines(newPt, kvp.Key, increment);
                        var newSloxel = new Sloxel(newPt, _pathLines, increment);
                        if (!sloxels[kvp.Key].Contains(newSloxel))
                        {
                            sloxels[kvp.Key].Add(newSloxel);
                            MonoBehaviour.print("Sloxel!");
                        }
                    }
                }
            }
        }
        GatherVoxels(min, max, increment);
    }

    public bool CheckForSloxel(float y, Vector2 xz)
    {
        var lefts = 0;
        var rights = 0;
        foreach (var tri in VAME_Manager.triangleList)
        {
            var p1 = tri.p1;
            var p2 = tri.p2;
            var p3 = tri.p3;

            var m_12 = Vector3.Normalize(p2 - p1);
            var m_23 = Vector3.Normalize(p3 - p2);
            var m_31 = Vector3.Normalize(p1 - p3);

            var t_12 = (y - p1.y) / m_12.y;
            var t_23 = (y - p2.y) / m_23.y;
            var t_31 = (y - p3.y) / m_31.y;


            if (p1.y > y)                                       // p1 above. p2 and p3 unknown.
            {
                if (p2.y > y && p3.y > y) continue;             // All are above
                if (p2.y > y)                                   // p1 p2 above. p3 below.
                {
                    var p_a = p2 + m_23 * t_23;
                    var p_b = p3 + m_31 * t_31;
                    var v = p_b - p_a;
                    var x = xz.x;
                    var sh = x - p_a.x;
                    var frac = sh / v.x;
                    if (frac >= 0 && frac <= 1)
                    {
                        var p = p_a + frac * v;
                        if (p.x <= x)
                            lefts++;
                        else
                            rights++;
                    }
                }
                else if (p3.y > y)                              // p1 p3 above. p2 below.
                {
                    var p_a = p1 + m_12 * t_12;
                    var p_b = p2 + m_23 * t_23;
                    var v = p_b - p_a;
                    var x = xz.x;
                    var sh = x - p_a.x;
                    var frac = sh / v.x;
                    if (frac >= 0 && frac <= 1)
                    {
                        var p = p_a + frac * v;
                        if (p.x <= x)
                            lefts++;
                        else
                            rights++;
                    }
                }
                else                                            // p1 above. p2 p3 below.
                {
                    var p_a = p1 + m_12 * t_12;
                    var p_b = p3 + m_31 * t_31;
                    var v = p_b - p_a;
                    var x = xz.x;
                    var sh = x - p_a.x;
                    var frac = sh / v.x;
                    if (frac >= 0 && frac <= 1)
                    {
                        var p = p_a + frac * v;
                        if (p.x <= x)
                            lefts++;
                        else
                            rights++;
                    }
                }
            }
            else if (p2.y > y)                                  // p2 above. p1 is not. p3 unknown.
            {
                if (p3.y > y)                                   // p2 p3 above. p1 below.
                {
                    var p_a = p1 + m_12 * t_12;
                    var p_b = p3 + m_31 * t_31;
                    var v = p_b - p_a;
                    var x = xz.x;
                    var sh = x - p_a.x;
                    var frac = sh / v.x;
                    if (frac >= 0 && frac <= 1)
                    {
                        var p = p_a + frac * v;
                        if (p.x <= x)
                            lefts++;
                        else
                            rights++;
                    }
                }
                else                                            // p2 above. p1 p3 below.
                {
                    var p_a = p1 + m_12 * t_12;
                    var p_b = p2 + m_23 * t_23;
                    var v = p_b - p_a;
                    var x = xz.x;
                    var sh = x - p_a.x;
                    var frac = sh / v.x;
                    if (frac >= 0 && frac <= 1)
                    {
                        var p = p_a + frac * v;
                        if (p.x <= x)
                            lefts++;
                        else
                            rights++;
                    }
                }
            }
            else if (p3.y > y)                                  // p3 above. p1 p2 are not.
            {
                var p_a = p2 + m_23 * t_23;
                var p_b = p3 + m_31 * t_31;
                var v = p_b - p_a;
                var x = xz.x;
                var sh = x - p_a.x;
                var frac = sh / v.x;
                if (frac >= 0 && frac <= 1)
                {
                    var p = p_a + frac * v;
                    if (p.x <= x)
                        lefts++;
                    else
                        rights++;
                }
            }

            else continue;
        }
        if (lefts % 2 == 1 && rights % 2 == 1)
            return true;
        else return false;
    }

    List<Vector2> GatherPathLines(Vector3 sloxelCenter, float y, float increment)
    {
        var _pathLines = new List<Vector2>();
        foreach (var path in VAME_Manager.pathLines[y])
        {
            var intersect = CheckForIntersect(path.p1, path.p2, sloxelCenter);
            if (intersect != null)
            {
                if (intersect.x >= sloxelCenter.x - increment / 2 && intersect.x <= sloxelCenter.x + increment / 2
                    && intersect.z >= sloxelCenter.z - increment / 2 && intersect.z <= sloxelCenter.z + increment / 2)
                {
                    _pathLines.Add(new Vector2 (y, VAME_Manager.pathLines[y].IndexOf(path)));
                }
            }
        }
        return _pathLines;
    }

    Vector3 CheckForIntersect(Vector3 p1, Vector3 p2, Vector3 pt)
    {
        float dx = p2.x - p1.x;
        float dz = p2.z - p1.z;

        if (dx == 0 && dz == 0)
        {
            return p1;
        }
        float t = ((pt.x - p1.x) * dx + (pt.z - p1.z) * dz) /
        (dx * dx + dz * dz);

        if (t < 0)
        {
            return p1;
        }
        else if (t > 1)
        {
            return p2;
        }
        else
        {
            return new Vector3(p1.x + t * dx, pt.y, p1.z + t * dz);
        }
    }

    void GatherVoxels(Vector3 min, Vector3 max, float increment)
    {
        for (var y = min.y; y <= max.y; y+= increment)
        {
            var i = 0;
            for (var z = min.z; z <= max.z; z += increment)
            {
                for (var x = min.x; x <= max.x; x += increment)
                {
                    var _sloxels = new List<Sloxel>();
                    foreach (var layer in sloxels)
                    {
                        if (layer.Key >= y - increment / 2 && layer.Key <= y + increment / 2)
                        {
                            foreach (var sloxel in layer.Value)
                            {
                                MonoBehaviour.print(i.ToString());
                                i++;
                                //if (Mathf.Abs(sloxel.origin.x - x) <= 2.0f && Mathf.Abs(sloxel.origin.z - z) <= 2.0f)
                                //{
                                //    _sloxels.Add(sloxel);
                                //}
                            }
                        }
                    }
                    if (_sloxels.Count > 0)
                    {
                        var v = new Voxel(_sloxels, new Vector3(x, y, z), Vector3.one * increment);
                        voxels.Add(v);
                    }
                }
            }
        }
    }
}
