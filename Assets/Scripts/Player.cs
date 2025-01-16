using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour, IAgent, IEnemyAttackable
{
    public Camera mainCamera; // Reference to the main camera
    public NavMeshAgent playerAgent; // Reference to the NavMeshAgent component on the player
    public List<IAgent> attackingAgents = new();
    public bool isInDanger, isBeingAttacked;

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

    TextMeshProUGUI IAgent.getDisplayText()
    {
        return null;   
    }

    //string getCurrentActiveLeaf();
    void IAgent.setCurrentActiveLeaf(string leafName)
    {
        //Noop
    }
    
    void IAgent.setIsInDanger(bool _isInDanger)
    {
        isInDanger = _isInDanger;
    }

    void IEnemyAttackable.SetAttackingAgent(IAgent agent, bool add)
    {
        if(add)
        {
            if(!attackingAgents.Contains(agent))
            {
                attackingAgents.Add(agent);
            }
        }
        else
        {
            if(attackingAgents.Contains(agent))
            {
                attackingAgents.Remove(agent);
            }
        }

        if(attackingAgents.Count > 0) isBeingAttacked = true;
    }
    Transform IAgent.getTransform()
    {
        return null;
    }
}

