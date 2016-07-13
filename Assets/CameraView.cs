using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraView : MonoBehaviour
{
    float scaleRot = 0.2f;
    Vector3 lastMp;
    bool isCameraControl = false;


    void Start()
    {
        lastMp = Input.mousePosition;
    }

    //public bool 
    void Update()
    {
        var mp = Input.mousePosition;
        if (mp.x < 250 || mp.x > Screen.width - 250)
        {
            lastMp = mp;
            return;
        }
        var xFactor = (mp.x - lastMp.x) * scaleRot;
        var yFactor = -(mp.y - lastMp.y) * scaleRot;

        if (Input.GetMouseButton(0))
        {

            GameObject.Find("Camera Rig").transform.RotateAround(Vector3.zero, Camera.main.transform.up, xFactor);
            GameObject.Find("Camera Rig").transform.RotateAround(Vector3.zero, Camera.main.transform.right, yFactor);
        }
        var zoom = Input.mouseScrollDelta.y * 0.5f;
        var pos = Camera.main.transform.localPosition;
        pos.z += zoom;
        Camera.main.transform.localPosition = pos;
        lastMp = mp;
    }
}
