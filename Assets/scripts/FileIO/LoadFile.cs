using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;


public class LoadFile
{
	public LoadFile()
    {
        var ofd = new OpenFileDialog();
        ofd.InitialDirectory = UnityEngine.Application.dataPath + "/Models";
        ofd.Filter = "Model Files (*.stl, *.obj) | *.stl;*.obj";// ;*.STL;*.obj;*.OBJ";
        ofd.Filter += " | Path Files (*.gcd, *.job) | *.gcd;*.job";
        ofd.Filter += " | CCAT Files (*.cct) | *.cct";
        ofd.Filter += " | Virtual CCAT Files (*.vcct) | *.vcct";
        ofd.Filter += " | VAME Files (*.vme) | *.vme";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            LoadIt(ofd.FileName);            
        }
    }

    public void LoadIt(string n)
    {
        if (n.ToLower().EndsWith(".stl"))
        {
            VAME_Manager.instance.ClearModel();
            var i = new stlInterpreter(n);
            VAME_Manager._type = VAME_Manager.type.model;
            VAME_Manager.modelFileName = n;
            cSection.instance.DoSlices(20);
        }
        else if (n.ToLower().EndsWith(".obj"))
        {
            VAME_Manager.instance.ClearModel();
            var i = new objInterpreter(n);
            VAME_Manager._type = VAME_Manager.type.model;
            VAME_Manager.modelFileName = n;
        }
        else if (n.ToLower().EndsWith(".gcd"))
        {
            VAME_Manager.instance.ClearPaths();
            var i = new gcdInterpreter(n);
            VAME_Manager._type = VAME_Manager.type.paths;
            VAME_Manager.pathsFileName = n;
        }
        else if (n.ToLower().EndsWith(".job"))
        {
            VAME_Manager.instance.ClearPaths();
            var i = new jobInterpreter(n);
            VAME_Manager._type = VAME_Manager.type.paths;
            VAME_Manager.pathsFileName = n;
        }
        else if (n.ToLower().EndsWith(".cct"))
        {
            VAME_Manager.instance.ClearCCAT();
            var i = new ccatInterpreter(n, true);
            VAME_Manager._type = VAME_Manager.type.points;
            VAME_Manager.cctFileName = n;
        }
        else if (n.ToLower().EndsWith(".vcct"))
        {
            VAME_Manager.instance.ClearCCAT();
            var i = new ccatInterpreter(n, false);
            VAME_Manager._type = VAME_Manager.type.points;
            VAME_Manager.cctFileName = n;
        }
        else if (n.ToLower().EndsWith(".vme"))
        {
            var fInfo = new FileInfo(n);
            var t = "/" + System.DateTime.UtcNow.Ticks.ToString() + "_tmp";
            var newDir = Directory.CreateDirectory(fInfo.DirectoryName + t);
            VAME_Manager.tmpFolderName = fInfo.DirectoryName + t;

            var sr = new StreamReader(n);
            var full = sr.ReadToEnd();
            sr.Close();
            var del = new string[] { "||VAME||" };
            var none = "NONE";
            var split = full.Split(del, System.StringSplitOptions.None);
            //foreach (var ddd in split)
            //{
            //    MonoBehaviour.print(ddd);
            //}
            if (split.Length > 6)
            {
                var pathsCode = split[6];
                if (pathsCode != none)
                {
                    var pathsExtension = "." + split[5];
                    var pathsFileName = newDir.FullName + "/paths" + pathsExtension;
                    StreamWriter psr = File.CreateText(pathsFileName);
                    psr.Write(pathsCode);
                    psr.Close();
                    LoadIt(pathsFileName);
                }
            }
            if (split.Length > 3)
            {
                var cct = split[4];
                if (cct != none)
                {
                    var ccatFileName = newDir.FullName + "/ccat.cct";
                    StreamWriter csr = File.CreateText(ccatFileName);
                    csr.Write(cct);
                    csr.Close();
                    LoadIt(ccatFileName);
                }
            }
            if (split.Length > 2)
            {
                var model = split[3];
                if (model != none)
                {
                    var modelExtension = "." + split[2];
                    var modelFileName = newDir.FullName + "/model" + modelExtension;
                    StreamWriter msr = File.CreateText(modelFileName);
                    msr.Write(model);
                    msr.Close();
                    LoadIt(modelFileName);
                }
            }

            if (split.Length > 0)
            {
                int res = 0;
                int.TryParse(split[1], out res);
                if (res > 0)
                { 
                    VAME_Manager.instance.resolution.value = res;
                    VAME_Manager.instance.ResolutionChanged();
                    VAME_Manager.instance.DoSloxels();
                }
            }
        }

        if (Sloxelizer2.instance != null)
        {
            VAME_Manager.instance.DoSloxels();
        }
    }
}
