using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class gcdInterpreter
{
    public gcdInterpreter instance;
    public static float y = 0;
    public Vector3 Min { get; set; }
    public Vector3 Max { get; set; }
    public bool LaserOn = false;
    private Vector3 lastPoint;
    //private List<Vector3> verts = new List<Vector3>();
    int i = 0;
    float lastEndTime = 0;

    public gcdInterpreter(string _filename)
    {
        instance = this;
        y = 0;
        Min = Vector3.one * 10000;
        Max = Vector3.one * -10000;

        VAME_Manager.pathPoints.Clear();
        VAME_Manager.pathLines.Clear();
        var reader = new StreamReader(_filename);
        while (!reader.EndOfStream)
        {
            var s = reader.ReadLine();
            //VAME_Manager.pathsCode.Add(s);
            scanGCD(s);
        }
        if (VAME_Manager.pathsCode.Count == 0) return;
        VAME_Manager.PathsMax = Max;
        VAME_Manager.PathsMin = Min;
        VAME_Manager.instance.NormalizePaths();
        InspectorL.instance.btnPaths.interactable = true;
        InspectorL.instance.pathVisibiltySlider.interactable = true;
        VAME_Manager.instance.voxelizerButton.interactable = true;
        InspectorL._mode = InspectorL.Mode.paths;
        InspectorL.instance.codeDummy.SetHeight(VAME_Manager.pathsCode.Count);
        InspectorL.instance.CodeArea(0);
        VAME_Manager.MinLayerToShow = 10000f;
        VAME_Manager.MaxLayerToShow = -VAME_Manager.MaxLayerToShow;
        foreach (var list in VAME_Manager.pathPoints)
        {
            foreach (var pp in list.Value)
            {
                var v1 = pp.pp;
                var index = VAME_Manager.pathPoints[list.Key].IndexOf(pp);
                if (index <= 0)
                    continue;
                var p0 = VAME_Manager.pathPoints[list.Key][index - 1];
                var v0 = p0.pp;
                var newSegment = new PathLine(v0, v1, true);
                newSegment.Show = p0.Show; ;
                newSegment.StartTime = lastEndTime;
                var d = Vector3.Distance(v1, v0);
                var endTime = lastEndTime + d;
                newSegment.EndTime = endTime;
                lastEndTime = endTime;
                if (!VAME_Manager.pathLines.ContainsKey(list.Key))
                {
                    VAME_Manager.pathLines.Add(list.Key, new List<PathLine>());
                    VAME_Manager.pathHeights.Add(list.Key);
                    if (list.Key > VAME_Manager.MaxLayerToShow)
                        VAME_Manager.MaxLayerToShow = list.Key;
                    if (list.Key < VAME_Manager.MinLayerToShow)
                        VAME_Manager.MinLayerToShow = list.Key;
                }
                newSegment.Index = VAME_Manager.allPathLines.Count;
                newSegment.Layer = list.Key;
                newSegment.LineInCode = p0.LineInCode;
                VAME_Manager.pathLines[list.Key].Add(newSegment);
                VAME_Manager.allPathLines.Add(newSegment);
            }            
        }
        InspectorL.instance.pathsMinSlider.interactable = true;
        InspectorL.instance.pathsMaxSlider.interactable = true;
        InspectorL.instance.pathsMinSlider.GetComponentInChildren<Text>().text = "Paths (Min): 0";
        InspectorL.instance.pathsMaxSlider.GetComponentInChildren<Text>().text = "Paths (Max): " + VAME_Manager.allPathLines.Count.ToString();
        foreach (var h in VAME_Manager.pathHeights)
        {
            var index = VAME_Manager.pathHeights.IndexOf(h);
            if (index == 0) continue;
            VAME_Manager.averagePathsHeight += h - VAME_Manager.pathHeights[index - 1];
        }
        VAME_Manager.averagePathsHeight /= VAME_Manager.pathPoints.Count;

        var buildEndTime = 1f;
        if (VAME_Manager.allPathLines.Count > 0)
        {
            buildEndTime = VAME_Manager.allPathLines[VAME_Manager.allPathLines.Count - 1].EndTime;
            if (buildEndTime <= 0)
                buildEndTime = 1f;
        }
        foreach (var pathLine in VAME_Manager.allPathLines)
        {
            pathLine.StartTime /= buildEndTime;
            pathLine.EndTime /= buildEndTime;
        }

        if (VAME_Manager.slicerForm != null)
        {
            VAME_Manager.slicerForm.panel1.Invalidate();
        }
    }

    void scanGCD(string _line)
    {
        VAME_Manager.pathsCode.Add(_line);
        if (_line == "\r\n") return;
        _line = _line.Trim();
        //gcdCode.Add(_line.ToString() + "\r\n");
        if (!_line.Contains(' ')) return;
        var chunks = _line.Split(' ');
        switch (chunks[0][0])
        {
            case 'G':
                if (!chunks[1].StartsWith("F"))
                {
                    StartsWithG(_line);
                }
                break;
            case 'O':
                if (chunks[0][1] == 'U')
                {
                    if (chunks.Length > 1 && chunks[1].Contains(","))
                    {
                        var command = chunks[1].Split(',');
                        if (command[1] == "0")
                            LaserOn = false;
                        else LaserOn = true;
                    }
                }
                //gcdInterpreter.StartsWithOU(_line);
                break;
            default:
                break;
        }
    }

    public void StartsWithG(string _line)
    {
        if (!_line.Contains(' ')) return;
        var _chunks = _line.Split(' ');
        var _chunk = _chunks[0]; // Initial Line, which starts with 'G'
        if (_chunks.Length < 2) return;
        switch (_chunk[1])
        {
            case '1':
                DoG(_chunks);
                break;
            default:
                break;
        }
    }

    public void StartsWithOU(string _line)
    {
        if (!_line.Contains(' ')) return;
        var _chunks = _line.Split(' ');
        var onOff = _chunks[1].Split(',')[1];
        switch (onOff[1])
        {
            case '1':
                LaserOn = true;
                break;
            case '0':
                LaserOn = false;
                break;
            default:
                break;
        }
    }

    void DoG(string[] _chunks)
    {
        //if (LaserOn) return;
        if (_chunks[1].Contains('F')) return;
        if (_chunks[1].StartsWith("Z"))
        {
            var Y = _chunks[1].TrimStart('Z');
            float _y;
            if (float.TryParse(Y, out _y))
            {
                y = _y;
                i++;
                return;
            }
        }
        Vector3 newVertex = new Vector3();
        var X = _chunks[1].TrimStart('X');
        var Z = _chunks[2].TrimStart('Y');
        float x;
        float z;
        if (float.TryParse(X, out x))
        {
            newVertex.x = x;
        }
        if (float.TryParse(Z, out z))
        {
            newVertex.z = z;
        }
        newVertex.y = y;
        var max = Max;
        var min = Min;
        if (x > Max.x) max.x = x;
        if (x < Min.x) min.x = x;
        if (y > Max.y) max.y = y;
        if (y < Min.y) min.y = y;
        if (z > Max.z) max.z = z;
        if (z < Min.z) min.z = z;
        Max = max;
        Min = min;
        
        if (!VAME_Manager.pathPoints.ContainsKey(y))
            VAME_Manager.pathPoints.Add(y, new List<pathPoint>());
        var pp = new pathPoint(newVertex, true);
        pp.LineInCode = VAME_Manager.pathsCode.Count - 1;
        pp.Show = LaserOn;
        VAME_Manager.pathPoints[y].Add(pp);


        //if (verts.Count < 2)
        //    verts.Add(newVertex);
        //if (verts.Count >= 2)
        //{
        //    if (!VAME_Manager.gcdPathLines.ContainsKey(y))
        //    {
        //        VAME_Manager.gcdPathLines.Add(y, new List<PathLine>());
        //    }
        //    VAME_Manager.gcdPathLines[y].Add(new PathLine(verts[0], verts[1]));
        //    verts.Clear();
        //}
    }

    public void StartsWithO(string _line)
    {
        var _chunks = _line.Split(' ');
        var _chunk = _chunks[0]; // Initial Line, which starts with 'O'
        switch (_chunk[1])
        {
            case 'U':
                switch (_chunks[2][3])
                {
                    case '0':
                        MessageBox.Show("Off");
                        break;
                    case '1':
                        MessageBox.Show("On");
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }
}

public class pathPoint
{
    public pathPoint(Vector3 _pp, bool laserOn)
    {
        pp = _pp;
        Show = true;
        LaserOn = laserOn;
    }
    public Vector3 pp { get; set; }
    public int LineInCode { get; set; }
    public bool Show { get; set; }
    public bool LaserOn { get; set; }
}
