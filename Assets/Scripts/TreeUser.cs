using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using UnityEngine;
using UnityEngine.AI;

public class TreeUser : MonoBehaviour
{
    [SerializeField] List<Transform> waypoints = new();
    [SerializeField] Transform safeSpot;
    [SerializeField] GameObject collectableObject, collectableObject2;
    NavMeshAgent agent;
    BehaviourTree tree;
    [SerializeField] bool isInDanger;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        tree = new BehaviourTree(GetType().Name);

        //Complex behaviour
        ComplexBehaviour();

        //Collect Object 1 Sequence Node
        Sequence goToObject = new Sequence("GoToObject", 20);
        goToObject.AddChild(new Leaf("IsObjectPresent", new Condition(() => collectableObject.activeSelf)));
        goToObject.AddChild(new Leaf("MoveToObject", new ActionStrategy(() => agent.SetDestination(collectableObject.transform.position))));

        //Collect Object 2 Sequence Node
        Sequence goToObject2 = new Sequence("GoToObject2", 10);
        goToObject2.AddChild(new Leaf("IsTreasure2Present", new Condition(() => collectableObject2.activeSelf)));
        goToObject2.AddChild(new Leaf("GoToObject2", new ActionStrategy(() => agent.SetDestination(collectableObject2.transform.position))));
        
        //Collect Objects Selector Node
        Selector goToObjects = new Selector("GoToObjects");
        goToObjects.AddChild(goToObject2);
        goToObjects.AddChild(goToObject);

        //Collect Objects Priority Selector Node
        PrioritySelector goToObjectsPriority = new PrioritySelector("GoToObjectsPriority");
        goToObjectsPriority.AddChild(goToObject);
        goToObjectsPriority.AddChild(goToObject2);

        //Collect Objects Random Selector Node
        PrioritySelector goToObjectsPriorityRandom = new RandomSelector("GoToObjectsPriorityRandom");
        goToObjectsPriorityRandom.AddChild(goToObject);
        goToObjectsPriorityRandom.AddChild(goToObject2);

        //Add Random Priority Selector Node: Go To Object 2, if not possible go to Object 1
        //tree.AddChild(goToObjectsPriorityRandom);

        //Add Priority Selector Node: Go To Object 2, if not possible go to Object 1
        //tree.AddChild(goToObjectsPriority);

        //Add Selector Node: Go to Object 2, if not possible go to Object 1
        //tree.AddChild(goToObjects);

        //Add Sequence Node: Go to Object 1
        //tree.AddChild(goToObject);

        //Add Leaf Node: Patrol
        //tree.AddChild(new Leaf("Patrol", new PatrolStrategy(transform, agent, waypoints, 2f)));
    }

    private void ComplexBehaviour()
    {
        //All the actions of the agent
        PrioritySelector actions = new PrioritySelector("Agent Logic");

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
        runToSafety.AddChild(new Leaf("GoToSafety", new ActionStrategy(() => agent.SetDestination(safeSpot.position))));
        
        //Add the runToSafety sequence to the agent's actions
        actions.AddChild(runToSafety);

        //Random Selector node that makes the agent go to object 1 or 2
        Selector goToRandomObject = new RandomSelector("GoToRandomObject", 50);
        
        //The sequence that makes the player move to object 1
        Sequence goToObject1 = new Sequence("GoToObject1");
            goToObject1.AddChild(new Leaf("IsObject1Present", new Condition(() => collectableObject.activeSelf)));
            goToObject1.AddChild(new Leaf("GoToObject1", new MoveToTarget(gameObject.transform, agent, collectableObject.transform, 2f))); 
            goToRandomObject.AddChild(goToObject1);
        //The sequence that makes the player move to object 2
        Sequence goToObject2 = new Sequence("GoToObject2");
            goToObject2.AddChild(new Leaf("IsObject2Present", new Condition(() => collectableObject2.activeSelf)));
            goToObject2.AddChild(new Leaf("GoToObject2", new MoveToTarget(gameObject.transform, agent, collectableObject2.transform, 2f))); 
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
        tree.Process();
    }
}
