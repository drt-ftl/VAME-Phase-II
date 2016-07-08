using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UnityEngine;
using System.IO;

public class gcdInterpreter
{
    public static float y = 0;
    public Vector3 Min { get; set; }
    public Vector3 Max { get; set; }
    public bool LaserOn = false;
    private Vector3 lastPoint;

    public gcdInterpreter(string _filename)
    {
        y = 0;
        Min = Vector3.one * 10000;
        Max = Vector3.one * -10000;

        var reader = new StreamReader(_filename);
        while (!reader.EndOfStream)
        {
            var s = reader.ReadToEnd();
            //s = s.Replace("  ", " ");
            //s = s.Replace("  ", " ");
            string[] lines = s.Split("\n"[0]);
            foreach (string item in lines)
            {
                scanGCD(item);
            }
        }
        VAME_Manager.PathsMax = Max;
        VAME_Manager.PathsMin = Min;
        VAME_Manager.instance.NormalizePaths();
        InspectorL.instance.btnPaths.interactable = true;
        InspectorL.instance.pathVisibiltySlider.interactable = true;
        InspectorL._mode = InspectorL.Mode.paths;
        InspectorL.instance.codeDummy.SetHeight(VAME_Manager.pathsCode.Count);
        InspectorL.instance.CodeArea(0);
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
        if (LaserOn) return;
        if (_chunks[1].Contains('F')) return;
        if (_chunks[1].StartsWith("Z"))
        {
            var Y = _chunks[1].TrimStart('Z');
            float _y;
            if (float.TryParse(Y, out _y))
            {
                y = _y;
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
        if (lastPoint == Vector3.one * 10000)
        {
            lastPoint = newVertex;
        }
        else
        {
            if (!VAME_Manager.gcdPathLines.ContainsKey(y))
            {
                VAME_Manager.gcdPathLines.Add(y, new List<PathLine>());
            }
            VAME_Manager.gcdPathLines[y].Add(new PathLine(lastPoint, newVertex));
            lastPoint = Vector3.one * 10000;
        }
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
