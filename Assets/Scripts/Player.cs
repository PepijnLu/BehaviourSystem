using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour, IAgent, IEnemyAttackable
{
    public Camera mainCamera;
    public NavMeshAgent playerAgent; 
    public List<GameObject> attackingAgents = new();
    public bool isInDanger, isBeingAttacked;
    [SerializeField] LayerMask ground;

    void Start()
    {
        
    }

    void Update()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, ground))
            {
                playerAgent.SetDestination(hit.point);
            }
        }
    }
    
    void IEnemyAttackable.setIsInDanger(bool _isInDanger)
    {
        isInDanger = _isInDanger;
    }

    void IEnemyAttackable.setIsBeingAttacked(bool _isBeingAttacked)
    {
        isBeingAttacked = _isBeingAttacked;
    }

    void IEnemyAttackable.SetAttackingAgent(GameObject agent, bool add)
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

        if(attackingAgents.Count > 0) isInDanger = true;
    }
    GameObject IEnemyAttackable.GetAttackingAgent()
    {
        if(attackingAgents.Count <= 0) return null;
        return attackingAgents[0];
    }
    Transform IAgent.getTransform()
    {
        return transform;
    }

    Transform IEnemyAttackable.getTransform()
    {
        return transform;
    }

    bool IEnemyAttackable.getIsBeingAttacked()
    {
        return isBeingAttacked;
    }

    Weapon IAgent.GetWeapon()
    {
        return null;
    }
    TextMeshProUGUI IAgent.getDisplayText()
    {
        return null;   
    }

    void IAgent.setIsStunned()
    {
        //Noop
    }

    void IAgent.DropWeapon()
    {
        //Noop
    }

    void IAgent.setCurrentActiveLeaf(string leafName)
    {
        //Noop
    }
}

