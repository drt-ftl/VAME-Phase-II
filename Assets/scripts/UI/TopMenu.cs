using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class TopMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    RectTransform container;
    bool isOpen;

    void Start()
    {
        container = transform.FindChild("Container").GetComponent<RectTransform>();
        var scale = container.localScale;
        scale.y = 0;
        container.localScale = scale;
        isOpen = false;
    }

    void Update()
    {
        Vector3 scale = container.localScale;
        scale.y = Mathf.Lerp(scale.y, isOpen ? 1 : 0, Time.deltaTime * 20);
        container.localScale = scale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isOpen = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isOpen = false;
    }
}
