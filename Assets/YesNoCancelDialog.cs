using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class YesNoCancelDialog : MonoBehaviour
{
    public void yes()
    {
        switch (transform.FindChild("Title").GetComponent<Text>().text)
        {
            case "Exit VAME":
                print("Exiting");
                Application.Quit();
                break;
            default:
                break;
        }
        DestroyImmediate(gameObject);
    }

    public void no()
    {
        DestroyImmediate(gameObject);
    }

    public void cancel()
    {
        DestroyImmediate(gameObject);
    }
}
