using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Voxelizer
{
    public static List<Vector3> origins = new List<Vector3>();

	public Voxelizer(float divisions)
    {
        var min = VAME_Manager.Min;
        var max = VAME_Manager.Max;
        var increment = 1.0f / divisions;
        min.x = Mathf.Floor(min.x * divisions) / divisions;
        min.y = Mathf.Floor(min.y * divisions) / divisions;
        min.z = Mathf.Floor(min.z * divisions) / divisions;
        for (var y = min.y; y <= max.y; y+= increment)
        {
            for (var z = min.z; z <= max.z; z += increment)
            {
                for (var x = min.x; x <= max.x; x += increment)
                {
                    var left = 0;
                    var right = 0;
                    foreach (var cs in cSection.cSectionsY)
                    {
                        //if (cs.Count >=1 && (y < cs[0].p1.y + 0.1f && y > cs[0].p1.y - 0.1f))
                        //{
                        //    foreach (var line in cs)
                        //    {
                        //        if ((line.p1.z > z && line.p2.z <= z)
                        //            || (line.p1.z <= z && line.p2.z > z))
                        //        {
                        //            var m = line.p2 - line.p1;
                        //            var xInt = (line.p1.x + x) / m.x;
                        //            if (xInt <= x) left++;
                        //            else right++;
                        //        }
                        //    }
                        //}
                    }
                    if (left % 2 != 0 && right % 2 !=0)
                    {
                        origins.Add(new Vector3(x, y, z));
                    }
                }
            }
        }
        MonoBehaviour.print(origins.Count.ToString());
    }
}
