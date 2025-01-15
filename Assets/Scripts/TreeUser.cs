using System;
using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using UnityEngine;
using UnityEngine.AI;

public class TreeUser : MonoBehaviour
{
    /*
        When the user's last leaf was "GoToSafety", "Patrol" will break
    */
    [SerializeField] List<Transform> waypoints = new();
    [SerializeField] Transform safeSpot;
    [SerializeField] GameObject collectableObject, collectableObject2;
    NavMeshAgent agent;
    BehaviourTree tree;
    [SerializeField] bool isInDanger;
    bool runningToSafety, collectingObject;
    GameObject objectBeingCollected;
    Dictionary<string, Func<bool>> strategyBreaks;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        tree = new BehaviourTree(GetType().Name);

        //Complex behaviour
        ComplexBehaviour();
        AddStrategyBreaks();
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
            ["GoToSafety"] = () => !isInDanger
        };
    }
}
