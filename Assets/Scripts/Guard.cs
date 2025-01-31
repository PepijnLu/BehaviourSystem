using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class Guard : MonoBehaviour, IAgent
{
    [SerializeField] private List<Transform> waypoints = new();
    [SerializeField] private Transform safeSpot, weaponLocation, raycastOrigin;
    [SerializeField] private GameObject collectableObject, collectableObject2, player;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private float attackRange, chaseRange = 5;
    private Dictionary<string, Func<bool>> strategyBreaks;
    private NavMeshAgent agent;
    private BehaviourTree tree;
    private Weapon weaponToGet, equippedWeapon;
    private IEnemyAttackable target;
    private Coroutine timerRoutine;
    private bool isInDanger, isStunned, hasWeapon;
    public string currentActiveLeaf;

    private void Awake()
    {
        weaponToGet = GetClosestWeapon();

        agent = GetComponent<NavMeshAgent>();
        tree = new BehaviourTree(GetType().Name);
        target = player.GetComponent<IEnemyAttackable>();

        GuardBehaviour();
        AddStrategyBreaks();
    }

    private void GuardBehaviour()
    {
        PrioritySequence actions = new PrioritySequence("Guard Logic");

        Sequence tryAddWeapon = new Sequence("TryAddWeapon", 15);
            tryAddWeapon.AddChild(new Leaf("CheckIfHasWeapon", new Condition(() => !hasWeapon)));
            tryAddWeapon.AddChild(new Leaf("FindClosestWeapon", new ActionStrategy(() => weaponToGet = GetClosestWeapon())));
            tryAddWeapon.AddChild(new Leaf("MoveToWeapon", new MoveToTarget(gameObject, gameObject.transform, agent, weaponLocation, 5f)));
            tryAddWeapon.AddChild(new Leaf("Pickup Weapon", new ActionStrategy(() => EquipWeapon())));

        Sequence attackPlayer = new Sequence("AttackPlayerSequence", 10);
            attackPlayer.AddChild(new Leaf("CheckInAttackRange", new Condition(() => CalculateDistanceToPlayer() <= attackRange)));
            attackPlayer.AddChild(new Leaf("SetBeingAttacked", new ActionStrategy(() => target.SetIsBeingAttacked(true))));
            attackPlayer.AddChild(new Leaf("AttackPlayer", new AttackTarget(gameObject, gameObject.transform, agent, player.transform, 5f)));

        Sequence chasePlayer = new Sequence("ChasePlayer", 5);
            chasePlayer.AddChild(new Leaf("CheckInChaseRange", new Condition(() => CalculateDistanceToPlayer() <= chaseRange)));
            chasePlayer.AddChild(new Leaf("MoveToPlayer", new MoveToTarget(gameObject, gameObject.transform, agent, player.transform, 5f)));

        PrioritySelector grabWeaponOrChase = new PrioritySelector("GrabWeaponOrChase", 10);
            grabWeaponOrChase.AddChild(tryAddWeapon);
            grabWeaponOrChase.AddChild(attackPlayer);
            grabWeaponOrChase.AddChild(chasePlayer);
        
        Sequence tryNoticePlayer = new Sequence("NoticePlayer", 10);
            tryNoticePlayer.AddChild(new Leaf("CanSeePlayer", new Condition(() => TryHitRaycast())));
            tryNoticePlayer.AddChild(new Leaf("SetTarget", new ActionStrategy(() => target.SetAttackingAgent(gameObject, true))));
            tryNoticePlayer.AddChild(new Leaf("SetInDangerBool", new ActionStrategy(() => target.SetIsInDanger(true))));
            tryNoticePlayer.AddChild(grabWeaponOrChase);

        Sequence beStunned = new Sequence("BeStunned", 15);
            beStunned.AddChild(new Leaf("CheckIfStunned", new Condition(() => isStunned)));
            beStunned.AddChild(new Leaf("BeStunned", new BeStunned(gameObject, agent)));
            beStunned.AddChild(new Leaf("BackToWaypoint", new MoveToTarget(gameObject, transform, agent, waypoints[0], 2f)));

        actions.AddChild(tryNoticePlayer);
        actions.AddChild(beStunned);

        Sequence patrol = new Sequence("Patrol");
            patrol.AddChild(new Leaf("SetInDangerBool", new ActionStrategy(() => target.SetIsInDanger(false))));
            patrol.AddChild(new Leaf("Patrol", new PatrolStrategy(gameObject, transform, agent, waypoints, 2f)));

        actions.AddChild(patrol);

        tree.AddChild(actions);
    }

    void EquipWeapon()
    {
        hasWeapon = true;
        weaponToGet.transform.SetParent(transform);
        equippedWeapon = weaponToGet;
        equippedWeapon.transform.LookAt(player.transform);
        equippedWeapon.transform.Rotate(-90, 0, 0);
    }

    void IAgent.DropWeapon()
    {
        hasWeapon = false;
        weaponToGet.transform.SetParent(null);
        equippedWeapon = null;
    }

    bool TryHitRaycast()
    {
        Debug.Log("Guard tries to hit raycast");

        Vector3 startPos = raycastOrigin.position;
        Vector3 direction = player.transform.position - startPos;

        RaycastHit hit;
        Debug.DrawRay(startPos, direction, Color.red);

        if (Physics.Raycast(startPos, direction, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject.layer != 8) 
            {
                Debug.Log("Raycast Hit Object: " + hit.collider.gameObject.name);
                Debug.Log("Raycast Hit Layer: " + hit.collider.gameObject.layer);
                return false;
            }
            else
            {
                float angle = Vector3.Angle(transform.forward, direction);

                Debug.Log("Angle between ObjectA's forward vector and the direction to ObjectB: " + angle + " degrees");

                if(angle < 120)
                {
                    Debug.Log("Guard hits raycast");
                    return true;
                }
                else
                {
                    return false;
                }
            } 
        }
        else
        {
            Debug.DrawRay(startPos, direction * 100, Color.blue); 
            Debug.Log("Raycast did not hit any object on the specified LayerMask.");
        }
        
        return false;
    }

    float CalculateDistanceToPlayer()
    {
        return (transform.position - player.transform.position).magnitude;
    }

    Weapon GetClosestWeapon()
    {
        Weapon[] weapons = FindObjectsOfType<Weapon>();
        if(weapons.Length == 0) 
        {
            Debug.Log("No weapon found");
            return null;
        }

        Weapon closestObject = weapons.OrderBy(obj => Vector3.Distance(transform.position, obj.transform.position)).FirstOrDefault();
        weaponToGet = closestObject;
        weaponLocation.position = weaponToGet.transform.position;
        Debug.Log("Closest Weapon: " + closestObject.gameObject.name);
        return closestObject;
    } 

    private void Update()
    {
        bool interrupt = CheckStrategyBreaks();
        Debug.Log($"Interrupt? {interrupt}");
        tree.Process(interrupt);
        TryHitRaycast();
    }

    bool CheckStrategyBreaks()
    {
        Debug.Log("Current Active Leaf: " + currentActiveLeaf);
        if(!strategyBreaks.ContainsKey(currentActiveLeaf)) return false;
        if(strategyBreaks[currentActiveLeaf]()) return true;
        return false;
    }
    void AddStrategyBreaks()
    {
        strategyBreaks = new()
        {
            ["GoToObject1"] = () => !collectableObject.activeSelf,
            ["GoToObject2"] = () => !collectableObject2.activeSelf,
            ["GoToSafety"] = () => !isInDanger,
            ["MoveToPlayer"] = () => CalculateDistanceToPlayer() <= attackRange || CalculateDistanceToPlayer() >= chaseRange || !player.activeSelf,
            ["AttackPlayer"] = () => CalculateDistanceToPlayer() >= attackRange || isStunned || !player.activeSelf,

            ["BeStunned"] = () => !isStunned
        };
    }

    IEnumerator StunTimer(float duration)
    {
        float elapsedTime = 0;
        while(elapsedTime < duration)
        {
            elapsedTime += 0.02f;
            yield return new WaitForFixedUpdate();
        }
        isStunned = false;
    }

    void IAgent.SetIsStunned()
    {
        Debug.Log("Get Stunned: " + gameObject.name);
        isStunned = true;
        if(timerRoutine == null) timerRoutine = StartCoroutine(StunTimer(5f));
        else
        { 
            StopCoroutine(timerRoutine);
            timerRoutine = StartCoroutine(StunTimer(5f));
        }
            
    }
    void IAgent.SetCurrentActiveLeaf(string leafName)
    {
        currentActiveLeaf = leafName;
    }
    TextMeshProUGUI IAgent.GetDisplayText()
    {
        return displayText;
    }

    Transform IAgent.GetTransform()
    {
        return transform;
    }


    Weapon IAgent.GetWeapon()
    {
        return equippedWeapon;
    }
}
