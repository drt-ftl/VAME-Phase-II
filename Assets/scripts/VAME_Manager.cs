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
    public GameObject crazyBall;
    public Transform pointParent;
    public Transform voxelHolder;
    public Slider resolution;
    public Slider voxelVisibility;
    public Button voxelizerButton;
    public Info selectedVoxelInfo;
    public ccatBall selectedBall;
    public GameObject yncVeil;
    public static int voxelResolution;

    public static List<Triangle> triangleList = new List<Triangle>();
    public static List<Triangle> tempTriangleList = new List<Triangle>();
    public static List<GameObject> Meshes = new List<GameObject>();
    public static List<string> modelCode = new List<string>();
    public static List<string> pathsCode = new List<string>();
    public static List<string> cctCode = new List<string>();
    public static Dictionary<float, List<PathLine>> pathLines = new Dictionary<float, List<PathLine>>();
    public static List<PathLine> allPathLines = new List<PathLine>();
    public static Dictionary<float, List<pathPoint>> pathPoints = new Dictionary<float, List<pathPoint>>();
    public static List<GameObject> crazyBalls = new List<GameObject>();
    public static Dictionary<float, List<GameObject>> crazyBallsByLayer = new Dictionary<float, List<GameObject>>();
    public static List<float> pathHeights = new List<float>();
    public static float MinLayerToShow;
    public static float MaxLayerToShow;

    public static string modelFileName ="";
    public static string pathsFileName = "";
    public static string cctFileName = "";
    public static string tmpFolderName = "";
    public static float averagePathsHeight = 0;

    public static SlicerForm.SlicerForm slicerForm;

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
        //var col = pathColor;
        //col.a = InspectorL.instance.pathVisibiltySlider.value;
        GL.Begin(GL.LINES);
        pathMat.SetPass(0);
        //GL.Color(col);
        foreach (var line in allPathLines)
        {
            if (!line.LaserOn) continue;
            if (line.Show)
            {
                var col = line.Color;
                col.a = InspectorL.instance.pathVisibiltySlider.value;
                if (slicerForm != null)
                {
                    if (line.Layer == slicerForm.h)
                        col = Color.green;
                    if (line == slicerForm.selectedPathLine)
                        col = Color.blue;
                }
                GL.Color(col);
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
        ClearVoxels();
        ClearCCAT();
        tmpFolderName = "";
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
        modelFileName = "";
        InspectorL.instance.CodeArea(0);
    }

    public void ClearPaths()
    {
        InspectorL.instance.pathsMinSlider.interactable = false;
        InspectorL.instance.pathsMaxSlider.interactable = false;
        ClearVoxels();
        PathsMax = Vector3.one * -100000;
        PathsMin = Vector3.one * 100000;
        pathPoints.Clear();
        pathLines.Clear();
        pathsCode.Clear();
        pathHeights.Clear();
        pathsFileName = "";
        allPathLines.Clear();
        InspectorL.instance.btnPaths.interactable = false;
        InspectorL.instance.CodeArea(0);
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
        VoxelInspector.instance.SetScrollText("");
    }

    public void ClearCCAT()
    {
        if (ccatInterpreter.instance == null) return;
        foreach (var layer in crazyBallsByLayer.Values)
        {
            foreach (var cb in layer)
                Destroy(cb.gameObject);
        }
        crazyBalls.Clear();
        crazyBallsByLayer.Clear();
        cctFileName = "";
        InspectorL.instance.btnPoints.interactable = false;
        InspectorL.instance.CodeArea(0);
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

    public void ClearSession()
    {
        YNC("Clear Session", "Are you sure?");
    }

    void OnApplicationQuit()
    {
        if (System.IO.Directory.Exists(VAME_Manager.tmpFolderName))
            System.IO.Directory.Delete(VAME_Manager.tmpFolderName, true);
        if (slicerForm != null)
            slicerForm.Close();
    }
    public void Generate()
    {
        //var splitter = new Splitter();
        if (VAME_Manager.modelCode.Count == 0) return;
        NormalizeModel();
        Redraw();
        InspectorL.instance.btnModel.interactable = true;
        InspectorL.instance.btnModel.Select();
        InspectorL.instance.tglTrans.interactable = true;
        InspectorL.instance.tglWire.interactable = true;
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
        var newDict = new Dictionary<float, List<pathPoint>>();
        foreach (var kvp in pathPoints)
        {
            var newKey = kvp.Key;
            newKey *= ratio;
            newKey -= c.y;
            newDict.Add(newKey, new List<pathPoint>());            
            foreach (var p in kvp.Value)
            {
                var _p = p;
                _p.pp *= ratio;
                _p.pp  -= c;
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
        voxelResolution = (int)resolution.value;
        var sl = new Sloxelizer2(voxelResolution);
        if (Meshes.Count > 0)
            cSection.instance.SliceItGCD();
        foreach (var voxel in sl.voxels)
        {
            var newVoxel = Instantiate(ball, voxel.origin, Quaternion.identity) as GameObject;
            newVoxel.transform.localScale = voxel.Scale * 0.99f;
            newVoxel.GetComponent<Info>().Activate(voxel, sl);
            newVoxel.transform.SetParent(voxelHolder);
            voxel.Cube = newVoxel;
        }
        AssignCrazyBallsToSloxels();        
    }

    public void CrazyBalls()
    {
        var size = ccatInterpreter.instance.MaxVector - ccatInterpreter.instance.MinVector;
        var centroid = (ccatInterpreter.instance.MaxVector + ccatInterpreter.instance.MinVector) / 2f;
        var max = Mathf.Max(size.x, size.y, size.z);
        var scale = 4.0f / max;
        foreach (var point in ccatInterpreter.instance.points)
        {
            var cb = Instantiate(crazyBall, (point.Position - centroid) * scale, Quaternion.identity) as GameObject;
            cb.transform.SetParent(pointParent);
            var layer = VAME_Manager.pathHeights[0];
            foreach (var height in pathHeights)
            {
                if (Mathf.Abs((point.Position.y - centroid.y) * scale - height) < averagePathsHeight / 4.0f)
                {
                    layer = height;
                    break;
                }
            }
            if (!crazyBallsByLayer.ContainsKey(layer))
                crazyBallsByLayer.Add(layer, new List<GameObject>());
            crazyBallsByLayer[layer].Add(cb);
            crazyBalls.Add(cb);
        }
        //pointParent.position -= centroid * scale;
        foreach (var cb in crazyBalls)
        {
            var index = crazyBalls.IndexOf(cb);
            cb.GetComponent<ccatBall>().Activate(index, (float)index/ crazyBalls.Count);
        }

    }

    public void AssignCrazyBallsToSloxels()
    {
        if (Sloxelizer2.instance == null) return;
        foreach (var layer in Sloxelizer2.instance.sloxels)
        {
            foreach (var sloxel in layer.Value)
            {
                if (crazyBallsByLayer.ContainsKey(layer.Key))
                {
                    foreach (var crazyBall in crazyBallsByLayer[layer.Key])
                    {
                        if (Mathf.Abs(crazyBall.transform.position.x - sloxel.origin.x) < 2.0f / resolution.value &&
                            Mathf.Abs(crazyBall.transform.position.z - sloxel.origin.z) < 2.0f / resolution.value)
                        {
                            foreach (var line in sloxel.PathLines)
                            {
                                crazyBall.GetComponent<ccatBall>().TestLine(pathLines[line.x][(int)line.y]);
                            }


                            var cu = Color.white;
                            if (crazyBall.GetComponent<ccatBall>().Distance < 0)
                            {
                                crazyBall.GetComponent<ccatBall>().SetColor(new Color(0, 0, 0, 0));
                                crazyBall.GetComponent<Collider>().enabled = false;
                            }
                            else
                            {
                                var d = crazyBall.GetComponent<ccatBall>().Distance;
                                d /= ccatInterpreter.instance.MaxDistanceError;
                                cu.a = 1.0f;
                                cu.r = d;
                                cu.g = 1.0f - d;
                                cu.b = 0f;
                                crazyBall.GetComponent<ccatBall>().SetColor(cu);
                            }
                            sloxel.ccatBalls.Add(crazyBall);
                        }                        
                    }
                }
            }
        }        
    }
    public void ResolutionChanged()
    {
        var res = resolution.value;
        resolution.gameObject.GetComponentInChildren<Text>().text = "Resolution: " + res.ToString("f0");
    }

    public void VoxelVisibiltyChanged()
    {
        var vis = voxelVisibility.value;
        var col = VoxelInspector.instance.baseColor;
        col.a = vis;
        VoxelInspector.instance.baseColor = col;
        //var col = VoxelInspector.instance.standard.color;
        //col.a = vis;
        VoxelInspector.instance.standard.color = col;

        //vis = (0.7f * Mathf.Pow(vis, 0.1f)) + 0.3f;
        //col = VoxelInspector.instance.highlight.color;
        //col.a = vis;
        //VoxelInspector.instance.highlight.color = col;

        //col = VoxelInspector.instance.selected.color;
        //col.a = vis;
        //VoxelInspector.instance.selected.color = col;
        voxelVisibility.GetComponentInChildren<Text>().text = "Visibility: " + (100 * vis).ToString("f0") + "%";
    }

    public float ShortestDistanceBetweenTwoLines(Vector3 p1, Vector3 p2, Vector3 _p1, Vector3 _p2)
    {
        var A1 = p2.z - p1.z;
        var B1 = p1.x - p2.x;
        var C1 = A1 * p1.x + B1 * p1.z;

        var A2 = _p2.z - _p1.z;
        var B2 = _p1.x - _p2.x;
        var C2 = A2 * _p1.x + B2 * _p1.z;

        float delta = A1 * B2 - A2 * B1;
        if (delta == 0)
        {
            return ShortestDistanceBetweenPointAndLine(p1, _p1, _p2, true);
        }
        var M1 = p2 - p1;
        var M2 = _p2 - _p1;
        var p = new Vector3();
        p.x = (B2 * C1 - B1 * C2) / delta;
        p.y = p1.y;
        p.z = (A1 * C2 - A2 * C1) / delta;
        var t1 = 0f;
        var t2 = 0f;
        var closestPoint1 = Vector3.zero;
        var closestPoint2 = Vector3.zero;

        if (M1.x != 0) t1 = (p.x - p1.x) / M1.x;
        else if (M1.z != 0) t1 = (p.z - p1.z) / M1.z;
        else return -1;

        if (t1 <= 0)            closestPoint1 = p1;
        else if (t1 >= 1)       closestPoint1 = p2;
        else                    closestPoint1 = p1 + t1 * M1;

        if (M2.x != 0)          t2 = (p.x - _p1.x) / M2.x;
        else if (M2.z != 0)     t2 = (p.z - _p1.z) / M2.z;
        else return -1;

        if (t2 <= 0) closestPoint2 = _p1;
        else if (t2 >= 1) closestPoint2 = _p2;
        else closestPoint2 = _p1 + t2 * M2;
        var d1 = ShortestDistanceBetweenPointAndLine(closestPoint1, _p1, _p2, false);
        var d2 = ShortestDistanceBetweenPointAndLine(closestPoint2, p1, p2, false);
        var min = Mathf.Min(d1, d2);
        return min;
    }

    public float ShortestDistanceBetweenPointAndLine (Vector3 pt, Vector3 p1, Vector3 p2, bool infinite)
    {
        float dx = p2.x - p1.x;
        float dz = p2.z - p1.z;
        Vector3 closest;

        if ((dx == 0) && (dz == 0))
        {
            // It's a point not a line segment.
            closest = p1;
            dx = pt.x - p1.x;
            dz = pt.z - p1.z;
            return -1;
        }

        // Calculate the t that minimizes the distance.
        float t = ((pt.x - p1.x) * dx + (pt.z - p1.z) * dz) /
            (dx * dx + dz * dz);

        // See if this represents one of the segment's
        // end points or a point in the middle.
        if (t < 0 && !infinite)
        {
            closest = p1;
            dx = pt.x - p1.x;
            dz = pt.z - p1.z;
        }
        else if (t > 1 && !infinite)
        {
            closest = p2;
            dx = pt.x - p2.x;
            dz = pt.z - p2.z;
        }
        else
        {
            closest = new Vector3(p1.x + t * dx, p1.y,  p1.z + t * dz);
            dx = pt.x - closest.x;
            dz = pt.z - closest.z;
        }

        return Mathf.Sqrt(dx * dx + dz * dz);
    }
}
