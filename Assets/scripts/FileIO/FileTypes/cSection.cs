﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

public class cSection : MonoBehaviour
{
    public static cSection instance;
    public static List<List<PathLine>> cSectionsX = new List<List<PathLine>>();
    public static List<List<PathLine>> cSectionsY = new List<List<PathLine>>();
    public static List<List<PathLine>> cSectionsZ = new List<List<PathLine>>();
    public static Dictionary<float, List<PathLine>> cSectionsGCD = new Dictionary<float, List<PathLine>>();

    void Awake()
    {
        instance = this;
    }

    List<PathLine> lines = new List<PathLine>();

    public void DoSlices(float divisions)
    {
        cSectionsX.Clear();
        cSectionsY.Clear();
        cSectionsZ.Clear();
        cSectionsGCD.Clear();

        var _tmpMin = VAME_Manager.Min;
        var minX = Mathf.Floor(_tmpMin.x * divisions) / divisions;
        var minY = Mathf.Floor(_tmpMin.y * divisions) / divisions;
        var minZ = Mathf.Floor(_tmpMin.z * divisions) / divisions;
        var increment = 4.0f / divisions;

        for (float height = minY; height <= VAME_Manager.Max.y; height += increment)
        {
            SliceItY(height);
            var newList = new List<PathLine>();
            foreach (var line in lines)
            {
                newList.Add(new PathLine(line.p1, line.p2, true));
            }
            cSectionsY.Add(newList);
        }

        for (float height = minX; height <= VAME_Manager.Max.x; height += increment)
        {
            SliceItX(height);
            var newList = new List<PathLine>();
            foreach (var line in lines)
            {
                newList.Add(new PathLine(line.p1, line.p2, true));
            }
            cSectionsX.Add(newList);
        }

        for (float height = minZ; height <= VAME_Manager.Max.z; height += increment)
        {
            SliceItZ(height);
            var newList = new List<PathLine>();
            foreach (var line in lines)
            {
                newList.Add(new PathLine(line.p1, line.p2, true));
            }
            cSectionsZ.Add(newList);
        }
        
    }

    public void SliceItGCD ()
    {
        foreach (var list in VAME_Manager.pathLines)
        {
            SliceItY(list.Key);
            var newList = new List<PathLine>();
            foreach (var line in lines)
            {
                newList.Add(new PathLine(line.p1, line.p2, true));
            }
            cSectionsGCD.Add(list.Key, newList);
        }
    }

    public List<Vector3> SliceItY (float y)
    {
        #region sliceIt
        lines.Clear();
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
                    var p_23 = p2 + m_23 * t_23;
                    var p_31 = p3 + m_31 * t_31;
                    var s = new PathLine(p_23, p_31, true);
                    lines.Add(s);
                }
                else if (p3.y > y)                              // p1 p3 above. p2 below.
                {
                    var p_12 = p1 + m_12 * t_12;
                    var p_23 = p2 + m_23 * t_23;
                    var s = new PathLine(p_12, p_23, true);
                    lines.Add(s);
                }
                else                                            // p1 above. p2 p3 below.
                {
                    var p_12 = p1 + m_12 * t_12;
                    var p_31 = p3 + m_31 * t_31;
                    var s = new PathLine(p_12, p_31, true);
                    lines.Add(s);
                }
            }
            else if (p2.y > y)                                  // p2 above. p1 is not. p3 unknown.
            {
                if (p3.y > y)                                   // p2 p3 above. p1 below.
                {
                    var p_12 = p1 + m_12 * t_12;
                    var p_31 = p3 + m_31 * t_31;
                    var s = new PathLine(p_12, p_31, true);
                    lines.Add(s);
                }
                else                                            // p2 above. p1 p3 below.
                {
                    var p_12 = p1 + m_12 * t_12;
                    var p_23 = p2 + m_23 * t_23;
                    var s = new PathLine(p_12, p_23, true);
                    lines.Add(s);
                }
            }
            else if (p3.y > y)                                  // p3 above. p1 p2 are not.
            {
                var p_23 = p2 + m_23 * t_23;
                var p_31 = p3 + m_31 * t_31;
                var s = new PathLine(p_23, p_31, true);
                lines.Add(s);
            }

            else continue;
        }
        #endregion

        List<Vector3> vectors = new List<Vector3>();
        for (int i = 0; i < lines.Count; i++)
        {
            vectors.Add(new Vector3(lines[i].p1.x, lines[i].p1.z, 0f));
            vectors.Add(new Vector3(lines[i].p2.x, lines[i].p2.z, 0f));
        }

        return vectors;
    }

    public List<Vector3> SliceItX(float x)
    {
        #region sliceIt
        lines.Clear();
        foreach (var tri in VAME_Manager.triangleList)
        {
            var p1 = tri.p1;
            var p2 = tri.p2;
            var p3 = tri.p3;

            var m_12 = Vector3.Normalize(p2 - p1);
            var m_23 = Vector3.Normalize(p3 - p2);
            var m_31 = Vector3.Normalize(p1 - p3);

            var t_12 = (x - p1.x) / m_12.x;
            var t_23 = (x - p2.x) / m_23.x;
            var t_31 = (x - p3.x) / m_31.x;


            if (p1.x > x)                                       // p1 above. p2 and p3 unknown.
            {
                if (p2.x > x && p3.x > x) continue;             // All are above
                if (p2.x > x)                                   // p1 p2 above. p3 below.
                {
                    var p_23 = p2 + m_23 * t_23;
                    var p_31 = p3 + m_31 * t_31;
                    var s = new PathLine(p_23, p_31, true);
                    lines.Add(s);
                }
                else if (p3.x > x)                              // p1 p3 above. p2 below.
                {
                    var p_12 = p1 + m_12 * t_12;
                    var p_23 = p2 + m_23 * t_23;
                    var s = new PathLine(p_12, p_23, true);
                    lines.Add(s);
                }
                else                                            // p1 above. p2 p3 below.
                {
                    var p_12 = p1 + m_12 * t_12;
                    var p_31 = p3 + m_31 * t_31;
                    var s = new PathLine(p_12, p_31, true);
                    lines.Add(s);
                }
            }
            else if (p2.x > x)                                  // p2 above. p1 is not. p3 unknown.
            {
                if (p3.x > x)                                   // p2 p3 above. p1 below.
                {
                    var p_12 = p1 + m_12 * t_12;
                    var p_31 = p3 + m_31 * t_31;
                    var s = new PathLine(p_12, p_31, true);
                    lines.Add(s);
                }
                else                                            // p2 above. p1 p3 below.
                {
                    var p_12 = p1 + m_12 * t_12;
                    var p_23 = p2 + m_23 * t_23;
                    var s = new PathLine(p_12, p_23, true);
                    lines.Add(s);
                }
            }
            else if (p3.x > x)                                  // p3 above. p1 p2 are not.
            {
                var p_23 = p2 + m_23 * t_23;
                var p_31 = p3 + m_31 * t_31;
                var s = new PathLine(p_23, p_31, true);
                lines.Add(s);
            }

            else continue;
        }
        #endregion

        List<Vector3> vectors = new List<Vector3>();
        for (int i = 0; i < lines.Count; i++)
        {
            vectors.Add(new Vector3(lines[i].p1.x, lines[i].p1.z, 0f));
            vectors.Add(new Vector3(lines[i].p2.x, lines[i].p2.z, 0f));
        }

        return vectors;
    }

    public List<Vector3> SliceItZ(float z)
    {
        #region sliceIt
        lines.Clear();
        foreach (var tri in VAME_Manager.triangleList)
        {
            var p1 = tri.p1;
            var p2 = tri.p2;
            var p3 = tri.p3;

            var m_12 = Vector3.Normalize(p2 - p1);
            var m_23 = Vector3.Normalize(p3 - p2);
            var m_31 = Vector3.Normalize(p1 - p3);

            var t_12 = (z - p1.z) / m_12.z;
            var t_23 = (z - p2.z) / m_23.z;
            var t_31 = (z - p3.z) / m_31.z;


            if (p1.z > z)                                       // p1 above. p2 and p3 unknown.
            {
                if (p2.z > z && p3.z > z) continue;             // All are above
                if (p2.z > z)                                   // p1 p2 above. p3 below.
                {
                    var p_23 = p2 + m_23 * t_23;
                    var p_31 = p3 + m_31 * t_31;
                    var s = new PathLine(p_23, p_31, true);
                    lines.Add(s);
                }
                else if (p3.z > z)                              // p1 p3 above. p2 below.
                {
                    var p_12 = p1 + m_12 * t_12;
                    var p_23 = p2 + m_23 * t_23;
                    var s = new PathLine(p_12, p_23, true);
                    lines.Add(s);
                }
                else                                            // p1 above. p2 p3 below.
                {
                    var p_12 = p1 + m_12 * t_12;
                    var p_31 = p3 + m_31 * t_31;
                    var s = new PathLine(p_12, p_31, true);
                    lines.Add(s);
                }
            }
            else if (p2.z > z)                                  // p2 above. p1 is not. p3 unknown.
            {
                if (p3.z > z)                                   // p2 p3 above. p1 below.
                {
                    var p_12 = p1 + m_12 * t_12;
                    var p_31 = p3 + m_31 * t_31;
                    var s = new PathLine(p_12, p_31, true);
                    lines.Add(s);
                }
                else                                            // p2 above. p1 p3 below.
                {
                    var p_12 = p1 + m_12 * t_12;
                    var p_23 = p2 + m_23 * t_23;
                    var s = new PathLine(p_12, p_23, true);
                    lines.Add(s);
                }
            }
            else if (p3.z > z)                                  // p3 above. p1 p2 are not.
            {
                var p_23 = p2 + m_23 * t_23;
                var p_31 = p3 + m_31 * t_31;
                var s = new PathLine(p_23, p_31, true);
                lines.Add(s);
            }

            else continue;
        }
        #endregion

        List<Vector3> vectors = new List<Vector3>();
        for (int i = 0; i < lines.Count; i++)
        {
            vectors.Add(new Vector3(lines[i].p1.x, lines[i].p1.z, 0f));
            vectors.Add(new Vector3(lines[i].p2.x, lines[i].p2.z, 0f));
        }

        return vectors;
    }
}



public class PathLine
{
    bool show = true;
    public PathLine(Vector3 _p1, Vector3 _p2, bool laserOn)
    {
        p1 = _p1;
        p2 = _p2;
        Show = true;
        Color = VAME_Manager.instance.pathColor;
        ccatBalls = new List<GameObject>();
        LaserOn = laserOn;
    }
    public Vector3 p1 { get; set; }
    public Vector3 p2 { get; set; }
    public bool LaserOn { get; set; }
    public bool Show
    {
        get
        {
            var selectedLayer = false;
            var max = InspectorL.instance.pathsMaxSlider.value * VAME_Manager.allPathLines.Count;
            var min = InspectorL.instance.pathsMinSlider.value * VAME_Manager.allPathLines.Count;
            if (VAME_Manager.slicerForm != null)
            {
                var v = VAME_Manager.slicerForm.LayerUpDown.Value;
                var layer = VAME_Manager.pathHeights[(int)v];
                if (layer == Layer)
                    selectedLayer = true;
            }
            if ((Index > max ||
                Index < min)
                && !selectedLayer)
                return false;
            return show;
        }
        set { show = value; }
    }

    private float minDistance = 10000f;
    public float MinDistance
    {
        get
        {
            foreach (var line in cSection.cSectionsGCD[Layer])
            {
                if (line == this) continue;
                var P1 = p1;
                var P2 = p2;
                var M = P2 - P1;
                var _P1 = line.p1;
                var _P2 = line.p2;
                var _M = _P2 - _P1;

                var A1 = 1.0f;
                var B1 = M.y / M.x;
                var C1 = B1 * P1.x - P1.y;

                var A2 = 1.0f;
                var B2 = _M.y / _M.x;
                var C2 = B2 * _P1.x - _P1.y;

                float delta = A1 * B2 - A2 * B1;
                if (delta == 0) // parallel
                {
                }
                else
                {
                    var x = (B2 * C1 - B1 * C2) / delta;
                    var y = (A1 * C2 - A2 * C1) / delta;
                    
                }
            }
            return minDistance;
        }
    }

    public float StartTime { get; set; }
    public float EndTime { get; set; }
    public Color Color { get; set; }
    public Sloxel Sloxel {get;set;}
    public List<GameObject> ccatBalls { get; set; }
    public int LineInCode { get; set; }
    public int Index { get; set; }
    public float Layer { get; set; }
}

