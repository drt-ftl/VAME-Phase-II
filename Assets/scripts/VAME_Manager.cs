using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class VAME_Manager : MonoBehaviour
{
    public static VAME_Manager instance;
    public Transform canvas;
    public GameObject yncDialog;
    public model modelMesh;
    public Transform modelHolder;
    public Material matFade;
    public Material matSolid;
    public Material matAmbiOcc;
    public Material pathMat;
    public Color pathColor;
    public float initialScale = 4;
    public enum type { model, paths, points }
    public static type _type;
    public static Vector3 Max;
    public static Vector3 Min;
    public static Vector3 PathsMax;
    public static Vector3 PathsMin;
    private Vector3 screenCenter;
    public Material glMat;
    public GameObject ball;
    public Transform voxelHolder;
    public Slider resolution;

    public static List<Triangle> triangleList = new List<Triangle>();
    public static List<Triangle> tempTriangleList = new List<Triangle>();
    public static List<GameObject> Meshes = new List<GameObject>();
    public static List<string> modelCode = new List<string>();
    public static List<string> pathsCode = new List<string>();
    public static Dictionary<float, List<PathLine>> pathLines = new Dictionary<float, List<PathLine>>();
    public static Dictionary<float, List<Vector3>> pathPoints = new Dictionary<float, List<Vector3>>();

    public static List<Vector3> gcdLineEndpoints = new List<Vector3>();
    public static List<PathLine> pl = new List<PathLine>();

    void Start()
    {
        screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
        //foreach (var pathList in pathPoints.Values)
        //{
        //    for ( int i = 0; i < pathList.Count; i++)
        //    {
        //        if (i == 0) continue;
        //        var p1 = (pathList[i - 1]);
        //        var p2 = (pathList[i]);
        //    }
        //    GL.End();
        //}
    }
    void OnGUI()
    {
        GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
        GL.modelview = Camera.main.worldToCameraMatrix;
        var col = pathColor;
        col.a = InspectorL.instance.pathVisibiltySlider.value;
        GL.Begin(GL.LINES);
        pathMat.SetPass(0);
        GL.Color(col);
        foreach (var list in pathLines)
        {
            foreach (var line in list.Value)
            {
                var p1 = (line.p1);
                var p2 = (line.p2);
                GL.Vertex(p1);
                GL.Vertex(p2);
            }
        }
        GL.End();

        if (InspectorL.isWireframe)
        {
            GL.Begin(GL.LINES);
            glMat.SetPass(0);
            GL.Color(new Color(0f, 0f, 0f, InspectorL.modelVisibility));
            foreach (var csLines in cSection.cSectionsX)
            {
                foreach (var l in csLines)
                {
                    GL.Vertex(l.p1);
                    GL.Vertex(l.p2);
                }
            }
            foreach (var csLines in cSection.cSectionsY)
            {
                foreach (var l in csLines)
                {
                    GL.Vertex(l.p1);
                    GL.Vertex(l.p2);
                }
            }
            GL.End();
        }
    }

    public void ClearAll()
    {
        ClearModel();
        ClearPaths();
    }

    public void ClearModel()
    {
        ClearVoxels();
        Max = Vector3.one * -100000;
        Min = Vector3.one * 100000;
        triangleList.Clear();
        tempTriangleList.Clear();
        foreach (var m in Meshes)
        {
            Destroy(m.gameObject);
        }
        Meshes.Clear();
        modelCode.Clear();
        InspectorL.instance.btnModel.interactable = false;
    }

    public void ClearPaths()
    {
        ClearVoxels();
        PathsMax = Vector3.one * -100000;
        PathsMin = Vector3.one * 100000;
        pathPoints.Clear();
        pathLines.Clear();
    }
    public void ClearVoxels()
    {
        if (Sloxelizer2.instance == null) return;
        foreach (var v in Sloxelizer2.instance.voxels)
        {
            Destroy(v.Cube);
        }
        Sloxelizer2.instance.voxels.Clear();
        Sloxelizer2.instance.sloxels.Clear();
    }

    public void LoadFile()
    {
        var lf = new LoadFile();
    }

    public void YNC(string _name, string _message)
    {
        var newYnc = Instantiate(yncDialog);
        var ync = newYnc.GetComponent<YesNoCancelDialog>();
        newYnc.transform.FindChild("Title").GetComponent<Text>().text = _name;
        newYnc.transform.FindChild("Inner").FindChild("Message").GetComponent<Text>().text = _message;
        ync.transform.SetParent(canvas);
        ync.GetComponent<RectTransform>().position = screenCenter;
    }

    public void ExitGame()
    {
        YNC("Exit VAME", "Are you sure?");
    }

    public void Generate()
    {
        //var splitter = new Splitter();
        NormalizeModel();
        Redraw();
        InspectorL.instance.btnModel.interactable = true;
        InspectorL.instance.btnModel.Select();
        InspectorL.instance.tglTrans.interactable = true;
        InspectorL.instance.tglWire.interactable = true;
        InspectorL.instance.btnModel.interactable = true;
        InspectorL.instance.visibiltySlider.interactable = true;
        InspectorL._mode = InspectorL.Mode.model;
        InspectorL.instance.codeDummy.SetHeight(modelCode.Count);
        InspectorL.instance.CodeArea(0);
        //var n = new Voxelizer(20);
    }

    public void SetMaxMin(Vector3 vert)
    {
        var max = Max;
        var min = Min;
        if (vert.x > Max.x) max.x = vert.x;
        if (vert.x < Min.x) min.x = vert.x;
        if (vert.y > Max.y) max.y = vert.y;
        if (vert.y < Min.y) min.y = vert.y;
        if (vert.z > Max.z) max.z = vert.z;
        if (vert.z < Min.z) min.z = vert.z;
        Max = max;
        Min = min;
    }

    public void Redraw()
    {
        var fullTempTriList = new List<Triangle>();
        foreach (var m in Meshes)
        {
            Destroy(m.gameObject);
        }
        Meshes.Clear();
        fullTempTriList = triangleList;// tempTriangleList;
        var maxPts = 20000;
        var numVerts = triangleList.Count * 3;
        var numMeshes = Mathf.CeilToInt((float)numVerts / maxPts);
        var tmp = new List<List<Triangle>>();
        for (int i = 0; i < numMeshes; i++)
        {
            var tl = new List<Triangle>();
            var vi = i * maxPts;
            for (int j = vi; j < vi + maxPts; j++)
            {
                if (fullTempTriList.Count > j)
                    tl.Add(fullTempTriList[j]);
            }
            tmp.Add(tl);
        }
        foreach (var l in tmp)
        {
            var mm = Instantiate(modelMesh) as model;
            mm.ClearAll();
            modelMesh.material = matSolid;
            foreach (var tri in l)
            {
                SetMaxMin(tri.p1);
                SetMaxMin(tri.p2);
                SetMaxMin(tri.p3);
                mm.AddTriangle(tri.p1, tri.p2, tri.p3, tri.norm, tri._binary);
            }
            mm.MergeMesh();
            mm.gameObject.transform.SetParent(modelHolder);
            mm.gameObject.transform.localPosition = Vector3.zero;
            mm.gameObject.transform.localEulerAngles = Vector3.zero;
            Meshes.Add(mm.gameObject);
        }
    }

    public void NormalizeModel()
    {
        var size = Max - Min;
        var biggest = Mathf.Max(size.x, size.y, size.z);
        var ratio = Mathf.Abs(initialScale / biggest);
        var c = ratio * (Max + Min) / 2.0f;
        //modelHolder.transform.position = -c;
        foreach (var t in triangleList)
        {
            t.p1 *= ratio;
            t.p2 *= ratio;
            t.p3 *= ratio;
            t.p1 -= c;
            t.p2 -= c;
            t.p3 -= c;
        }
        Max *= ratio;
        Max -= c;
        Min *= ratio;
        Min -= c;
    }

    public void NormalizePaths()
    {
        var size = PathsMax - PathsMin;
        var biggest = Mathf.Max(size.x, size.y, size.z);
        var ratio = Mathf.Abs(initialScale / biggest);
        var c = ((PathsMax + PathsMin) * ratio) / 2.0f;
        var newDict = new Dictionary<float, List<Vector3>>();
        foreach (var kvp in pathPoints)
        {
            var newKey = kvp.Key;
            newKey *= ratio;
            newKey -= c.y;
            newDict.Add(newKey, new List<Vector3>());            
            foreach (var p in kvp.Value)
            {
                var _p = p;
                _p *= ratio;
                _p  -= c;
                newDict[newKey].Add(_p);
            }            
        }
        pathPoints.Clear();
        pathPoints = newDict;
        PathsMax *= ratio;
        PathsMax -= c;
        PathsMin *= ratio;
        PathsMin -= c;
    }

    public void DoSloxels()
    {
        ClearVoxels();
        var sl = new Sloxelizer2((int)resolution.value);
        foreach (var voxel in sl.voxels)
        {
            var newVoxel = Instantiate(ball, voxel.origin, Quaternion.identity) as GameObject;
            newVoxel.transform.localScale = voxel.Scale * 0.99f;
            newVoxel.GetComponent<Info>().Activate(voxel, sl);
            newVoxel.transform.SetParent(voxelHolder);
            voxel.Cube = newVoxel;
        }
    }

    public void ResolutionChanged()
    {
        var res = resolution.value;
        resolution.gameObject.GetComponentInChildren<Text>().text = "Resolution: " + res.ToString("f0");
    }
}
