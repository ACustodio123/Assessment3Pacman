using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderDotsScroller : MonoBehaviour
{
    public float speed = 60.0f;
    public bool horizontal = true;
    public float spacing = 10.0f;
    private RectTransform[] dots;
    private RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        int childCount = transform.childCount;
        dots = new RectTransform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            dots[i] = transform.GetChild(i).GetComponent<RectTransform>();
        }
    }

    void Update()
    {
        float delta = speed * Time.unscaledDeltaTime;
        float length = horizontal ? rt.rect.width : rt.rect.height;

        foreach (var d in dots)
        {
            var p = d.anchoredPosition;
            if (horizontal) p.x += delta; else p.y += delta;

            if (horizontal && p.x > length) p.x -= length + spacing;
            if (!horizontal && p.y > length) p.y -= length + spacing;

            d.anchoredPosition = p;
        }
    }
}
