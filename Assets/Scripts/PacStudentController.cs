using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private float tileSize = 1.28f;
    [SerializeField] private float tilesPerSecond = 6f;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private HUDManager hudManager;
    [SerializeField] private float pelletCheckRadius = 0.4f;
    [SerializeField] private float ghostScaredDuration = 10f;


    [Header("Walkability")]
    [SerializeField] private LayerMask wallMask;

    [Header("Audio")]
    [SerializeField] private AudioSource moveAudio;
    [SerializeField] private AudioClip normalMoveClip;
    [SerializeField] private AudioClip eatMoveClip;

    [Header("Visuals")]
    [SerializeField] private ParticleSystem dust;



    private Vector2Int lastInput = Vector2Int.zero;
    private Vector2Int currentInput = Vector2Int.zero;
    private bool isMoving = false;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float moveT = 0f;
    private float moveDuration;
    private Vector2Int gridPos;

    void Awake()
    {
        RecalcMoveDuration();
        gridPos = WorldToGrid(transform.position);
        transform.position = GridToWorld(gridPos);
    }

    private void RecalcMoveDuration()
    {
        moveDuration = 1f / Mathf.Max(0.1f, tilesPerSecond);
    }

    void OnValidate() { RecalcMoveDuration(); }

    private void StopMove()
    {
        transform.position = targetPos;
        isMoving = false;
        gridPos = WorldToGrid(transform.position);

        if (dust){ dust.Stop(true, ParticleSystemStopBehavior.StopEmitting); }


        if (moveAudio != null)
            moveAudio.Stop();

        animator?.SetBool("IsMoving", false);
        Debug.Log($"StopMove at {Time.time}");
    }

    void Update()
    {
        Vector2Int input = ReadDiscreteInput();
        if (input != Vector2Int.zero)
        {
            lastInput = input;
        }

        if (isMoving)
        {
            moveT += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, Mathf.Clamp01(moveT));

            if (moveT >= 1f)
            {
                StopMove();
                CheckForPellets();
            }

            return;
        }

        if (lastInput != Vector2Int.zero && TryMove(lastInput))
        {
            currentInput = lastInput;
            return;
        }
        
        if (currentInput != Vector2Int.zero)
        {
            TryMove(currentInput);
        }
    }

    private Vector2Int ReadDiscreteInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) return Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.A)) return Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.S)) return Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.D)) return Vector2Int.right;
        return Vector2Int.zero;
    }

    private Vector3 GridToWorld(Vector2Int grid)
    {
        return new Vector3(grid.x * tileSize, grid.y * tileSize, 0f);
    }

    private Vector2Int WorldToGrid(Vector3 world)
    {
        int gx = Mathf.RoundToInt(world.x / tileSize);
        int gy = Mathf.RoundToInt(world.y / tileSize);
        return new Vector2Int(gx, gy);
    }

    private bool TryMove(Vector2Int dir)
    {
        Vector2Int next = gridPos + dir;

        if (!IsWalkable(next))
            return false;

        StartMove(dir);
        UpdateAnimation(dir);

        return true;
    }

    private void StartMove(Vector2Int dir)
    {
        isMoving = true;
        moveT = 0f;
        startPos = transform.position;
        targetPos = startPos + new Vector3(dir.x, dir.y, 0f) * tileSize;

        if (dust)
        {
            var em = dust.emission; em.enabled = true;
            dust.Clear();
            dust.Play(true);
            Debug.Log($"StartMove at {Time.time}");
        }

        if (moveAudio)
        {
            bool pelletAhead = IsPelletAhead(dir);
            var nextClip = pelletAhead ? eatMoveClip : normalMoveClip;
            if (moveAudio.clip != nextClip || !moveAudio.isPlaying)
            {
                moveAudio.clip = nextClip;
                moveAudio.loop = true;
                moveAudio.Play();
            }

        }
        animator?.SetBool("IsMoving", true);
    }

    private bool IsWalkable(Vector2Int gp)
    {
        Vector3 center = new Vector3(gp.x * tileSize, gp.y * tileSize, 0f);
        Vector2 boxSize = new Vector2(tileSize * 0.8f, tileSize * 0.8f);
        Collider2D hit = Physics2D.OverlapBox(center, boxSize, 0f, wallMask);
        return hit == null;
    }

    private bool InBounds(int[,] map, Vector2Int gp)
    {
        int rows = map.GetLength(0);
        int cols = map.GetLength(1);
        return gp.x >= 0 && gp.x < cols && gp.y >= 0 && gp.y < rows;
    }

    private bool Contains(int[] arr, int value)
    {
        if (arr == null) return false;
        for (int i = 0; i < arr.Length; i++)
            if (arr[i] == value) return true;
        return false;
    }

    private void UpdateAnimation(Vector2Int dir)
    {
        if (animator != null)
        {
            if (dir == Vector2Int.right) { animator.SetInteger("Direction", 0); if (sr) sr.flipX = false; }
            if (dir == Vector2Int.left) { animator.SetInteger("Direction", 1); if (sr) sr.flipX = false; }
            if (dir == Vector2Int.up) { animator.SetInteger("Direction", 2); if (sr) sr.flipX = false; }
            if (dir == Vector2Int.down) { animator.SetInteger("Direction", 3); if (sr) sr.flipX = false; }
        }
    }

    private void CheckForPellets()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pelletCheckRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Pellet"))
            {
                hudManager?.AddScore(10);
                Destroy(hit.gameObject);
            }

            else if (hit.CompareTag("PowerPellet"))
            {
                hudManager?.AddScore(50);
                hudManager?.StartGhostTimer(ghostScaredDuration);
                Destroy(hit.gameObject);
            }
        }
    }

    private bool IsPelletAhead(Vector2Int dir)
    {
        Vector2Int next = gridPos + dir;
        Vector3 center = new Vector3(next.x * tileSize, next.y * tileSize, 0f);
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, 0.3f);
        foreach (var h in hits)
            if (h.CompareTag("Pellet") || h.CompareTag("PowerPellet"))
                return true;
        return false;
    }

}
