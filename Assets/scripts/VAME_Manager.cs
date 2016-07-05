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
    public RectTransform viewportRect;
    public float initialScale = 4;
    public static Vector3 viewportOrigin;
    public enum type { model, paths, points }
    public static type _type;
    public static Vector3 Max;
    public static Vector3 Min;
    public static Vector3 PathsMax;
    public static Vector3 PathsMin;
    private Vector3 screenCenter;
    public Material glMat;

    public static List<Triangle> triangleList = new List<Triangle>();
    public static List<Triangle> tempTriangleList = new List<Triangle>();
    public static List<GameObject> Meshes = new List<GameObject>();
    public static List<string> modelCode = new List<string>();
    public static List<string> pathsCode = new List<string>();
    public static List<PathLine> gcdPathLines = new List<PathLine>();

    void Start ()
    {
        screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        instance = this;
	}

	void OnGUI ()
    {
        RectTransformExtensions.SetWidth(viewportRect, Screen.width - 411);
        GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
        GL.modelview = Camera.main.worldToCameraMatrix;

        foreach (var gcdLine in gcdPathLines)
        {
            var p1 = modelHolder.TransformPoint(gcdLine.p1);
            var p2 = modelHolder.TransformPoint(gcdLine.p2);
            var col = pathColor;
            col.a = InspectorL.instance.pathVisibiltySlider.value;
            GL.Begin(GL.LINES);
            pathMat.SetPass(0);
            GL.Color(col);
            GL.Vertex(p1);
            GL.Vertex(p2);
            GL.End();
        }

        if (InspectorL.isWireframe)
        {
            foreach (var csLines in cSection.cSections)
            {
                foreach (var l in csLines)
                {
                    var p1 = modelHolder.TransformPoint(l.p1);
                    var p2 = modelHolder.TransformPoint(l.p2);
                    GL.Begin(GL.LINES);
                    glMat.SetPass(0);
                    GL.Color(new Color(0f, 0f, 0f, InspectorL.modelVisibility));
                    GL.Vertex(p1);
                    GL.Vertex(p2);
                    GL.End();
                }
            }
        }
    }

    public void ClearAll()
    {
        ClearModel();
        ClearPaths();
    }

    public void ClearModel()
    {
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
        PathsMax = Vector3.one * -100000;
        PathsMin = Vector3.one * 100000;
        gcdPathLines.Clear();
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
        cSection.instance.DoSlices(20);
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
        Min *= ratio;
    }

    public void NormalizePaths()
    {
        var size = PathsMax - PathsMin;
        var biggest = Mathf.Max(size.x, size.y, size.z);
        var ratio = Mathf.Abs(initialScale / biggest);
        var c = ((PathsMax + PathsMin) * ratio) / 2.0f;
        foreach (var l in gcdPathLines)
        {
            l.p1 *= ratio;
            l.p1 -= c;
            l.p2 *= ratio;
            l.p2 -= c;
        }
        PathsMax *= ratio;
        PathsMin *= ratio;
    }
}
