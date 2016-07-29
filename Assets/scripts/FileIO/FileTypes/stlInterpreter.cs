using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;

public class stlInterpreter
{
    private int num_facets = 0;
    private float divisor = 10000;
    float stlScale = 65f;
    int _tCount = 0;
    int _vCount = 0;

    private List<Vector3> currentVertices = new List<Vector3>();
    private Vector3 Normal = Vector3.zero;

    public stlInterpreter(string _filename)
    {
        if (CheckForStlBinary(_filename))
            Parse_StlBinary(_filename);
        else ParseStlAscii(_filename);
        if (VAME_Manager.slicerForm != null)
            VAME_Manager.slicerForm.panel1.Invalidate();

    }

    public void ClearAll()
    {
        num_facets = 0;
        divisor = 10000;
        stlScale = 65f;
        _tCount = 0;
        _vCount = 0;
        currentVertices.Clear();
        Normal = Vector3.zero;
        VAME_Manager.modelCode.Clear();
    }

    public bool CheckForStlBinary(string _path)
    {
        var _isBinary = false;
        var readBytesArray = File.ReadAllBytes(_path);
        var numBytes = readBytesArray.Length;
        if (numBytes < 84)
            return false;
        var numFacets = new byte[4];
        for (int i = 80; i < 84; i++)
        {
            numFacets[i - 80] = readBytesArray[i];
        }
        var num_facets = BitConverter.ToInt32(numFacets, 0);
        var predictedNumBytes = (84 + 50 * num_facets);
        if (numBytes == predictedNumBytes)
        {
            _isBinary = true;
        }
        else
        {
            _isBinary = false;
        }
        return _isBinary;
    }

    #region Binary
    void Parse_StlBinary(string _path)
    {
        var readBytesArray = File.ReadAllBytes(_path);
        var l = new List<byte>();
        foreach (var b in readBytesArray)
        {
            l.Add(b);
        }
        var numFacetBytes = new byte[4];
        for (int i = 80; i < 84; i++)
        {
            numFacetBytes[i - 80] = l[i];
        }
        num_facets = BitConverter.ToInt32(numFacetBytes, 0);
        var sPos = 84;
        var chunk = new List<byte>();
        for (int facet = 0; facet < num_facets; facet++)
        {
            if (l.Count > sPos + 50)
            {
                chunk.Clear();
                for (int vector3 = sPos; vector3 < sPos + 50; vector3++)
                {
                    chunk.Add(l[vector3]);
                }//Makes chunk of 50
                var triangle = chunk.ToArray();
                var str = BitConverter.ToString(chunk.ToArray());
                ParseBinaryVertices(triangle);
                sPos += 50;
            }
        }
        VAME_Manager.instance.Generate();
    }

    public void ParseBinaryVertices(byte[] triangle)
    {
        var sPos = 0;
        var chunk = new byte[4];

        var p = new Vector3[3];

        for (int i = sPos; i < sPos + 4; i++)
        {
            chunk[i - sPos] = triangle[i];
        }
        var n_i = BitConverter.ToSingle(chunk, 0);
        sPos += 4;

        for (int i = sPos; i < sPos + 4; i++)
        {
            chunk[i - sPos] = triangle[i];
        }
        var n_j = BitConverter.ToSingle(chunk, 0);
        sPos += 4;

        for (int i = sPos; i < sPos + 4; i++)
        {
            chunk[i - sPos] = triangle[i];
        }
        var n_k = BitConverter.ToSingle(chunk, 0);
        sPos += 4;

        var normal = new Vector3(n_i, n_j, n_k);
        normal = normal / divisor;
        for (int ii = 0; ii < 3; ii++)
        {
            for (int i = sPos; i < sPos + 4; i++)
            {
                chunk[i - sPos] = triangle[i];
            }
            var x = (float)BitConverter.ToSingle(chunk, 0);
            sPos += 4;

            for (int i = sPos; i < sPos + 4; i++)
            {
                chunk[i - sPos] = triangle[i];
            }
            var z = (float)BitConverter.ToSingle(chunk, 0);
            sPos += 4;

            for (int i = sPos; i < sPos + 4; i++)
            {
                chunk[i - sPos] = triangle[i];
            }
            var y = (float)BitConverter.ToSingle(chunk, 0);
            sPos += 4;

            p[ii] = new Vector3(x, y, z);
        }

        var t = new Triangle(p[0], p[1], p[2], normal, true);
        VAME_Manager.triangleList.Add(t);
        VAME_Manager.instance.SetMaxMin(p[0]);
        VAME_Manager.instance.SetMaxMin(p[1]);
        VAME_Manager.instance.SetMaxMin(p[2]);
    }
    #endregion

    #region ASCII
    void ParseStlAscii (string fileName)
    {
        var reader = new StreamReader(fileName);
        while (!reader.EndOfStream)
        {
            string line = reader.ReadToEnd();
            line = line.Replace("facet", "|facet");
            line = line.Replace("outer loop", "|outer loop");
            line = line.Replace("endloop", "|endloop");
            line = line.Replace("vertex", "|vertex");
            line = line.Replace("endfacet", "|endfacet");
            var _lines = line.Split('|');
            foreach (var _line in _lines)
            {
                VAME_Manager.modelCode.Add(_line);
            }
        }
        reader.Close();
        foreach (var l in VAME_Manager.modelCode)
        {
            scanSTL(l);
        }
        VAME_Manager.instance.Generate();
    }

    public void scanSTL(string _line)
    {
        _line = _line.Trim();
        if (_line.Contains("outer"))
        {
            outerloop();
        }
        else if (_line.Contains("endloop"))
        {
            endloop(_line);
        }
        else if (_line.Contains("vertex"))
        {
            vertex(_line);
        }

        else if (_line.Contains("normal"))
        {
            normal(_line);
        }
    }

    public void normal(string _line)
    {
        float x;
        float y;
        float z;
        var split = _line.Split('l');
        split[1].TrimStart(' ');
        var coords = split[1].Split(' ');
        var xString = coords[0];
        var yString = coords[1];
        var zString = coords[2];
        var xStrSplit = xString.Split('e');
        if (float.TryParse(xStrSplit[0], out x))
        {
            float xE;
            if (xStrSplit.Length > 1 && float.TryParse(xStrSplit[1], out xE))
                x *= (Mathf.Pow(10f, xE));
        }

        var yStrSplit = yString.Split('e');
        if (float.TryParse(yStrSplit[0], out y))
        {
            float yE;
            if (yStrSplit.Length > 1 && float.TryParse(yStrSplit[1], out yE))
                y *= (Mathf.Pow(10f, yE));
        }

        var zStrSplit = zString.Split('e');
        if (float.TryParse(zStrSplit[0], out z))
        {
            float zE;
            if (zStrSplit.Length > 1 && float.TryParse(zStrSplit[1], out zE))
                z *= (Mathf.Pow(10f, zE));
        }
        Normal = new Vector3(x, y, z);
    }

    public void outerloop()
    {
    }

    public void endloop(string _line)
    {
        try
        {
            var p = currentVertices;
            //mm.AddTriangle(p[0], p[1], p[2], Normal, false);
            var t = new Triangle(p[0], p[1], p[2], Normal, false);
            VAME_Manager.triangleList.Add(t);
            VAME_Manager.instance.SetMaxMin(p[0]);
            VAME_Manager.instance.SetMaxMin(p[1]);
            VAME_Manager.instance.SetMaxMin(p[2]);
            currentVertices.Clear();
        }
        catch { }
    }

    public void vertex(string _line)
    {
        {
            float x;
            float y;
            float z;
            var coordSep = _line.Split('x');
            var coords = coordSep[1].TrimStart(' ').Split(' ');

            var xString = coords[0];
            var yString = coords[1];
            var zString = coords[2];
            var xStrSplit = xString.Split('e');
            if (float.TryParse(xStrSplit[0], out x))
            {
                float xE;
                if (xStrSplit.Length > 1 && float.TryParse(xStrSplit[1], out xE))
                    x *= (Mathf.Pow(10f, xE));
            }

            var yStrSplit = yString.Split('e');
            if (float.TryParse(yStrSplit[0], out y))
            {
                float yE;
                if (yStrSplit.Length > 1 && float.TryParse(yStrSplit[1], out yE))
                    y *= (Mathf.Pow(10f, yE));
            }

            var zStrSplit = zString.Split('e');
            if (float.TryParse(zStrSplit[0], out z))
            {
                float zE;
                if (zStrSplit.Length > 1 && float.TryParse(zStrSplit[1], out zE))
                    z *= (Mathf.Pow(10f, zE));
            }
            var newVertex = new Vector3(x, y, z) * stlScale;
            //VAME_Manager.instance.SetMaxMin(newVertex);
            currentVertices.Add(newVertex);
        }
    }
    #endregion
}
