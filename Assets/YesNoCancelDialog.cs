using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class YesNoCancelDialog : MonoBehaviour
{
    public GameObject veil;

    void Start()
    {
        veil = VAME_Manager.instance.yncVeil;
        veil.SetActive(true);
    }

    public void yes()
    {
        switch (transform.FindChild("Title").GetComponent<Text>().text)
        {
            case "Exit VAME":
                print("Exiting");
                if (System.IO.Directory.Exists(VAME_Manager.tmpFolderName))
                    System.IO.Directory.Delete(VAME_Manager.tmpFolderName, true);
                Application.Quit();
                break;
            case "Clear Session":
                if (System.IO.Directory.Exists(VAME_Manager.tmpFolderName))
                    System.IO.Directory.Delete(VAME_Manager.tmpFolderName, true);
                VAME_Manager.instance.ClearAll();
                break;
            default:
                break;
        }
        veil.SetActive(false);
        DestroyImmediate(gameObject);
    }

    public void no()
    {
        veil.SetActive(false);
        DestroyImmediate(gameObject);
    }

    public void cancel()
    {
        veil.SetActive(false);
        DestroyImmediate(gameObject);
    }
}
