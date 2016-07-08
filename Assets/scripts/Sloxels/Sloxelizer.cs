using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Sloxelizer
{
    int numberOfThem = 0;

    public Sloxelizer(float divs)
    {
        var sloxelCenters = new Dictionary<float, List<Vector3>>();
        var min = VAME_Manager.Min;
        var max = VAME_Manager.Max;
        var biggest = Mathf.Max(Mathf.Abs(max.x - min.x), Mathf.Abs(max.z - min.z));
        var increment = 4.0f / divs;
        min.x = Mathf.Floor(min.x * divs) / divs;
        min.z = Mathf.Floor(min.z * divs) / divs;
        MonoBehaviour.print("Min: " + min.ToString());
        MonoBehaviour.print("Inc: " + increment.ToString("f4"));
        foreach (var kvp in VAME_Manager.gcdPathLines)
        {
            sloxelCenters.Add(kvp.Key, new List<Vector3>());
            for (var x = min.x; x <= max.x; x += increment)
            {
                for (var z = min.z; z <= max.z; z += increment)
                {
                    if (CheckForSloxel(kvp.Key, new Vector2(x, z)))
                    {
                        if (!sloxelCenters.ContainsKey(kvp.Key))
                        {
                            sloxelCenters.Add(kvp.Key, new List<Vector3>());
                        }
                        var newPt = new Vector3(x, kvp.Key, z);
                        if (!sloxelCenters[kvp.Key].Contains(newPt))
                        {
                            sloxelCenters[kvp.Key].Add(newPt);
                            MonoBehaviour.print(newPt.ToString("f4"));
                            numberOfThem++;
                        }
                    }
                }
            }
        }
        MonoBehaviour.print("Layers: " + VAME_Manager.gcdPathLines.Count);
        MonoBehaviour.print("Number: " + numberOfThem.ToString());
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

    void CheckIt()
    {

    }
}
