using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntMover : MonoBehaviour
{
    public List<Vector3> waypoints = new List<Vector3>();
    private int currentWaypointIndex = 0;
    private float moveSpeed = 2f;
    private float startTime;
    private Vector3 startPos, endPos;
    private float journeyLength;
    private bool isMoving = false;
    private AudioSource walkingSound;

    void Start()
    {
        walkingSound = GetComponent<AudioSource>();
        MoveToNextWaypoint();

        if (!walkingSound.isPlaying)
        {
            walkingSound.Play();
        }


  
    }

    void MoveToNextWaypoint()
    {
        startPos = transform.position;
        endPos = waypoints[currentWaypointIndex];
        startTime = Time.time;
        journeyLength = Vector3.Distance(startPos, endPos);
        isMoving = true;
        
        SetRotation();
        

    }

    void SetRotation()
    {
        Vector3 direction = (endPos - startPos).normalized;

        float angle = 0f;

        if (direction == Vector3.up) 
        {
            angle = 0f;
        }
        else if (direction == Vector3.left)
        {
            angle = 90f;
        }
        else if (direction == Vector3.down)
        {
            angle = 180f;
        }
        else if (direction == Vector3.right)
        {
            angle = 270f;
        }

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        if (!isMoving || waypoints.Count == 0) return;

        float distCovered = (Time.time - startTime) * moveSpeed;
        float fracJourney = distCovered / journeyLength;

        transform.position = Vector3.Lerp(startPos, endPos, fracJourney);

        if (fracJourney >= 1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            MoveToNextWaypoint();
        }
    }
}
