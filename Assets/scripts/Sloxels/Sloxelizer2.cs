using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Sloxelizer2
{
    public Dictionary<float, List<Sloxel>> sloxels = new Dictionary<float, List<Sloxel>>();
    public List<Voxel> voxels = new List<Voxel>();
    public static Sloxelizer2 instance;
    public float HighestMean = 0;

    [HideInInspector]
    public float increment;

    public Sloxelizer2(float divs)
    {
        instance = this;
        
        cSection.instance.DoSlices(20);
        sloxels = new Dictionary<float, List<Sloxel>>();
        var min = VAME_Manager.PathsMin;
        var max = VAME_Manager.PathsMax;
        min.x = Mathf.Floor(min.x * divs) / divs;
        min.y = Mathf.Floor(min.y * divs) / divs;
        min.z = Mathf.Floor(min.z * divs) / divs;
        //min = -Vector3.one * 4.0f;

        max.x = Mathf.Ceil(max.x * divs) / divs;
        max.y = Mathf.Ceil(max.y * divs) / divs;
        max.z = Mathf.Ceil(max.z * divs) / divs;
        //max = Vector3.one * 4.0f;

        var biggest = Mathf.Max(Mathf.Abs(max.x - min.x), Mathf.Abs(max.z - min.z));
        increment = biggest / (divs);
        min -= Vector3.one * increment;
        max += Vector3.one * increment;


        foreach (var cSect in VAME_Manager.pathLines)
        {
            sloxels.Add(cSect.Key, new List<Sloxel>());
            for (var x = min.x; x <= max.x; x += increment)
            {
                for (var z = min.z; z <= max.z; z += increment)
                {
                    if (!sloxels.ContainsKey(cSect.Key))
                    {
                        sloxels.Add(cSect.Key, new List<Sloxel>());
                    }
                    var getInts = GetIntersects(new Vector3(x, cSect.Key, z));
                    if (getInts.Count < 1) continue;
                    var newSloxel = new Sloxel(new Vector3(x, cSect.Key, z), getInts, increment);
                    newSloxel.SetDistances();
                    if (!sloxels[cSect.Key].Contains(newSloxel))
                    {
                        sloxels[cSect.Key].Add(newSloxel);
                    }
                }
            }
        }

        for (float y = min.y; y <= max.y; y += increment)
        {
            for (var z = min.z; z <= max.z; z += increment)
            {
                for (var x = min.x; x <= max.x; x += increment)
                {
                    var here = new Vector2(x, z);
                    var newVoxel = new Voxel(new List<Sloxel>(), new Vector3(x, y, z), Vector3.one * increment);
                    foreach (var list in sloxels)
                    {
                        if (list.Key > y - increment / 2 && list.Key < y + increment / 2)
                        {
                            foreach (var sl in list.Value)
                            {
                                var there = new Vector2(sl.origin.x, sl.origin.z);
                                if (here == there)
                                {
                                    newVoxel.Sloxels.Add(sl);
                                    sl.Voxel = newVoxel;
                                }
                            }
                        }
                    }
                    if (newVoxel.Sloxels.Count > 0)
                    {
                        newVoxel.SetDistances();
                        voxels.Add(newVoxel);
                    }
                }
            }
        }
        if (VAME_Manager.slicerForm == null)
        {
            VAME_Manager.slicerForm = new SlicerForm.SlicerForm();
        }
        VAME_Manager.slicerForm.Show();
        VAME_Manager.slicerForm.panel1.Invalidate();
        VoxelInspector.instance.Min.interactable = true;
        VoxelInspector.instance.Max.interactable = true;
    }

    public List<Vector2> GetIntersects(Vector3 p)
    {
        var pathLines = new List<Vector2>();
        foreach (var line in VAME_Manager.pathLines[p.y])
        {
            if (CheckForIntersect(p, line.p1, line.p2))
            {
                pathLines.Add(new Vector2(p.y, VAME_Manager.pathLines[p.y].IndexOf(line)));
            }
        }
        return pathLines;
    }


    public bool CheckForIntersect(Vector3 p, Vector3 p1, Vector3 p2)
    {
        var min = p - Vector3.one * increment / 2f;
        var max = p + Vector3.one * increment / 2f;
        var line = p2 - p1;
        if (p1.x == p2.x) //              travels in x
        {
            if (p1.z == p2.z) return false;
            if (((min.z - p1.z) / line.z >= 0 && (min.z - p1.z) / line.z <= 1) ||
                ((max.z - p1.z) / line.z >= 0 && (max.z - p1.z) / line.z <= 1))
            {
                if (p1.x >= min.x && p1.x <= max.x)
                {
                    return true;
                }
            }
            else return false;
        }

        if (p1.z == p2.z) //              travels in z
        {
            if (p1.x == p2.x) return false;
            if (((min.x - p1.x) / line.x >= 0 && (min.x - p1.x) / line.x <= 1) ||
                ((max.x - p1.x) / line.x >= 0 && (max.x - p1.x) / line.x <= 1))
            {
                if (p1.z >= min.z && p1.z <= max.z)
                {
                    return true;
                }
            }
            else return false;
        }

        var t = (min.x - p1.x) / line.x;
        if (t >= 0 && t <= 1)
        {
            var z = p1.z + t * line.z;
            if (z >= min.z && z <= max.z)
                return true;
        }

        t = (max.x - p1.x) / line.x;
        if (t >= 0 && t <= 1)
        {
            var z = p1.z + t * line.z;
            if (z >= min.z && z <= max.z)
                return true;
        }

        t = (min.z - p1.z) / line.z;
        if (t >= 0 && t <= 1)
        {
            var x = p1.x + t * line.x;
            if (x >= min.x && x <= max.x)
                return true;
        }

        t = (max.z - p1.z) / line.z;
        if (t >= 0 && t <= 1)
        {
            var x = p1.x + t * line.x;
            if (x >= min.x && x <= max.x)
                return true;
        }
        return false;
    }

    public void GetDistances()
    {
        
    }
}

