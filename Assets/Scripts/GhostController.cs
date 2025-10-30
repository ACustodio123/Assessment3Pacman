using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite scaredSprite;

    [Header("Movement Speeds")]
    [SerializeField] private float normalSpeed = 3f;
    [SerializeField] private float scaredSpeed = 1.5f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Spawn / Reset")]
    [SerializeField] private Vector3 startWorldPos;

    [Header("Grid Movement")]
    [SerializeField] private float tileSize = 1.28f;
    [SerializeField] private LayerMask walkableMask;

    [Header("Behaviour")]
    [SerializeField] private GhostType ghostType = GhostType.Random;
    [SerializeField] private PacStudentController pac;

    [Header("Dead / Respawn")]
    [SerializeField] private Vector3 spawnWorldPos;  
    [SerializeField] private float deadSpeed = 2.0f; 
    [SerializeField] private float reachSpawnRadius = 0.2f;



    private bool isMoving = false;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float moveT = 0f;
    private float moveDuration;  
    private Vector2Int gridPos;
    private Vector2Int currentDir = Vector2Int.right; 
    private Vector2Int lastDir = Vector2Int.zero;
    private bool isScared = false;
    private float scaredTimer = 0f;
    private bool isFrozen = false;
    private bool isDead = false;

    private static readonly Vector2Int[] DIRS = new Vector2Int[]
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };
    public enum GhostType
    {
        Chaser = 1,     // Ghost 2
        Evader = 2,     // Ghost 1
        Random = 3,     // Ghost 3
        Clockwise = 4   // Ghost 4
    }

    private Vector3 GetPacPos()
    {
        return pac != null ? pac.transform.position : transform.position;
    }

    private void Awake()
    {
        gridPos = WorldToGrid(transform.position);
        transform.position = GridToWorld(gridPos);
        RecalcMoveDuration();
    }

    private void RecalcMoveDuration()
    {
        float speed = normalSpeed;
        moveDuration = tileSize / Mathf.Max(0.05f, speed);
    }

    void Update()
    {
        if (isFrozen) return;

        HandleScaredTimer();


        if (isDead)
        {
            HandleDeadMovement();
        }
        else
        {
            HandleMovement();
        }

        if (isScared)
        {
            scaredTimer -= Time.deltaTime;

            if (scaredTimer < 2f)
                sr.color = Color.Lerp(Color.white, Color.gray, Mathf.PingPong(Time.time * 5f, 1f));

            if (scaredTimer <= 0f)
                SetScared(false, 0f);
        }

        float currentSpeed = isScared ? scaredSpeed : normalSpeed;
    }

    private void HandleDeadMovement()
    {
        Vector3 target = spawnWorldPos;
        Vector3 current = transform.position;

        Vector3 toSpawn = target - current;
        float dist = toSpawn.magnitude;

        if (dist <= reachSpawnRadius)
        {
            ReviveFromDead();
            return;
        }

        Vector3 dir = toSpawn / dist;
        transform.position += dir * deadSpeed * Time.deltaTime;

        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
        }
    }

    private void ReviveFromDead()
    {
        isDead = false;
        isFrozen = false;

        transform.position = spawnWorldPos;

        gridPos = WorldToGrid(spawnWorldPos);
        isMoving = false;
        moveT = 0f;
        startPos = spawnWorldPos;
        targetPos = spawnWorldPos;

        currentDir = Vector2Int.right;
        lastDir = Vector2Int.left;

        if (animator != null)
        {
            animator.enabled = true;
            animator.SetBool("IsScared", false);
            animator.SetBool("IsMoving", false);
        }

        if (normalSprite) sr.sprite = normalSprite;
        sr.color = Color.white;

        moveDuration = tileSize / Mathf.Max(0.05f, normalSpeed);
    }

    private void HandleMovement()
    {
        if (isDead) { return; }
        if (isMoving)
        {
            moveT += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, Mathf.Clamp01(moveT));

            if (moveT >= 1f)
            {
                FinishMove();
            }
            return;
        }
        DecideAndMove();
    }

    private void FinishMove()
    {
        isMoving = false;
        transform.position = targetPos;
        gridPos = WorldToGrid(transform.position);
        if (animator != null){ animator.SetBool("IsMoving", false); }

    }

    private void DecideAndMove()
    {
        List<Vector2Int> validDirs = new List<Vector2Int>();

        foreach (var dir in DIRS)
        {
            Vector2Int candidate = gridPos + dir;

            if (!IsWalkable(candidate))
                continue;

            if (dir == lastDir)
                continue;

            validDirs.Add(dir);
        }

        Vector2Int chosenDir;

        if (validDirs.Count == 0)
        {
            chosenDir = lastDir;
        }
        else
        {
            if (isScared)
            {
                chosenDir = PickDir_FurtherFromPac(validDirs);
            }
            else
            {
                chosenDir = PickDirectionByType(validDirs);
            }
            
        }

        Vector2Int nextGrid = gridPos + chosenDir;
        currentDir = chosenDir;
        StartMove(nextGrid, chosenDir);
    }

    private Vector2Int PickDirectionByType(List<Vector2Int> validDirs)
    {
        switch (ghostType)
        {
            case GhostType.Evader:
                return PickDir_FurtherFromPac(validDirs);
            case GhostType.Chaser:
                return PickDir_CloserToPac(validDirs);
            case GhostType.Random:
                return PickDir_Random(validDirs);
            case GhostType.Clockwise:
                return PickDir_Clockwise(validDirs);
            default:
                return PickDir_Random(validDirs);
        }
    }

    private Vector2Int PickDir_FurtherFromPac(List<Vector2Int> validDirs)
    {
        Vector3 pacPos = GetPacPos();
        float currentDist = Vector3.Distance(transform.position, pacPos);

        List<Vector2Int> better = new List<Vector2Int>();

        foreach (var dir in validDirs)
        {
            Vector3 candidateWorld = GridToWorld(gridPos + dir);
            float d = Vector3.Distance(candidateWorld, pacPos);
            if (d >= currentDist - 0.001f)
                better.Add(dir);
        }

        if (better.Count > 0)
        {
            int idx = Random.Range(0, better.Count);
            return better[idx];
        }

        return PickDir_Random(validDirs);
    }

    private Vector2Int PickDir_CloserToPac(List<Vector2Int> validDirs)
    {
        Vector3 pacPos = GetPacPos();
        float currentDist = Vector3.Distance(transform.position, pacPos);

        List<Vector2Int> better = new List<Vector2Int>();

        foreach (var dir in validDirs)
        {
            Vector3 candidateWorld = GridToWorld(gridPos + dir);
            float d = Vector3.Distance(candidateWorld, pacPos);
            if (d <= currentDist + 0.001f)
                better.Add(dir);
        }

        if (better.Count > 0)
        {
            int idx = Random.Range(0, better.Count);
            return better[idx];
        }

        return PickDir_Random(validDirs);
    }

    private Vector2Int PickDir_Random(List<Vector2Int> validDirs)
    {
        int idx = Random.Range(0, validDirs.Count);
        return validDirs[idx];
    }

    private static readonly Vector2Int[] CLOCKWISE_ORDER = new Vector2Int[]
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

    private Vector2Int PickDir_Clockwise(List<Vector2Int> validDirs)
    {
        foreach (var dir in CLOCKWISE_ORDER)
        {
            if (validDirs.Contains(dir))
                return dir;
        }

        return PickDir_Random(validDirs);
    }

    private void StartMove(Vector2Int nextGridPos, Vector2Int dir)
    {
        isMoving = true;
        moveT = 0f;
        startPos = transform.position;
        targetPos = GridToWorld(nextGridPos);
        gridPos = nextGridPos;

        lastDir = -dir;

        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
        }
        UpdateAnimation(dir);
    }

    private void UpdateAnimation(Vector2Int dir)
    {
        if (animator == null) return;

        if (dir == Vector2Int.right)
            {
                animator.SetInteger("Direction", 0);
                sr.flipX = false;
            }
        else if (dir == Vector2Int.left)
        {
            animator.SetInteger("Direction", 1);
            sr.flipX = false;
        }
            
        else if (dir == Vector2Int.up)
        {
            animator.SetInteger("Direction", 2);
            sr.flipX = false;
        }
            
        else if (dir == Vector2Int.down)
        {
            animator.SetInteger("Direction", 3);
            sr.flipX = false;
        }
            

        animator.SetBool("IsMoving", true);
    }

    private bool IsWalkable(Vector2Int gp)
    {
        Vector3 center = new Vector3(gp.x * tileSize, gp.y * tileSize, 0f);
        Vector2 boxSize = new Vector2(tileSize * 0.8f, tileSize * 0.8f);
        Collider2D hit = Physics2D.OverlapBox(center, boxSize, 0f, walkableMask);
        return hit == null;
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

    private void HandleScaredTimer()
    {
        if (!isScared) return;

        scaredTimer -= Time.deltaTime;

        bool hasAnimatorScared = animator != null;

        if (scaredTimer < 2f && !hasAnimatorScared)
        {
            sr.color = Color.Lerp(Color.white, Color.gray, Mathf.PingPong(Time.time * 5f, 1f));
        }

        if (scaredTimer <= 0f)
            SetScared(false, 0f);
    }


    public void SetScared(bool state, float duration = 10f)
    {
        if (isDead) return;

        isScared = state;
        scaredTimer = duration;

        if (animator != null)
        {
            animator.enabled = true;            
            animator.SetBool("IsScared", state); 
        }

        if (!state)
        {
            if (normalSprite) sr.sprite = normalSprite;
            sr.color = Color.white;
        }
        else
        {
            sr.color = Color.white;
        }

        if (state)
        {
            moveDuration = tileSize / Mathf.Max(0.05f, scaredSpeed);
        }
        else
        {
            moveDuration = tileSize / Mathf.Max(0.05f, normalSpeed);
        }
    }


    public bool IsScared() => isScared;
    public bool IsDead() => isDead;

    public void SetFrozen(bool f)
    {
        isFrozen = f;
        if (animator != null) { animator.SetBool("IsMoving", !f); }
    }

    public void KillByPlayer()
    {
        if (isDead) return;

        isDead = true;
        isScared = false;

        isFrozen = false;

        if (sr)
            sr.color = new Color(1f, 1f, 1f, 0.5f);

        if (animator)
        {
            animator.enabled = true;    
            animator.SetBool("IsScared", false);
            animator.SetBool("IsMoving", true);
        }

    }

    private IEnumerator DeadRecoveryRoutine()
    {
        yield return new WaitForSeconds(3f);
        isDead = false;
        isFrozen = false;

        if (animator) { animator.enabled = true; }
        if (normalSprite) sr.sprite = normalSprite;
        sr.color = Color.white;

        transform.position = startWorldPos;
        gridPos = WorldToGrid(startWorldPos);
    }

    public void ResetToStart()
    {
        transform.position = startWorldPos;
        gridPos = WorldToGrid(startWorldPos);
        isFrozen = false;
        isDead = false;
        SetScared(false, 0f);
    }

    public void SetDead(bool dead)
    {
        isDead = dead;
        if (dead)
        {
            if (animator) animator.enabled = false;
            sr.color = new Color(1f, 1f, 1f, 0.6f);
        }
        else
        {
            if (animator) { animator.enabled = true; }
            sr.color = Color.white;
        }
    }
}
