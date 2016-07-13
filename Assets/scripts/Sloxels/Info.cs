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

    public void Activate (Voxel _voxel, Sloxelizer2 _sl2)
    {
        voxel = _voxel;
        sl2 = _sl2;
        Voxel();
        activated = true;
    }

    void OnMouseDown()
    {
        if (!activated) return;
        gameObject.GetComponent<Renderer>().material = VoxelInspector.instance.selected;
        txt = "";
        foreach (var sloxel in voxel.Sloxels)
        {
            foreach (var v2 in sloxel.PathLines)
            {
                txt += "Line[" + v2.x.ToString("f4") + "][" + v2.y.ToString("f0") + "]\n";
            }
        }
    }

    void OnMouseOver()
    {
        txt = "";
        foreach (var sloxel in voxel.Sloxels)
        {
            foreach (var v2 in sloxel.PathLines)
            {
                txt += "Line[" + v2.x.ToString("f4") + "][" + v2.y.ToString("f0") + "]\n";
            }
        }
        VoxelInspector.instance.SetScrollText(txt);
        gameObject.GetComponent<MeshRenderer>().material = VoxelInspector.instance.highlight;
    }

    void OnMouseExit()
    {
        gameObject.GetComponent<MeshRenderer>().material = VoxelInspector.instance.standard;
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
