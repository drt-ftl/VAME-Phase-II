﻿using UnityEngine;
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
        sfd.Filter = "VAME File (.vme) | *.vme | Paths To DXF (.dxf) | *.dxf";
        //sfd.ShowDialog();
        if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var path = sfd.FileName;
            if (path.ToLower().EndsWith(".dxf"))
                SaveDXF(path);
            if (path.ToLower().EndsWith(".vme"))
                SaveVME(path);
        }
    }

    void SaveVME(string path)
    {
        var del = "||VAME||";                                                                   // Split on ||VAME||
        var tw = new System.IO.StreamWriter(path);
        tw.Write("VAME_File\n");
        var res = VAME_Manager.instance.resolution;
        tw.WriteLine("VOXEL RESOLUTION");
        tw.Write(del);
        if (Sloxelizer2.instance.voxels.Count > 0)
            tw.Write(VAME_Manager.instance.resolution.ToString());                              // Voxel Resolution = split[1]
        else tw.Write("A");
        tw.Write(del);
        var modelExtension = "stl";
        if (VAME_Manager.modelFileName.ToLower().EndsWith(".obj"))
            modelExtension = "obj";
        tw.Write(modelExtension);                                                               // ModelFileType = split[2]
        tw.Write(del);
        var sr = new StreamReader(VAME_Manager.modelFileName);                                  // Model Code = split[3];
        tw.Write(sr.ReadToEnd());
        sr.Close();
        tw.Write(del);
        sr = new StreamReader(VAME_Manager.cctFileName);                                        // CCAT Code = split[4]
        tw.Write(sr.ReadToEnd());
        sr.Close();
        tw.Write(del);
        sr = new StreamReader(VAME_Manager.pathsFileName);                                      // Paths Code = split[5]
        tw.Write(sr.ReadToEnd());
        sr.Close();
        tw.Close();
    }

    public void SaveDXF(string path)
    {        
        StreamWriter w = File.CreateText(path);
        w.WriteLine(path);
        foreach (var layer in VAME_Manager.pathLines)
        {
            foreach (var line in layer.Value)
            {
                //var s = 0.25f;
                //var p1 = line.p1 / s;
                //var p2 = t.p2 / s;
                //var p3 = t.p3 / s;
                //w.WriteLine("facet normal " + t.norm.x + " " + t.norm.y + " " + t.norm.z);
                //w.WriteLine("outer loop");
                //w.WriteLine("vertex " + p1.x + " " + p1.y + " " + p1.z);
                //w.WriteLine("vertex " + p2.x + " " + p2.y + " " + p2.z);
                //w.WriteLine("vertex " + p3.x + " " + p3.y + " " + p3.z);
                //w.WriteLine("endloop");
                //w.WriteLine("endfacet");
            }
        }
        w.WriteLine("endsolid");
        w.Close();
    }
    #endregion
}
