using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InspectorL : MonoBehaviour
{
    public static InspectorL instance;
    public Button btnModel;
    public Button btnPaths;
    public Button btnPoints;
    public Toggle tglWire;
    public Toggle tglTrans;
    public ScrollRect codeArea;
    public Text codeText;
    public RectTransform codeDummy;
    private RectTransform codeDummyRect;

    public Slider visibiltySlider;
    public Slider pathVisibiltySlider;
    public static float modelVisibility = 1.0f;
    public static float pathVisibility = 1.0f;
    public static float pointVisibility = 1.0f;
    public enum Mode { model, paths, points, none }
    public static Mode _mode;
    public enum ViewMode { solid, fade, wire, hide }
    public static ViewMode _viewMode;
    public static bool isWireframe = false;
    public static bool isTransparent = false;


    void Awake()
    {
        instance = this;
        codeDummyRect = codeDummy;
    }

    #region modes

    public void pressBtnModel ()
    {
        visibiltySlider.value = modelVisibility;
        _mode = Mode.model;
        CodeArea(0);
    }

    public void pressBtnPaths()
    {
        visibiltySlider.value = pathVisibility;
        _mode = Mode.paths;
        CodeArea(0);
    }

    public void pressBtnPoints()
    {
        visibiltySlider.value = pointVisibility;
        _mode = Mode.points;
        CodeArea(0);
    }

    #endregion

    #region viewModes

    public void pressBtnViewFade()
    {
        foreach (var mesh in VAME_Manager.Meshes)
        {
            if (tglTrans.isOn)
                mesh.GetComponent<MeshRenderer>().material = VAME_Manager.instance.matFade;
            else
                mesh.GetComponent<MeshRenderer>().material = VAME_Manager.instance.matSolid;
        }
        _viewMode = ViewMode.fade;
    }

    public void pressBtnViewWire()
    {
        isWireframe = tglWire.isOn;
        _viewMode = ViewMode.wire;
    }

    public void pressBtnViewHide()
    {
        _viewMode = ViewMode.hide;
    }

    #endregion

    public void slideVisibility()
    {
        modelVisibility = visibiltySlider.value;
        foreach (var m in VAME_Manager.Meshes)
        {
            var rndMat = m.GetComponent<MeshRenderer>().material;
            var c = rndMat.color;
            c.a = visibiltySlider.value;
            rndMat.color = c;
            c = rndMat.GetColor("_SpecColor");
            c *= visibiltySlider.value;
            rndMat.SetColor("_SpecColor", c);
            visibiltySlider.gameObject.GetComponentInChildren<Text>().text = "Visibility: " + (visibiltySlider.value * 100).ToString("f0") + "%";
        }
    }

    public void slidePathVisibility()
    {
        pathVisibility = pathVisibiltySlider.value;
        pathVisibiltySlider.gameObject.GetComponentInChildren<Text>().text = "Visibility: " + (pathVisibiltySlider.value * 100).ToString("f0") + "%";
    }

    public void ScrollCode()
    {
        if (_mode == Mode.model)
        {
            CodeArea((int)(codeArea.verticalScrollbar.value * VAME_Manager.modelCode.Count));
        }
        else if (_mode == Mode.paths)
        {
            CodeArea((int)(codeArea.verticalScrollbar.value * VAME_Manager.pathsCode.Count));
        }
        else if (_mode == Mode.points)
        {
            CodeArea((int)(codeArea.verticalScrollbar.value * VAME_Manager.cctCode.Count));
        }
    }

    public void CodeArea(int value)
    {
        var str = "";
        for (int i = value; i < value + 100; i++)
        {
            if (i == value) str += "<color=green>";
            if (_mode == Mode.model)
            {
                if (VAME_Manager.modelCode.Count > i)
                    str += VAME_Manager.modelCode[i] + "\n";
            }
            else if (_mode == Mode.paths)
            {
                if (VAME_Manager.pathsCode.Count > i)
                    str += VAME_Manager.pathsCode[i] + "\n";
            }
            else if (_mode == Mode.points)
            {
                if (VAME_Manager.cctCode.Count > i)
                    str += VAME_Manager.cctCode[i];
            }
            if (i == value) str += "</color>";
        }
        codeText.text = str;
        if (value == 0)
        {
            var pos = codeDummyRect.transform.position;
            switch(_mode)
            {
                case Mode.model:
                    pos.y = VAME_Manager.modelCode.Count / 2 + 100;
                    break;
                case Mode.paths:
                    pos.y = VAME_Manager.pathsCode.Count / 2 + 100;
                    break;
                case Mode.points:
                    break;
                default:
                    break;
            }
            codeDummy.transform.position = pos;
        }
    }
}
