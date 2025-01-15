using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using UnityEngine;
using UnityEngine.AI;

public class TreeUser : MonoBehaviour
{
    [SerializeField] List<Transform> waypoints = new();
    [SerializeField] Transform safeSpot, weaponLocation, raycastOrigin;
    [SerializeField] GameObject collectableObject, collectableObject2, player;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] float attackRange, chaseRange = 5;
    NavMeshAgent agent;
    BehaviourTree tree;
    [SerializeField] Weapon weaponToGet;
    [SerializeField] bool isInDanger;
    bool hasWeapon;
    Dictionary<string, Func<bool>> strategyBreaks;

    private void Awake()
    {
        weaponToGet = GetClosestWeapon();

        agent = GetComponent<NavMeshAgent>();
        tree = new BehaviourTree(GetType().Name);

        //Complex behaviour
        //ComplexBehaviour();
        //Guard Enemy Behaviour
        GuardBehaviour();
        AddStrategyBreaks();
    }

    private void GuardBehaviour()
    {
        PrioritySequence actions = new PrioritySequence("Guard Logic");

        Sequence tryAddWeapon = new Sequence("TryAddWeapon", 15);
            tryAddWeapon.AddChild(new Leaf("CheckIfHasWeapon", new Condition(() => !hasWeapon)));
            tryAddWeapon.AddChild(new Leaf("FindClosestWeapon", new ActionStrategy(() => weaponLocation.position = GetClosestWeapon().transform.position)));
            tryAddWeapon.AddChild(new Leaf("MoveToWeapon", new MoveToTarget(gameObject.transform, agent, weaponLocation, 5f)));
            tryAddWeapon.AddChild(new Leaf("Pickup Weapon", new ActionStrategy(() => hasWeapon = true)));

        // Sequence tryChasePlayer = new Sequence("TryChasePlayer", 5);
        //     tryAddWeapon.AddChild(new Leaf("CheckHasWeapon", new Condition(() => hasWeapon)));
        //     tryAddWeapon.AddChild(new Leaf("MoveToPlayer", new MoveToTarget(gameObject.transform, agent, player.transform, 5f)));

        Sequence attackPlayer = new Sequence("AttackPlayer", 10);
            attackPlayer.AddChild(new Leaf("CheckInAttackRange", new Condition(() => CalculateDistanceToPlayer() <= attackRange)));
            attackPlayer.AddChild(new Leaf("AttackPlayer", new AttackTarget(gameObject.transform, agent, player.transform, 5f)));

        Sequence chasePlayer = new Sequence("ChasePlayer", 5);
            chasePlayer.AddChild(new Leaf("CheckInChaseRange", new Condition(() => CalculateDistanceToPlayer() <= chaseRange)));
            chasePlayer.AddChild(new Leaf("MoveToPlayer", new MoveToTarget(gameObject.transform, agent, player.transform, 5f)));

        // PrioritySelector chaseOrAttackPlayer = new PrioritySelector("ChaseOrAttackPlayer", 5);
        //     chaseOrAttackPlayer.AddChild(attackPlayer);
        //     chaseOrAttackPlayer.AddChild(chasePlayer);
        
        PrioritySelector grabWeaponOrChase = new PrioritySelector("GrabWeaponOrChase", 10);
            grabWeaponOrChase.AddChild(tryAddWeapon);
            grabWeaponOrChase.AddChild(attackPlayer);
            grabWeaponOrChase.AddChild(chasePlayer);
        
        Sequence tryNoticePlayer = new Sequence("NoticePlayer", 10);
            tryNoticePlayer.AddChild(new Leaf("CanSeePlayer", new Condition(() => TryHitRaycast())));
            tryNoticePlayer.AddChild(grabWeaponOrChase);

        actions.AddChild(tryNoticePlayer);

        //Sequence patrolPlayer = new Sequence("PatrolPlayer", 0);


        //Patrol leaf with the default priority of 0
        Leaf patrol = new Leaf("Patrol", new PatrolStrategy(transform, agent, waypoints, 2f));
        actions.AddChild(patrol);

        tree.AddChild(actions);
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

        if (Physics.Raycast(startPos, direction, out hit, Mathf.Infinity, playerLayer))
        {
            // Draw the raycast in the scene view for visualization
            //Debug.DrawRay(startPos, direction, Color.red);

            // Calculate the angle between objectA's forward vector and the direction to objectB
            float angle = Vector3.Angle(transform.forward, direction);

            // Log the angle to the console
            Debug.Log("Angle between ObjectA's forward vector and the direction to ObjectB: " + angle + " degrees");

            if(angle < 180)
            {
                Debug.Log("Guard hits raycast");
                return true;
            }
            else
            {
                return false;
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
        Debug.Log("Closest Weapon: " + closestObject.gameObject.name);
        return closestObject;
    } 

    private void ComplexBehaviour()
    {
        //All the actions of the agent
        PrioritySequence actions = new PrioritySequence("Agent Logic");

        //The sequence that makes the agent run to safety
        Sequence runToSafety = new Sequence("RunToSafety", 100);

        //Function that checks if the agent is in danger (if not, reset the runToSafety sequence)
        bool IsInDanger()
        {
            if(!isInDanger)
            {
                runToSafety.Reset();
                return false;
            }
            return true;
        }

        //Add the necessary leaf nodes to the runToSafety sequence
        runToSafety.AddChild(new Leaf("isInDanger?", new Condition(IsInDanger)));
        //runToSafety.AddChild(new Leaf("SetRunToSafetyTrue", new ActionStrategy(() => runningToSafety = true))); 
        runToSafety.AddChild(new Leaf("GoToSafety", new MoveToTarget(gameObject.transform, agent, safeSpot.transform, 5f)));
        //runToSafety.AddChild(new Leaf("SetRunToSafetyFalse", new ActionStrategy(() => runningToSafety = false))); 
        // runToSafety.priority = 0;
        
        //Add the runToSafety sequence to the agent's actions
        actions.AddChild(runToSafety);

        //Random Selector node that makes the agent go to object 1 or 2
        Selector goToRandomObject = new RandomSelector("GoToRandomObject", 50);
        
        //The sequence that makes the player move to object 1
        Sequence goToObject1 = new Sequence("GoToObject1");
            goToObject1.AddChild(new Leaf("IsObject1Present", new Condition(() => collectableObject.activeSelf)));
            goToObject1.AddChild(new Leaf("GoToObject1", new MoveToTarget(gameObject.transform, agent, collectableObject.transform, 5)));
            goToObject1.AddChild(new Leaf("PickUpObject1", new ActionStrategy(() => collectableObject.SetActive(false)))); 
            goToRandomObject.AddChild(goToObject1);
        //The sequence that makes the player move to object 2
        Sequence goToObject2 = new Sequence("GoToObject2");
            goToObject2.AddChild(new Leaf("IsObject2Present", new Condition(() => collectableObject2.activeSelf)));
            goToObject2.AddChild(new Leaf("GoToObject2", new MoveToTarget(gameObject.transform, agent, collectableObject2.transform, 5)));
            goToObject2.AddChild(new Leaf("PickUpObject2", new ActionStrategy(() => collectableObject2.SetActive(false)))); 
            goToRandomObject.AddChild(goToObject2);

        //Add the RandomSelector node to the agent's actions
        actions.AddChild(goToRandomObject);

        //Patrol leaf with the default priority of 0
        Leaf patrol = new Leaf("Patrol", new PatrolStrategy(transform, agent, waypoints, 2f));
        actions.AddChild(patrol);

        //Add the actions PrioritySelector to the tree
        tree.AddChild(actions);
    }


    private void Update()
    {
        bool interrupt = CheckStrategyBreaks();
        Debug.Log($"Interrupt? {interrupt}");
        tree.Process(interrupt);
    }

    bool CheckStrategyBreaks()
    {
        Debug.Log("Current Active Leaf: " + GameData.currentActiveLeaf);
        if(!strategyBreaks.ContainsKey(GameData.currentActiveLeaf)) return false;
        if(strategyBreaks[GameData.currentActiveLeaf]()) return true;
        return false;
    }

    void AddStrategyBreaks()
    {
        strategyBreaks = new()
        {
            ["GoToObject1"] = () => !collectableObject.activeSelf,
            ["GoToObject2"] = () => !collectableObject2.activeSelf,
            ["GoToSafety"] = () => !isInDanger,
            ["MoveToPlayer"] = () => CalculateDistanceToPlayer() <= attackRange || CalculateDistanceToPlayer() >= chaseRange,
            ["AttackPlayer"] = () => CalculateDistanceToPlayer() >= attackRange
        };
    }
}
