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

    [Header("Wall Collision FX")]
    [SerializeField] private ParticleSystem bumpParticlesPrefab;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip bumpClip;

    [Header("Teleporters (grid coords)")]
    [SerializeField] private int tunnelRowY;
    [SerializeField] private int leftExitX;
    [SerializeField] private int rightExitX;
    [SerializeField] private int leftAppearX;
    [SerializeField] private int rightAppearX;

    [Header("Ghost Collision / Death")]
    [SerializeField] private float ghostCheckRadius = 0.4f;
    [SerializeField] private GhostController[] ghosts; 
    [SerializeField] private ParticleSystem deathFX;
    [SerializeField] private Vector3 respawnWorldPos; 
    [SerializeField] private Sprite deadUp;
    [SerializeField] private Sprite deadDown;
    [SerializeField] private Sprite deadLeft;
    [SerializeField] private Sprite deadRight;
    [SerializeField] private float PowerPelletDuration = 10f;

    // movement state
    private Vector2Int lastInput = Vector2Int.zero;
    private Vector2Int currentInput = Vector2Int.zero;
    private bool isMoving = false;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float moveT = 0f;
    private float moveDuration;
    private Vector2Int gridPos;

    // death
    private bool isDead = false;
    private Coroutine powerRoutine;

    private bool isFrozen = false;

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

    void OnValidate()
    {
        RecalcMoveDuration();
    }

    private void StopMove()
    {
        transform.position = targetPos;
        isMoving = false;
        gridPos = WorldToGrid(transform.position);

        StopDust();

        if (moveAudio != null)
            moveAudio.Stop();

        animator?.SetBool("IsMoving", false);
    }

    void Update()
    {
        if (isDead || isFrozen) { return; }
        // read input
        Vector2Int input = ReadDiscreteInput();
        if (input != Vector2Int.zero)
        {
            lastInput = input;
        }

        // moving lerp
        if (isMoving)
        {
            moveT += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, Mathf.Clamp01(moveT));

            if (moveT >= 1f)
            {
                StopMove();
                CheckForPellets();

                // teleporter check first
                if (TryTeleportIfAtTunnel())
                    return;

                // then ghost collision check
                if (!isDead)
                    CheckForGhostHit();
            }

            return;
        }

        // not moving: try lastInput first
        if (lastInput != Vector2Int.zero && TryMove(lastInput))
        {
            currentInput = lastInput;
            return;
        }

        // otherwise keep moving in currentInput
        if (currentInput != Vector2Int.zero)
        {
            TryMove(currentInput);
        }

        if (Input.GetKeyDown(KeyCode.G))
            {
            Vector2Int gp = WorldToGrid(transform.position);
            Debug.Log($"PacStudent grid position: ({gp.x}, {gp.y}) | World pos: {transform.position}");
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
        {
            PlayBumpFX(dir);
            return false;
        }

        StartMove(dir);
        UpdateAnimation(dir);
        PlayDust();

        return true;
    }

    private void PlayBumpFX(Vector2Int dir)
    {
        Vector3 hitPos = transform.position + new Vector3(dir.x, dir.y, 0f) * (tileSize * 0.5f);

        if (bumpParticlesPrefab != null)
        {
            ParticleSystem bumpFx = Instantiate(bumpParticlesPrefab, hitPos, Quaternion.identity);
            bumpFx.Play();
            Destroy(bumpFx.gameObject, 1f);
        }

        if (sfxSource != null && bumpClip != null)
        {
            sfxSource.PlayOneShot(bumpClip);
        }
    }

    private void StartMove(Vector2Int dir)
    {
        isMoving = true;
        moveT = 0f;
        startPos = transform.position;
        targetPos = startPos + new Vector3(dir.x, dir.y, 0f) * tileSize;

        // audio
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

    private void PlayDust()
    {
        if (!dust) return;
        if (!dust.isPlaying) dust.Play();
    }

    private void StopDust()
    {
        if (dust && dust.isPlaying)
            dust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private bool IsWalkable(Vector2Int gp)
    {
        Vector3 center = new Vector3(gp.x * tileSize, gp.y * tileSize, 0f);
        Vector2 boxSize = new Vector2(tileSize * 0.8f, tileSize * 0.8f);
        Collider2D hit = Physics2D.OverlapBox(center, boxSize, 0f, wallMask);
        return hit == null;
    }

    private void UpdateAnimation(Vector2Int dir)
    {
        if (animator == null) return;

        if (dir == Vector2Int.right) { animator.SetInteger("Direction", 0); if (sr) sr.flipX = false; }
        else if (dir == Vector2Int.left) { animator.SetInteger("Direction", 1); if (sr) sr.flipX = false; }
        else if (dir == Vector2Int.up) { animator.SetInteger("Direction", 2); if (sr) sr.flipX = false; }
        else if (dir == Vector2Int.down) { animator.SetInteger("Direction", 3); if (sr) sr.flipX = false; }
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
                CheckGameOverPellets();
            }
            else if (hit.CompareTag("PowerPellet"))
            {
                hudManager?.AddScore(50);
                hudManager?.StartGhostTimer(ghostScaredDuration);
                AudioManager.Instance?.TriggerScaredMusic(ghostScaredDuration);
                StartPowerVisuals(ghostScaredDuration);
                Destroy(hit.gameObject);
            }
            else if (hit.CompareTag("Cherry"))
            {
                hudManager?.AddScore(100);
                Destroy(hit.gameObject);
            }
        }
    }

    private void CheckGameOverPellets()
    {
        if (GameObject.FindGameObjectsWithTag("Pellet").Length == 0 && GameObject.FindGameObjectsWithTag("PowerPellet").Length == 0)
        {
            hudManager?.TriggerGameOver();
        }
    }

    private void StartPowerVisuals(float duration)
    {
        if (powerRoutine != null)
        {
            StopCoroutine(powerRoutine);
        }
        powerRoutine = StartCoroutine(PowerVisualsRoutine(duration));
    }

    private IEnumerator PowerVisualsRoutine(float duration)
    {
        if (animator != null)
        {
            animator.SetBool("IsPowered", true);
        }
        yield return new WaitForSeconds(duration);

        if (animator != null)
        {
            animator.SetBool("IsPowered", false);
        }
        powerRoutine = null;
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

    private bool TryTeleportIfAtTunnel()
    {
        Vector2Int dir = currentInput != Vector2Int.zero ? currentInput : lastInput;
        if (gridPos.y != tunnelRowY || (dir != Vector2Int.left && dir != Vector2Int.right))
            return false;

        if (dir == Vector2Int.left && gridPos.x <= leftExitX)
        {
            Vector2Int newPos = new Vector2Int(leftAppearX, tunnelRowY);
            TeleportTo(newPos, dir);
            return true;
        }

        if (dir == Vector2Int.right && gridPos.x >= rightExitX)
        {
            Vector2Int newPos = new Vector2Int(rightAppearX, tunnelRowY);
            TeleportTo(newPos, dir);
            return true;
        }

        return false;
    }

    private void TeleportTo(Vector2Int newGridPos, Vector2Int continueDir)
    {
        gridPos = newGridPos;
        transform.position = GridToWorld(gridPos);
        StartMove(continueDir);
        UpdateAnimation(continueDir);
    }

    private void CheckForGhostHit()
    {
        if (isDead) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, ghostCheckRadius);
        foreach (var h in hits)
        {
            GhostController gc = h.GetComponent<GhostController>();
            if (gc == null) { continue; }
            if (gc.IsScared() && !gc.IsDead())
            {
                gc.KillByPlayer();
                hudManager?.AddScore(300);
                return;
            }
            if (!gc.IsDead())
            {
                StartCoroutine(DoDeathSequence());
                return;
            }
        }
    }

    public void SetFrozen(bool f)
    {
        isFrozen = f;
        if (f)
        {
            isMoving = false;
            StopDust();

            if (moveAudio != null && moveAudio.isPlaying) { moveAudio.Stop(); }
            if (animator != null) { animator.SetBool("IsMoving", false); }
        }
    }

    private IEnumerator DoDeathSequence()
    {
        isDead = true;
        isMoving = false;

        // stop current vfx/sfx
        StopDust();
        if (moveAudio != null && moveAudio.isPlaying)
            moveAudio.Stop();

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.enabled = false;
        }

            

        // freeze ghosts
        foreach (var g in ghosts)
            if (g != null)
                g.SetFrozen(true);

        // death particles
        if (deathFX != null)
        {
            var fx = Instantiate(deathFX, transform.position, Quaternion.identity);
            fx.Play();
            Destroy(fx.gameObject, 1.5f);
        }

        // show death sprite matching direction
        if (sr != null)
        {
            Vector2Int dir = currentInput != Vector2Int.zero ? currentInput : lastInput;
            if (dir == Vector2Int.up && deadUp) sr.sprite = deadUp;
            else if (dir == Vector2Int.down && deadDown) sr.sprite = deadDown;
            else if (dir == Vector2Int.left && deadLeft) sr.sprite = deadLeft;
            else if (deadRight) sr.sprite = deadRight;
        }

        // lose a life
        hudManager?.LoseLife();

        // let player see the death
        yield return new WaitForSeconds(1.0f);

        // respawn
        transform.position = respawnWorldPos;
        gridPos = WorldToGrid(respawnWorldPos);
        lastInput = Vector2Int.zero;
        currentInput = Vector2Int.zero;

        // reset animator to face right / idle
        if (animator != null)
        {
            animator.enabled = true;
            animator.SetBool("IsMoving", false);
            animator.SetInteger("Direction", 0);
            animator.SetBool("IsPowered", false);
        }

        // reset ghosts
        foreach (var g in ghosts)
            if (g != null)
                g.ResetToStart();

        // tiny delay before allowing death again
        yield return new WaitForSeconds(0.3f);

        yield return StartCoroutine(WaitForPlayerInput());

        isDead = false;
    }

    private IEnumerator WaitForPlayerInput()
    {
        while (!Input.anyKeyDown) { yield return null; }
    }

    private void OnEnable()
    {
        if (moveAudio != null)
            moveAudio.enabled = true;

        if (sfxSource != null)
            sfxSource.enabled = true;
    }

}