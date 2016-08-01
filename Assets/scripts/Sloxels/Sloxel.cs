using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sloxel
{
    public Sloxel(Vector3 _origin, List<Vector2> _pathLines, float increment)
    {
        origin = _origin;
        PathLines = new List<Vector2>(_pathLines);
        Scale = new Vector3(increment, 0.1f, increment);
        ccatBalls = new List<GameObject>();
    }

    public Vector3 origin           { get; internal set; }
    public List<Vector2> PathLines  { get; internal set; }
    public List<GameObject> ccatBalls { get; set; }
    public Vector3 Scale            { get; internal set; }
    public Voxel Voxel { get; set; }
    public List<float> distances { get; internal set; }
    public float MinSeparation { get; internal set; }
    public float MaxSeparation { get; internal set; }
    public float MedianSeparation { get; internal set; }
    public float MeanSeparation { get; internal set; }

    public void SetDistances()
    {
        distances = new List<float>();
        var done = new List<PathLine>();
        foreach (var key in PathLines)
        {
            var line = VAME_Manager.pathLines[key.x][(int)key.y];
            if (done.Contains(line)) continue;
            foreach (var _key in PathLines)
            {
                var _line = VAME_Manager.pathLines[_key.x][(int)_key.y];
                if (_line.p1 == line.p1 || _line.p1 == line.p2 ||
                    _line.p2 == line.p1 || _line.p2 == line.p2)
                    continue;
                if (_line == line) continue;
                var d = VAME_Manager.instance.ShortestDistanceBetweenTwoLines(line.p1, line.p2, _line.p1, _line.p2);
                if (d == -1) continue;
                distances.Add(d);
            }
            done.Add(line);
        }
        MinSeparation = Mathf.Min(distances.ToArray());
        MaxSeparation = Mathf.Max(distances.ToArray());
        var running = 0f;
        foreach (var d in distances) running += d;
        if (distances.Count > 0)
        {
            MeanSeparation = running / distances.Count;
            if (distances.Count % 2 == 1)
            {
                var i = (int)(distances.Count / 2f);
                MedianSeparation = distances[i];
            }
            else
            {
                var i = (int)(distances.Count / 2f);
                var _i = i - 1;
                MedianSeparation = (distances[i] + distances[_i]) / 2.0f;
            }

        }
        else
        {
            MeanSeparation = 0;
            MedianSeparation = 0;            
        }
    }
}
