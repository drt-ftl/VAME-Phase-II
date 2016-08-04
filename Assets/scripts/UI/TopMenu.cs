using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using System.IO;

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

    #region Save
    public void SaveFile()
    {
        var sfd = new System.Windows.Forms.SaveFileDialog();
        sfd.Filter = "VAME File (.vme) | *.vme | All Paths To DXF (.dxf) | *.dxf | Selected Paths To DXF (.dxf) | *.dxf";
        //sfd.ShowDialog();
        if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var path = sfd.FileName;
            //if (path.ToLower().EndsWith(".vme"))
                SaveVME(path);
            //else if (sfd.FilterIndex == 1)
                //SaveDXF(path, true);
            //else if (sfd.FilterIndex == 2)
            //    SaveDXF(path, false);
        }
    }

    void SaveVME(string path)
    {
        StreamReader sr;
        var del = "||VAME||";                                                                   // Split on ||VAME||
        var none = "NONE";
        var tw = new System.IO.StreamWriter(path);
        tw.Write("VAME_File\n");
        var res = VAME_Manager.instance.resolution;
        tw.WriteLine("VOXEL RESOLUTION");
        tw.Write(del);
        if (Sloxelizer2.instance != null && Sloxelizer2.instance.voxels.Count > 0)
            tw.Write(VAME_Manager.instance.resolution.ToString());                              // Voxel Resolution = split[1]
        else tw.Write("A");
        tw.Write(del);
        if (VAME_Manager.modelFileName.ToLower().EndsWith(".obj") || VAME_Manager.modelFileName.ToLower().EndsWith(".stl"))
        {
            var modelExtension = "stl";
            if (VAME_Manager.modelFileName.ToLower().EndsWith(".obj"))
                modelExtension = "obj";
            tw.Write(modelExtension);                                                               // ModelFileType = split[2]
            tw.Write(del);
            sr = new StreamReader(VAME_Manager.modelFileName);                                  // Model Code = split[3];
            tw.Write(sr.ReadToEnd());
            sr.Close();
        }
        else
        {
            tw.Write(none);
            tw.Write(del);
            tw.Write(none);
        }

        tw.Write(del);

        if (VAME_Manager.cctFileName.ToLower().EndsWith(".cct") || VAME_Manager.cctFileName.ToLower().EndsWith(".vcct"))
        {
            sr = new StreamReader(VAME_Manager.cctFileName);                                        // CCAT Code = split[4]
            tw.Write(sr.ReadToEnd());
            sr.Close();
        }
        else
        {
            tw.Write(none);
        }

        tw.Write(del);

        if (VAME_Manager.pathsFileName.ToLower().EndsWith(".gcd") || VAME_Manager.pathsFileName.ToLower().EndsWith(".job"))
        {
            var pathsExtension = "gcd";
            if (VAME_Manager.pathsFileName.ToLower().EndsWith(".job"))
                pathsExtension = "job";
            tw.Write(pathsExtension);                                                               // PathsFileType = split[5]
            tw.Write(del);
            sr = new StreamReader(VAME_Manager.pathsFileName);                                      // Paths Code = split[6]
            tw.Write(sr.ReadToEnd());
            sr.Close();
        }
        else
        {
            tw.Write(none);
            tw.Write(del);
            tw.Write(none);
        }
        tw.Close();
    }

    public void SaveDXF(string path, bool isFull)
    {
        var saveDxf = new SaveDXF();
        if (isFull)
            saveDxf.ConvertFull(VAME_Manager.allPathLines);
        else
        {
            var lines = new System.Collections.Generic.List<PathLine>();
            foreach(var line in VAME_Manager.allPathLines)
            {
                if (line.Show)
                    lines.Add(line);
            }
            saveDxf.ConvertFull(lines);
        }
        var str = saveDxf.GetDxf;
        StreamWriter w = File.CreateText(path);
        w.Write(str);
        w.Close();
    }
    #endregion
}
