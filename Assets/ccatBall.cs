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
        Distance = -1f;
        activated = true;
    }

    void OnMouseDown()
    {
        return;
        if (!activated) return;
        gameObject.GetComponent<Renderer>().material = VoxelInspector.instance.selected;
        txt = "Id: " + Id.ToString() + "\n" + "Time: " + Time.ToString();
        VoxelInspector.instance.SetScrollText(txt);
    }

    void OnMouseOver()
    {
        if (!activated) return;
        txt = "";
        txt = "Id: " + Id.ToString() + "\n" + "Time: " + Time.ToString("f4") + "\n" + "Distance: " + Distance.ToString("f4");
        VoxelInspector.instance.SetScrollText(txt);
        gameObject.GetComponent<MeshRenderer>().material = VoxelInspector.instance.highlight;
        if (ClosestLine != null)
            ClosestLine.Color = Color.blue;
        VoxelInspector.instance.SetScrollText(txt);
    }

    void OnMouseExit()
    {
        if (!activated) return;
        gameObject.GetComponent<MeshRenderer>().material = VoxelInspector.instance.standard;
        CurrentColor();

        if (ClosestLine != null)
            ClosestLine.Color = VAME_Manager.instance.pathColor;
        txt = "";
        VoxelInspector.instance.SetScrollText(txt);
    }

    public int Id { get; set; }
    public float Time { get; set; }
    public float layer { get; set; }
    public PathLine ClosestLine { get; set; }
    public Vector3 ClosestPoint { get; internal set; }
    public float Distance { get; internal set; }
    public Sloxel Sloxel { get; set; }

    private void CurrentColor()
    {
        var color = new Color(0, 0, 0, 0.3f);
        if (Distance >= 0)
            color = new Color(0, 0, Distance * 2f, 0.3f);
        GetComponent<MeshRenderer>().material.SetColor("_SpecColor", color);
    }

    public void TestLine (PathLine testLine)
    {
        var p1 = testLine.p1;
        var p2 = testLine.p2;
        var pt = gameObject.transform.position;
        float dx = p2.x - p1.x;
        float dz = p2.z - p1.z;
        if ((dx == 0) && (dz == 0))
        {
            // It's a point not a line segment.
            var d = Vector3.Distance(p1, pt);
            if (d < Distance || Distance < 0)
            {
                ClosestLine = testLine;
                Distance = d;
                ClosestPoint = p1;
            }
        }
        else
        {
            // Calculate the t that minimizes the distance.
            float t = ((pt.x - p1.x) * dx + (pt.z - p1.z) * dz) /
                (dx * dx + dz * dz);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                var d = Vector3.Distance(p1, pt);
                if (d < Distance || Distance < 0)
                {
                    ClosestLine = testLine;
                    Distance = d;
                    ClosestPoint = p1;
                }
            }
            else if (t > 1)
            {
                var d = Vector3.Distance(p2, pt);
                if (d < Distance || Distance < 0)
                {
                    ClosestLine = testLine;
                    Distance = d;
                    ClosestPoint = p2;
                }
            }
            else
            {
                var closest = new Vector3(p1.x + t * dx, 0, p1.z + t * dz);
                var d = Vector3.Distance(closest, pt);
                if (d < Distance || Distance < 0)
                {
                    ClosestLine = testLine;
                    Distance = d;
                    ClosestPoint = closest;
                }
            }
        }
        CurrentColor();
    }
}
