using UnityEngine;
using System.Collections;

public class ccatBall : MonoBehaviour
{
    string txt = "";
    bool activated = false;
    public Material material;
    Color currentDefaultColor;
    Color baseColor;
    public ccatInterpreter.BallState ballState { get; set;}
    
    public void Activate(int id, float time)
    {
        Id = id;
        Time = time;
        ballState = ccatInterpreter.BallState.Default;
        foreach (var height in VAME_Manager.pathHeights)
        {
            if (Mathf.Abs(gameObject.transform.position.y - height) < 0.005f)
            {
                layer = height;
                break;
            }
        }
        Distance = -1f;
        Temp = 3000f;
        activated = true;
        currentDefaultColor = Color.gray;
        baseColor = currentDefaultColor;
        material.color = currentDefaultColor;
        gameObject.GetComponent<Renderer>().material = material;
    }

    void OnMouseDown()
    {
        SelectBall(true);
    }

    void OnMouseOver()
    {
        if (!activated) return;
        txt = "";
        txt = "Id: " + Id.ToString() + "\n" + "Time: " + Time.ToString("f4") + "\n" + "Distance: " + Distance.ToString("f4");
        VoxelInspector.instance.SetScrollText(txt);
        gameObject.GetComponent<MeshRenderer>().material.color = Color.blue;
        if (ClosestLine != null)
            ClosestLine.Color = Color.blue;
        VoxelInspector.instance.SetScrollText(txt);
    }

    void OnMouseExit()
    {
        if (!activated) return;
        SetColor(currentDefaultColor);

        if (ClosestLine != null)
            ClosestLine.Color = VAME_Manager.instance.pathColor;
        txt = "";
        VoxelInspector.instance.SetScrollText(txt);
    }

    public int Id { get; set; }
    public float Time { get; set; }
    public float TimeError { get; set; }
    public float layer { get; set; }
    public PathLine ClosestLine { get; set; }
    public Vector3 ClosestPoint { get; internal set; }
    public float Distance { get; internal set; }
    public Sloxel Sloxel { get; set; }
    public float Temp { get; set; }

    public void SetToDefault()
    {
        currentDefaultColor = baseColor;
    }

    public void SetColor(Color color)
    {
        currentDefaultColor = color;
        gameObject.GetComponent<MeshRenderer>().material.color = currentDefaultColor;
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
                TimeError = testLine.StartTime - Time;
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
                    TimeError = testLine.StartTime - Time;
                }
            }
            else if (t > 1)
            {
                var d = Vector3.Distance(p2, pt);
                d /= 4.0f;
                if (d < Distance || Distance < 0)
                {
                    ClosestLine = testLine;
                    Distance = d;
                    ClosestPoint = p2;
                    TimeError = testLine.EndTime - Time;
                }
            }
            else
            {
                var closest = new Vector3(p1.x + t * dx, 0, p1.z + t * dz);
                var d = Vector3.Distance(closest, pt);
                d /= 4.0f;
                if (d < Distance || Distance < 0)
                {
                    ClosestLine = testLine;
                    Distance = d;
                    ClosestPoint = closest;
                    var D = Vector3.Distance(p2, p1);
                    TimeError = d / D * (testLine.EndTime - testLine.StartTime);
                }
            }
        }
        //CurrentColor();
    }

    public void SetActive(bool active)
    {
        if (ballState == ccatInterpreter.BallState.Selected || ballState == ccatInterpreter.BallState.PartOfSet && !active) return;
        //if (VAME_Manager.instance.selectedVoxelInfo == this) return;

        if (active && !GetComponent<Renderer>().enabled && !GetComponent<Collider>().enabled)
        {
            GetComponent<Renderer>().enabled = true;
            GetComponent<Collider>().enabled = true;
        }
        else if (!active && GetComponent<Renderer>().enabled && GetComponent<Collider>().enabled)
        {
            GetComponent<Renderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
        }
    }

    public void SelectBall(bool directClick)
    {
        if (!activated) return;
        if (VAME_Manager.instance.selectedBall != null)
        {
            VAME_Manager.instance.selectedBall.gameObject.GetComponent<MeshRenderer>().material.color = VAME_Manager.instance.selectedBall.currentDefaultColor;
        }
        VAME_Manager.instance.selectedBall = this;
        var h = Sloxel.origin.y;
        //var ind = VAME_Manager.pathHeights.IndexOf(h);
        //if (directClick)
        //    VAME_Manager.slicerForm.ChangeLayer(ind, voxel);
        gameObject.GetComponent<MeshRenderer>().material.color = Color.yellow;
        txt = "";
        txt += "Point ID: " + VAME_Manager.crazyBalls.IndexOf(gameObject) + "\n\n";
        //txt += "Contains " + voxel.Sloxels.Count.ToString() + " Voxels" + "\n";
        //var i = 1;
        //foreach (var sloxel in voxel.Sloxels)
        //{
        //    var ht = sloxel.origin.y;
        //    txt += i.ToString() + ". " + "Sloxel[" + ht.ToString("f3") + "][" + Sloxelizer2.instance.sloxels[ht].IndexOf(sloxel).ToString() + "] \n";
        //    txt += "Containing " + sloxel.PathLines.Count.ToString() + " Path(s)\n";
        //    foreach (var v2 in sloxel.PathLines)
        //    {
        //        txt += "Path[" + v2.x.ToString("f3") + "][" + v2.y.ToString("f0") + "]\n";
        //    }
        //    i++;
        //}
        VoxelInspector.instance.SetScrollText(txt);
        ballState = ccatInterpreter.BallState.Selected;
        SetActive(true);
    }
}
