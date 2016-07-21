using UnityEngine;
using System.Collections;

public class ExampleClass : MonoBehaviour
{
    public Rect windowRect = new Rect(20, 20, 120, 50);
    void OnGUI()
    {
        windowRect = GUILayout.Window(0, windowRect, DoMyWindow, "My Window", GUILayout.Width(100));
    }
    void DoMyWindow(int windowID)
    {
        if (GUILayout.Button("Please click me a lot"))
            print("Got a click");

    }
}
