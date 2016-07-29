using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class TopMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    RectTransform container;
    bool isOpen;

    void Start()
    {
        container = transform.FindChild("Container").GetComponent<RectTransform>();
        var scale = container.localScale;
        scale.y = 0;
        container.localScale = scale;
        isOpen = false;
    }

    void Update()
    {
        Vector3 scale = container.localScale;
        scale.y = Mathf.Lerp(scale.y, isOpen ? 1 : 0, Time.deltaTime * 20);
        container.localScale = scale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isOpen = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isOpen = false;
    }

    public void SaveFile()
    {
        var sfd = new System.Windows.Forms.SaveFileDialog();
        sfd.Filter = "VAME File (.vme) | *.vme | Paths To DXF (.dxf) | *.dxf";
        //sfd.ShowDialog();
        if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var path = sfd.FileName;
            SaveVME(path);
        }
    }

    void SaveVME(string path)
    {
        var tw = new System.IO.StreamWriter(path);
        tw.WriteLine("VAME_File");
        #region MODEL
        tw.WriteLine("MODEL BEGIN");
        foreach (var _mesh in VAME_Manager.Meshes)
        {
            var mesh = _mesh.GetComponent<model>();
            tw.WriteLine("MESH BEGIN");
            tw.WriteLine("VertexCount|" + mesh.verts.Count.ToString());
            tw.WriteLine("VERTICES BEGIN");
            foreach (var p in mesh.verts)
            {
                tw.WriteLine(p.x.ToString("f5") + "|" + p.y.ToString("f5") + "|" + p.z.ToString("f5"));
            }
            tw.WriteLine("VERTICES END");

            tw.WriteLine("UVS BEGIN");
            foreach (var p in mesh.uvs)
            {
                tw.WriteLine(p.x.ToString("f5") + "|" + p.y.ToString("f5"));
            }
            tw.WriteLine("UVS END");

            tw.WriteLine("NORMALS BEGIN");
            foreach (var p in mesh.normals)
            {
                tw.WriteLine(p.x.ToString("f5") + "|" + p.y.ToString("f5") + "|" + p.z.ToString("f5"));
            }
            tw.WriteLine("NORMALS END");

            tw.WriteLine("MESH END");
        }
        tw.WriteLine("MODEL END");
        #endregion
        #region PATHS
        tw.WriteLine("PATHS BEGIN");
        foreach (var layer in VAME_Manager.pathLines.Values)
        {
            foreach (var line in layer)
            {
                tw.WriteLine("PATH BEGIN");
                var p1 = line.p1;
                var p2 = line.p2;
                tw.WriteLine(p1.x.ToString("f5") + "|" + p1.y.ToString("f5") + "|" + p1.z.ToString("f5"));
                tw.WriteLine(p2.x.ToString("f5") + "|" + p2.y.ToString("f5") + "|" + p2.z.ToString("f5"));
                tw.WriteLine("PATH END");
            }
        }
        tw.WriteLine("PATHS END");
        tw.WriteLine("VOXEL RESOLUTION|" + VAME_Manager.voxelResolution.ToString());
        tw.WriteLine("VAME END");
        #endregion
        tw.Close();
    }
}
