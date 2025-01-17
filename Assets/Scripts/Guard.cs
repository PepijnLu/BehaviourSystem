using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class Guard : MonoBehaviour, IAgent
{
    [SerializeField] List<Transform> waypoints = new();
    [SerializeField] Transform safeSpot, weaponLocation, raycastOrigin;
    [SerializeField] GameObject collectableObject, collectableObject2, player;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] float attackRange, chaseRange = 5;
    [SerializeField] TextMeshProUGUI displayText;
    NavMeshAgent agent;
    BehaviourTree tree;
    Weapon weaponToGet, equippedWeapon;
    [SerializeField] bool isInDanger;
    bool hasWeapon;
    Dictionary<string, Func<bool>> strategyBreaks;
    public string currentActiveLeaf;
    IEnemyAttackable target;
    public bool isStunned;
    Coroutine timerRoutine;

    private void Awake()
    {
        weaponToGet = GetClosestWeapon();

        agent = GetComponent<NavMeshAgent>();
        tree = new BehaviourTree(GetType().Name);
        target = player.GetComponent<IEnemyAttackable>();

        //Guard Enemy Behaviour
        GuardBehaviour();
        AddStrategyBreaks();
    }

    TextMeshProUGUI IAgent.getDisplayText()
    {
        return displayText;
    }

    void IAgent.setCurrentActiveLeaf(string leafName)
    {
        currentActiveLeaf = leafName;
    }

    Transform IAgent.getTransform()
    {
        return transform;
    }

    void IAgent.setIsStunned()
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

    private void GuardBehaviour()
    {
        PrioritySequence actions = new PrioritySequence("Guard Logic");

        Sequence tryAddWeapon = new Sequence("TryAddWeapon", 15);
            tryAddWeapon.AddChild(new Leaf("CheckIfHasWeapon", new Condition(() => !hasWeapon)));
            tryAddWeapon.AddChild(new Leaf("FindClosestWeapon", new ActionStrategy(() => weaponToGet = GetClosestWeapon())));
            tryAddWeapon.AddChild(new Leaf("MoveToWeapon", new MoveToTarget(gameObject, gameObject.transform, agent, weaponLocation, 5f)));
            //tryAddWeapon.AddChild(new Leaf("Pickup Weapon", new ActionStrategy(() => hasWeapon = true)));
            tryAddWeapon.AddChild(new Leaf("Pickup Weapon", new ActionStrategy(() => EquipWeapon())));

        Sequence attackPlayer = new Sequence("AttackPlayer", 10);
            attackPlayer.AddChild(new Leaf("CheckInAttackRange", new Condition(() => CalculateDistanceToPlayer() <= attackRange)));
            attackPlayer.AddChild(new Leaf("SetBeingAttacked", new ActionStrategy(() => target.setIsBeingAttacked(true))));
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
            tryNoticePlayer.AddChild(new Leaf("SetInDangerBool", new ActionStrategy(() => target.setIsInDanger(true))));
            tryNoticePlayer.AddChild(grabWeaponOrChase);

        Sequence beStunned = new Sequence("BeStunned", 15);
            beStunned.AddChild(new Leaf("CheckIfStunned", new Condition(() => isStunned)));
            beStunned.AddChild(new Leaf("BeStunned", new BeStunned(gameObject, agent)));

        actions.AddChild(tryNoticePlayer);
        actions.AddChild(beStunned);

        //Sequence patrolPlayer = new Sequence("PatrolPlayer", 0);


        //Patrol leaf with the default priority of 0
        Sequence patrol = new Sequence("Patrol");
            patrol.AddChild(new Leaf("SetInDangerBool", new ActionStrategy(() => target.setIsInDanger(false))));
            //patrol.AddChild(new Leaf("Settarget", new ActionStrategy(() => target = null)));
            patrol.AddChild(new Leaf("Patrol", new PatrolStrategy(gameObject, transform, agent, waypoints, 2f)));
        // Leaf patrol = new Leaf("Patrol", new PatrolStrategy(this, transform, agent, waypoints, 2f));
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
        //return true;

        Debug.Log("Guard tries to hit raycast");
        // Calculate the direction from objectA to objectB
        Vector3 startPos = raycastOrigin.position;
        Vector3 direction = player.transform.position - startPos;

        // Perform the raycast
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
                // Calculate the angle between objectA's forward vector and the direction to objectB
                float angle = Vector3.Angle(transform.forward, direction);

                // Log the angle to the console
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
            Debug.DrawRay(startPos, direction * 100, Color.blue); // Draw a ray indicating no hit
            Debug.Log("Raycast did not hit any object on the specified LayerMask.");
        }
        //Debug.Log("Guard misses raycast");
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

    Weapon IAgent.GetWeapon()
    {
        return equippedWeapon;
    }

    void AddStrategyBreaks()
    {
        strategyBreaks = new()
        {
            ["GoToObject1"] = () => !collectableObject.activeSelf,
            ["GoToObject2"] = () => !collectableObject2.activeSelf,
            ["GoToSafety"] = () => !isInDanger,
            ["MoveToPlayer"] = () => CalculateDistanceToPlayer() <= attackRange || CalculateDistanceToPlayer() >= chaseRange,
            ["AttackPlayer"] = () => CalculateDistanceToPlayer() >= attackRange || isStunned,

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
}
