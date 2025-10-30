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

    private bool isScared = false;
    private float scaredTimer = 0f;
    private bool isFrozen = false;
    private bool isDead = false;

    void Update()
    {
        // Pause everything if frozen (during death or game pause)
        if (isFrozen) return;

        // Handle scared countdown & flashing
        if (isScared)
        {
            scaredTimer -= Time.deltaTime;

            // Flash gray for last 2 seconds
            if (scaredTimer < 2f)
                sr.color = Color.Lerp(Color.white, Color.gray, Mathf.PingPong(Time.time * 5f, 1f));

            // End scared state
            if (scaredTimer <= 0f)
                SetScared(false, 0f);
        }

        // Use speed for AI movement (handled elsewhere)
        float currentSpeed = isScared ? scaredSpeed : normalSpeed;
    }

    // Sets the ghost to Scared or Normal state.
    public void SetScared(bool state, float duration = 0f)
    {
        if (isDead) return; // Dead ghosts ignore state changes

        isScared = state;
        scaredTimer = duration;

        if (state)
        {
            if (animator) animator.enabled = false;
            if (scaredSprite) sr.sprite = scaredSprite;
            sr.color = Color.white;
        }
        else
        {
            if (animator) animator.enabled = true;
            if (normalSprite) sr.sprite = normalSprite;
            sr.color = Color.white;
        }
    }

    public bool IsScared() => isScared;
    public bool IsDead() => isDead;

    public void SetFrozen(bool f)
    {
        isFrozen = f;
    }

    //Called when Pacstudent kills a ghost
    public void KillByPlayer()
    {
        if (isDead) { return; }
        isDead = true;
        isScared = false;

        if (animator) { animator.enabled = false; }
        if (sr)
        {
            sr.color = new Color(1f, 1f, 1f, 0.5f);
        }

        isFrozen = true;
        StartCoroutine(DeadRecoveryRoutine());
    }



    private IEnumerator DeadRecoveryRoutine()
    {
        yield return new WaitForSeconds(3f);
        isDead = false;
        isFrozen = false;

        if (animator) { animator.enabled = true; }
        if (normalSprite) sr.sprite = normalSprite;
        sr.color = Color.white;
    }


    // Called when PacStudent dies or new round starts.
    // Resets ghost to normal behavior and starting position.
    public void ResetToStart()
    {
        transform.position = startWorldPos;
        isFrozen = false;
        isDead = false;
        SetScared(false, 0f);
    }

    // Use this later when PacStudent eats the ghost in scared mode.
    public void SetDead(bool dead)
    {
        isDead = dead;
        if (dead)
        {
            if (animator) animator.enabled = false;
            sr.color = new Color(1f, 1f, 1f, 0.6f);
        }
    }
}
