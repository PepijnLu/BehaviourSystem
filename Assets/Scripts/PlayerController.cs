using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public Camera mainCamera; // Reference to the main camera
    public NavMeshAgent playerAgent; // Reference to the NavMeshAgent component on the player

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Check for mouse input
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the ray hits the ground
            if (Physics.Raycast(ray, out hit))
            {
                // Move the player to the clicked position
                playerAgent.SetDestination(hit.point);
            }
        }
    }
}
