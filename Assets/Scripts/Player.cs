using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour, IAgent, IEnemyAttackable
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask ground;
    private NavMeshAgent playerAgent; 
    private List<GameObject> attackingAgents = new();
    private bool isBeingAttacked;
    private float health = 10;

    void Start()
    {
        playerAgent = GetComponent<NavMeshAgent>();
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

    void Die()
    {
        gameObject.SetActive(false);
    }

    float IEnemyAttackable.GetHealth()
    {
        return health;
    }

    void IEnemyAttackable.SetHealth(float _health)
    {
        health = _health;

        if(health < 0)
        {
            Die();
        }
    }
    
    void IEnemyAttackable.SetIsBeingAttacked(bool _isBeingAttacked)
    {
        isBeingAttacked = _isBeingAttacked;
    }

    void IEnemyAttackable.SetAttackingAgent(GameObject _agent, bool _add)
    {
        if(_add)
        {
            if(!attackingAgents.Contains(_agent))
            {
                attackingAgents.Add(_agent);
            }
        }
        else
        {
            if(attackingAgents.Contains(_agent))
            {
                attackingAgents.Remove(_agent);
            }
        }
    }
    GameObject IEnemyAttackable.GetAttackingAgent()
    {
        if(attackingAgents.Count <= 0) return null;
        return attackingAgents[0];
    }
    Transform IAgent.GetTransform()
    {
        return transform;
    }

    Transform IEnemyAttackable.GetTransform()
    {
        return transform;
    }

    bool IEnemyAttackable.GetIsBeingAttacked()
    {
        return isBeingAttacked;
    }

    Weapon IAgent.GetWeapon()
    {
        return null;
    }
    TextMeshProUGUI IAgent.GetDisplayText()
    {
        return null;   
    }

    void IAgent.SetIsStunned()
    {
        //Noop
    }

    void IAgent.DropWeapon()
    {
        //Noop
    }

    void IAgent.SetCurrentActiveLeaf(string _leafName)
    {
        //Noop
    }

    void IEnemyAttackable.SetIsInDanger(bool _isInDanger)
    {
        //Noop
    }

}

