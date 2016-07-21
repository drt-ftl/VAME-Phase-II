using UnityEngine;
using System.Collections;

public class ccatBall : MonoBehaviour
{
    string txt = "";
    bool activated = false;
    
    public void Activate(int id, float time)
    {
        Id = id;
        Time = time;
        foreach (var height in VAME_Manager.pathHeights)
        {
            if (Mathf.Abs(gameObject.transform.position.y - height) < 0.005f)
            {
                layer = height;
                break;
            }
        }
        activated = true;
    }

    void OnMouseDown()
    {
        if (!activated) return;
        gameObject.GetComponent<Renderer>().material = VoxelInspector.instance.selected;
        txt = "Id: " + Id.ToString() + "\n" + "Time: " + Time.ToString();
        VoxelInspector.instance.SetScrollText(txt);
    }

    void OnMouseOver()
    {
        if (!activated) return;
        txt = "";
        //foreach (var sloxel in voxel.Sloxels)
        //{
        //    foreach (var v2 in sloxel.PathLines)
        //    {
        //        txt += "Line[" + v2.x.ToString("f4") + "][" + v2.y.ToString("f0") + "]\n";
        //    }
        //}
        //VoxelInspector.instance.SetScrollText(txt);
        txt = "Id: " + Id.ToString() + "\n" + "Time: " + Time.ToString("f4");
        VoxelInspector.instance.SetScrollText(txt);
        gameObject.GetComponent<MeshRenderer>().material = VoxelInspector.instance.highlight;
    }

    void OnMouseExit()
    {
        if (!activated) return;
        gameObject.GetComponent<MeshRenderer>().material = VoxelInspector.instance.standard;
    }

    public int Id { get; set; }
    public float Time { get; set; }
    public float layer { get; set; }
}
