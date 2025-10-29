using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CherrySpawner : MonoBehaviour
{
    [Header("Prefab & Camera")]
    [SerializeField] private CherryController cherryPrefab;
    [SerializeField] private Camera cam;

    [Header("Grid / Speed")]
    [SerializeField] private float tileSize = 1.28f;
    [SerializeField] private float tilesPerSecond = 6f;

    [Header("Timing")]
    [SerializeField] private float spawnDelaySecs = 5f;
    [SerializeField] private float offscreenMargin = 2.0f;

    private CherryController current;

    private void Start()
    {
        if (!cam) cam = Camera.main;
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(spawnDelaySecs);

        while (true)
        {
            SpawnOne();

            yield return new WaitUntil(() => current == null);

            yield return new WaitForSeconds(spawnDelaySecs);
        }
    }

    private void SpawnOne()
    {
        Vector3 wmin = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 wmax = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
        float cx = (wmin.x + wmax.x) * 0.5f;
        float cy = (wmin.y + wmax.y) * 0.5f;

        bool horizontal = Random.value > 0.5f;
        bool startOnNegativeSide = Random.value > 0.5f;
        float speedUnitsPerSec = tilesPerSecond * tileSize;

        Vector3 a, b;

        if (horizontal)
        {
            float y = RoundToGrid(cy, tileSize);
            float startX = startOnNegativeSide ? (wmin.x - offscreenMargin) : (wmax.x + offscreenMargin);
            float endX = startOnNegativeSide ? (wmax.x + offscreenMargin) : (wmin.x - offscreenMargin);
            a = new Vector3(startX, y, 0f);
            b = new Vector3(endX, y, 0f);
        }

        else
        {
            float x = RoundToGrid(cx, tileSize);
            float startY = startOnNegativeSide ? (wmin.y - offscreenMargin) : (wmax.y + offscreenMargin);
            float endY = startOnNegativeSide ? (wmax.y + offscreenMargin) : (wmin.y - offscreenMargin);
            a = new Vector3(x, startY, 0f);
            b = new Vector3(x, endY, 0f);
        }

        current = Instantiate(cherryPrefab, a, Quaternion.identity, transform);
        current.Init(a, b, speedUnitsPerSec);
    }
    
    private float RoundToGrid(float v, float step)
    {
        return Mathf.Round(v / step) * step;
    }
}
