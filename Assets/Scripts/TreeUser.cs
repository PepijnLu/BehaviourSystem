using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TreeUser : MonoBehaviour
{
    [SerializeField] List<Transform> waypoints = new();
    NavMeshAgent agent;
    BehaviourTree tree;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        tree = new BehaviourTree(GetType().Name);
        tree.AddChild(new Leaf("Patrol", new PatrolStrategy(transform, agent, waypoints, 2f)));
    }
    private void Start()
    {
        
    }

    private void Update()
    {
        tree.Process();
    }
}
