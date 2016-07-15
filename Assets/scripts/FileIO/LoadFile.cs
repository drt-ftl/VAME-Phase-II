using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

public class LoadFile
{
	public LoadFile()
    {
        var ofd = new OpenFileDialog();
        ofd.InitialDirectory = UnityEngine.Application.dataPath + "/Models";
        ofd.Filter = "Model Files (*.stl, *.obj) | *.stl";// ;*.STL;*.obj;*.OBJ";
        ofd.Filter += " | Path Files (*.gcd) | *.gcd";
        ofd.Filter += " | CCAT Files (*.cct) | *.cct";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            var n = ofd.FileName;
            if (n.ToLower().EndsWith(".stl"))
            {
                VAME_Manager.instance.ClearModel();
                var i = new stlInterpreter(n);
                VAME_Manager._type = VAME_Manager.type.model;
            }
            else if (n.ToLower().EndsWith(".obj"))
            {
                VAME_Manager.instance.ClearModel();
                var i = new objInterpreter(n);
                VAME_Manager._type = VAME_Manager.type.model;
            }
            else if (n.ToLower().EndsWith(".gcd"))
            {
                VAME_Manager.instance.ClearPaths();
                var i = new gcdInterpreter(n);
                VAME_Manager._type = VAME_Manager.type.paths;
            }
            else if (n.ToLower().EndsWith(".cct"))
            {
                //VAME_Manager.instance.ClearPaths();
                MonoBehaviour.print("CCAT!");
                var i = new ccatInterpreter(n);
                VAME_Manager._type = VAME_Manager.type.points;
            }
        }
    }
}
