using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UnityEngine;
using System.IO;

public class gcdInterpreter
{
    public gcdInterpreter instance;
    public static float y = 0;
    public Vector3 Min { get; set; }
    public Vector3 Max { get; set; }
    public bool LaserOn = false;
    private Vector3 lastPoint;
    private List<Vector3> verts = new List<Vector3>();
    int i = 0;
    float lastEndTime = 0;
    public static float averageHeight = 0;

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
            VAME_Manager.pathsCode.Add(s);
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
        foreach (var list in VAME_Manager.pathPoints)
        {
            foreach (var v1 in list.Value)
            {
                var index = VAME_Manager.pathPoints[list.Key].IndexOf(v1);
                if (index <= 0)
                    continue;
                var v0 = VAME_Manager.pathPoints[list.Key][index - 1];
                var newSegment = new PathLine(v0, v1);
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
                }
                VAME_Manager.pathLines[list.Key].Add(newSegment);
            }            
        }
        foreach (var h in VAME_Manager.pathHeights)
        {
            var index = VAME_Manager.pathHeights.IndexOf(h);
            if (index == 0) continue;
            averageHeight += h - VAME_Manager.pathHeights[index - 1];
        }
        averageHeight /= VAME_Manager.pathPoints.Count;
        if (VAME_Manager.slicerForm != null)
        {
            VAME_Manager.slicerForm.panel1.Invalidate();
        }
    }

    void scanGCD(string _line)
    {
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
            VAME_Manager.pathPoints.Add(y, new List<Vector3>());
        VAME_Manager.pathPoints[y].Add(newVertex);


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
