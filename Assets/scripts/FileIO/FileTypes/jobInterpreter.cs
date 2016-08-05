using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class jobInterpreter
{
    public static jobInterpreter instance;
    public static float y = 0;
    private Vector3 Min { get; set; }
    private Vector3 Max { get; set; }
    public bool LaserOn = false;
    private Vector3 lastPoint;
    float lastEndTime = 0;
    public static float layerHeight = 0.001f;

    //public Vector3 centroid
    //{
    //    get
    //    {
    //        var c = (Min + Max) / 2.0f;
    //        return c;
    //    }
    //}

    public jobInterpreter(string _filename)
    {
        instance = this;
        LaserOn = true;
        y = 0;
        Min = Vector3.one * 10000;
        Max = Vector3.one * -10000;

        VAME_Manager.pathPoints.Clear();
        VAME_Manager.pathLines.Clear();
        var reader = new StreamReader(_filename);
        while (!reader.EndOfStream)
        {
            var s = reader.ReadLine();
            scanJOB(s);
        }
        reader.Close();
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
                var newSegment = new PathLine(v0, v1, pp.LaserOn);
                if (v0.y != v1.y)
                    newSegment.Show = false;
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
                //newSegment.Show = pp.Show;
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
            //VAME_Manager.averagePathsHeight += h - VAME_Manager.pathHeights[index - 1];
        }
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
        //VAME_Manager.averagePathsHeight /= VAME_Manager.pathPoints.Count;
        if (VAME_Manager.slicerForm != null)
        {
            VAME_Manager.slicerForm.panel1.Invalidate();
        }
    }

    void scanJOB(string _line)
    {
        VAME_Manager.pathsCode.Add(_line.ToString() + "\r\n");
        if (_line.Contains("M14"))
        {
            LaserOn = true;
            return;
        }
        if (_line.Contains("M15"))
        {
            LaserOn = false;
            return;
        }
        if (_line == "\r\n") return;
        _line = _line.Trim();
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
            case '*':
                StartsWithStar(_line);
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
        if (_chunks.Length < 3) return;
        switch (_chunk[1])
        {
            case '0':
                switch (_chunk[2])
                {
                    case '0':
                        
                        break;
                    case '1':
                        DoG(_chunks);
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }

    void DoG(string[] _chunks)
    {
        Vector3 newVertex = new Vector3();
        var X = _chunks[1].TrimStart('X');
        var Z = _chunks[2].TrimStart('Y');
        float x;
        float z;
        if (float.TryParse(X, out x))
            newVertex.x = x;
        if (float.TryParse(Z, out z))
            newVertex.z = z;
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
        var pp = new pathPoint(newVertex, LaserOn);
        pp.LineInCode = VAME_Manager.pathsCode.Count - 1;
        VAME_Manager.pathPoints[y].Add(pp);
    }

    public void StartsWithM(string _line)
    {
        var _chunks = _line.Split(' ');
        var _chunk = _chunks[0]; // Initial Line, which starts with 'G'
        switch (_chunk[1])
        {
            case '0':
                switch (_chunk[2])
                {
                    case '0':
                        break;
                    case '1':
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }

	public void StartsWithStar(string _line)
	{
		var _chunks = _line.Split(' ');
        if (_chunks[0] == "*REM" && _chunks[1] == "Layer")
        {
            if (_chunks[2] == "Thickness")
            {
                float th;
                if (float.TryParse(_chunks[4], out th))
                {
                    layerHeight = th;
                    VAME_Manager.averagePathsHeight = layerHeight;
                }
            }
            if (_chunks[2] == "Change")
                y += layerHeight;
        }
	}
}
