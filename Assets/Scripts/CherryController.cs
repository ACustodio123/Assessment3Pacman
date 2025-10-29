using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CherryController : MonoBehaviour
{
    [Header("Render")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private int sortingOrderOnTop = 200;
    [SerializeField] private SpriteMaskInteraction maskMode = SpriteMaskInteraction.VisibleInsideMask;

    private Vector3 a, b;
    private float duration;
    private float t;
    private bool activeMove;

    public void Init(Vector3 start, Vector3 end, float unitsPerSecond)
    {
        a = start;
        b = end;
        transform.position = a;
        float dist = Vector3.Distance(a, b);
        float speed = Mathf.Max(0.01f, unitsPerSecond);
        duration = dist / speed;
        t = 0f;
        activeMove = true;

        if (sr != null)
        {
            sr.sortingOrder = sortingOrderOnTop;
            sr.maskInteraction = maskMode;
        }
    }

    private void Update()
    {
        if (!activeMove) return;

        t += Time.deltaTime / duration;
        float u = Mathf.Clamp01(t);
        transform.position = Vector3.Lerp(a, b, u);

        if (t >= 1.05f)
        {
            activeMove = false;
            Destroy(gameObject);
        }
    }
}
