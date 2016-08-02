using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Info : MonoBehaviour
{
    private Voxel voxel;
    private Sloxelizer2 sl2;
    bool activated = false;
    string txt = "";
    public enum State { Standard, Highlighted, Selected }
    public State state;

    public void Activate (Voxel _voxel, Sloxelizer2 _sl2)
    {
        state = State.Standard;
        voxel = _voxel;
        sl2 = _sl2;
        Voxel();
        activated = true;
    }

    void Update()
    {
        var col = VoxelInspector.instance.standard.color;
        if (col.a < 0.1f && GetComponent<Renderer>().enabled && GetComponent<Collider>().enabled && this != VAME_Manager.instance.selectedVoxelInfo)
            SetActive(false);
        else if (col.a >= 0.1f && !GetComponent<Renderer>().enabled && !GetComponent<Collider>().enabled)
            SetActive(true);
    }

    public void SetActive(bool active)
    {
        if (active)
        {
            GetComponent<Renderer>().enabled = true;
            GetComponent<Collider>().enabled = true;
        }
        else
        {
            GetComponent<Renderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
        }
    }
    void OnMouseDown()
    {
        SelectVoxel(true);
    }

    void OnMouseOver()
    {
        if (this == VAME_Manager.instance.selectedVoxelInfo) return;
        gameObject.GetComponent<MeshRenderer>().material = VoxelInspector.instance.highlight;
        state = State.Highlighted;
    }

    void OnMouseExit()
    {
        if (this == VAME_Manager.instance.selectedVoxelInfo) return;
        gameObject.GetComponent<MeshRenderer>().material = VoxelInspector.instance.standard;
        state = State.Standard;
    }

    public void SelectVoxel(bool directClick)
    {
        if (!activated) return;
        if (VAME_Manager.instance.selectedVoxelInfo != null)
        {
            VAME_Manager.instance.selectedVoxelInfo.gameObject.GetComponent<MeshRenderer>().material = VoxelInspector.instance.standard;
        }
        VAME_Manager.instance.selectedVoxelInfo = this;
        var h = voxel.Sloxels[0].origin.y;
        var ind = VAME_Manager.pathHeights.IndexOf(h);
        if (directClick)
            VAME_Manager.slicerForm.ChangeLayer(ind, voxel);
        gameObject.GetComponent<Renderer>().material = VoxelInspector.instance.selected;
        txt = "";
        txt += "Voxel ID: " + Sloxelizer2.instance.voxels.IndexOf(voxel) + "\n\n";
        txt += "Contains " + voxel.Sloxels.Count.ToString() + " Voxels" + "\n";
        var i = 1;
        foreach (var sloxel in voxel.Sloxels)
        {
            var ht = sloxel.origin.y;
            txt += i.ToString() + ". " + "Sloxel[" + ht.ToString("f3") + "][" + Sloxelizer2.instance.sloxels[ht].IndexOf(sloxel).ToString() + "] \n";
            txt += "Containing " + sloxel.PathLines.Count.ToString() + " Path(s)\n";
            foreach (var v2 in sloxel.PathLines)
            {
                txt += "Path[" + v2.x.ToString("f3") + "][" + v2.y.ToString("f0") + "]\n";
            }
            i++;
        }
        VoxelInspector.instance.SetScrollText(txt);
        state = State.Selected;
        SetActive(true);
    }

    #region Cube
    Mesh mesh;
    private List<int> triangles = new List<int>();
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();
    private List<Color> colors = new List<Color>();
    private List<Vector2> uvs = new List<Vector2>();
    public Material material;
    public Color color;

    public void Voxel()
    {
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();

        mesh = GetComponent<MeshFilter>().mesh;
        gameObject.AddComponent<BoxCollider>();
        mesh.Clear();

        var rnd = GetComponent<MeshRenderer>();
        rnd.material = material;
        rnd.receiveShadows = true;
        rnd.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        AddCube();
    }
    public void AddCube()
    {
        var position = Vector3.zero;
        var half = Vector3.one / 2;
        var startPosition = position;
        var Plus = position + half;
        var Minus = position - half;
        Vector3 p1;
        Vector3 p2;
        Vector3 p3;
        Vector3 norm;

        position = Minus;

        position.x = Plus.x;
        p1 = position;
        position.x = Minus.x;
        p2 = position;
        position.y = Plus.y;
        p3 = position;
        norm = Normal(p1, p2, p3);
        AddTriangle(p1, p2, p3, norm);

        p1 = position;
        position.x = Plus.x;
        p2 = position;
        position.y = Minus.y;
        p3 = position;
        norm = Normal(p1, p2, p3);
        AddTriangle(p1, p2, p3, norm);

        p1 = position;
        position.y = Plus.y;
        p2 = position;
        position.z = Plus.z;
        p3 = position;
        norm = Normal(p1, p2, p3);
        AddTriangle(p1, p2, p3, norm);

        p1 = position;
        position.y = Minus.y;
        p2 = position;
        position.z = Minus.z;
        p3 = position;
        norm = Normal(p1, p2, p3);
        AddTriangle(p1, p2, p3, norm);

        p1 = position;
        position.z = Plus.z;
        p2 = position;
        position.x = Minus.x;
        p3 = position;
        norm = Normal(p1, p2, p3);
        AddTriangle(p1, p2, p3, norm);

        p1 = position;
        position.z = Minus.z;
        p2 = position;
        position.x = Plus.x;
        p3 = position;
        norm = Normal(p1, p2, p3);
        AddTriangle(p1, p2, p3, norm);

        //This One
        position = Plus;
        p1 = position;
        position.y = Minus.y;
        p2 = position;
        position.x = Minus.x;
        p3 = position;
        norm = Normal(p1, p3, p2);
        AddTriangle(p1, p3, p2, norm);

        //This One
        p1 = position;
        position.y = Plus.y;
        p2 = position;
        position.x = Plus.x;
        p3 = position;
        norm = Normal(p1, p3, p2);
        AddTriangle(p1, p3, p2, norm);

        p1 = position;
        position.z = Minus.z;
        p2 = position;
        position.x = Minus.x;
        p3 = position;
        norm = Normal(p1, p2, p3);
        AddTriangle(p1, p2, p3, norm);

        p1 = position;
        position.z = Plus.z;
        p2 = position;
        position.x = Plus.x;
        p3 = position;
        norm = Normal(p1, p2, p3);
        AddTriangle(p1, p2, p3, norm);

        position = Minus;
        p1 = position;
        position.z = Plus.z;
        p2 = position;
        position.y = Plus.y;
        p3 = position;
        norm = Normal(p1, p2, p3);
        AddTriangle(p1, p2, p3, norm);

        p1 = position;
        position.z = Minus.z;
        p2 = position;
        position.y = Minus.y;
        p3 = position;
        norm = Normal(p1, p2, p3);
        AddTriangle(p1, p2, p3, norm);

        MergeMesh();
    }

    public void AddTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 norm)
    {
        var count = triangles.Count;

        triangles.Add(count);
        triangles.Add(count + 1);
        triangles.Add(count + 2);
        vertices.Add(p1);
        vertices.Add(p2);
        vertices.Add(p3);
        uvs.Add(p1);
        uvs.Add(p2);
        uvs.Add(p3);
        for (int i = 0; i < 3; i++)
            normals.Add(norm);
        for (int i = 0; i < 3; i++)
            colors.Add(color);
    }

    public Vector3 Normal(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        var dir = Vector3.Cross(p2 - p1, p3 - p1);
        var norm = Vector3.Normalize(dir);
        return norm;
    }

    public void MergeMesh()
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();

        mesh.RecalculateNormals();
        mesh.Optimize();
    }
    #endregion
}
