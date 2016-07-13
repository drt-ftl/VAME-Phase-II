using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

public class testLoader : MonoBehaviour
{

    public static Dictionary<float, List<Vector3>> pathPoints = new Dictionary<float, List<Vector3>>();
    public Color pathColor;
    public Material pathMat;
    public Transform modelHolder;

	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    void OnGUI()
    {
        foreach (var pathList in pathPoints.Values)
        {
            GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
            GL.modelview = Camera.main.worldToCameraMatrix;
            var col = pathColor;
            //col.a = InspectorL.instance.pathVisibiltySlider.value;
            GL.Begin(GL.LINES);
            pathMat.SetPass(0);
            GL.Color(col);
            foreach (var vertex in pathList)
            {
                if (pathList.IndexOf(vertex) == 0) continue;
                var p1 = modelHolder.TransformPoint(pathList[pathList.IndexOf(vertex) - 1]);
                var p2 = modelHolder.TransformPoint(vertex);
                GL.Vertex(p1);
                GL.Vertex(p2);
            }
            GL.End();
        }
    }

    public void LoadIt()
    {
        var ofd = new OpenFileDialog();
        ofd.InitialDirectory = UnityEngine.Application.dataPath + "/Models";
        ofd.Filter = "Model Files (*.stl, *.obj) | *.stl";// ;*.STL;*.obj;*.OBJ";
        ofd.Filter += " | Path Files (*.gcd) | *.gcd";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            var n = ofd.FileName;
            var gcdI = new GcdInterpreter();
            var reader = new StreamReader(n);
            while (!reader.EndOfStream)
            {
                var r = reader.ReadLine();
                scanGCD(r);
            }
            reader.Close();
        }
    }

    void scanGCD(string _line)
    {
        if (_line == "\r\n") return;
        _line = _line.Trim();
        //gcdCode.Add(_line.ToString() + "\r\n");
        if (!_line.Contains(" ")) return;
        var chunks = _line.Split(' ');
        switch (chunks[0][0])
        {
            case 'G':
                if (!chunks[1].StartsWith("F"))
                {
                    GcdInterpreter.instance.StartsWithG(_line);
                }
                break;
            case 'O':
                if (chunks[0][1] == 'U')
                {
                    if (chunks.Length > 1 && chunks[1].Contains(","))
                    {
                        var command = chunks[1].Split(',');
                        if (command[1] == "0")
                            GcdInterpreter.instance.LaserOn = false;
                        else GcdInterpreter.instance.LaserOn = true;
                    }
                }
                //    gcdInterpreter.StartsWithO(_line);
                break;
            default:
                break;
        }
    }
}
