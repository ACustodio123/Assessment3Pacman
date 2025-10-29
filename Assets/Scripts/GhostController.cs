using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite scaredSprite;

    [Header("Scared Settings")]
    [SerializeField] private float normalSpeed = 3f;
    [SerializeField] private float scaredSpeed = 1.5f;
    [SerializeField] private Animator animator;

    private bool isScared = false;
    private float scaredTimer = 0f;

    void Update()
    {
        if (isScared)
        {
            scaredTimer -= Time.deltaTime;

            if (scaredTimer < 2f)
            {
                sr.color = Color.Lerp(Color.white, Color.gray, Mathf.PingPong(Time.time * 5f, 1f));
            }

            if (scaredTimer <= 0f)
            {
                SetScared(false);
            }
        }
        float currentSpeed = isScared ? scaredSpeed : normalSpeed;
    }

public void SetScared(bool state, float duration = 0f)
{
    isScared = state;
    scaredTimer = duration;

        if (state)
        {
            if (animator) animator.enabled = false;
            sr.sprite = scaredSprite;
        }
        else
        {
            if (animator) animator.enabled = true;
            sr.sprite = normalSprite;
            sr.color = Color.white;
        }
    
    // Debug.Log(name + " -> SetScared(" + state + ", " + duration + ")");
}

    public bool IsScared() => isScared;
}
