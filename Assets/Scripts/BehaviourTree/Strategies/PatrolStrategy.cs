using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolStrategy : IStrategy
{
    readonly Transform entity;
    readonly NavMeshAgent agent;
    readonly List<Transform> patrolPoints;
    int currentIndex;
    bool isPathCalculated, onFirstRoute;
    IAgent runnerInterface;

    public PatrolStrategy(GameObject runner, Transform entity, NavMeshAgent agent, List<Transform> patrolPoints, float patrolSpeed)
    {
        this.entity = entity;
        this.agent = agent;
        this.patrolPoints = patrolPoints;
        this.agent.speed = patrolSpeed;
        onFirstRoute = true;

        
        if(agent.TryGetComponent(out IAgent _runnerInterface))
        {
            runnerInterface = _runnerInterface;
        }
        else throw new Exception($"Runner {runner.name} isn't an agent");
    }

    public Node.Status Process(bool isInterrupted, string leafName)
    {
        if(isInterrupted)
        {
            runnerInterface.SetCurrentActiveLeaf("");
            if(runnerInterface.GetDisplayText().text != "") runnerInterface.GetDisplayText().text = "";
            Reset();
            return Node.Status.Failure; 
        }

        if(currentIndex == patrolPoints.Count) 
        {
            Reset();
            return Node.Status.Success; 
        }
        Transform target = patrolPoints[currentIndex];
        agent.SetDestination(target.position);
        entity.LookAt(target);

        if(isPathCalculated && agent.remainingDistance < 0.1f)
        {
            currentIndex++;
            onFirstRoute = false;
            isPathCalculated = false;
        }

        if(agent.pathPending || onFirstRoute)
        {
            isPathCalculated = true;
        }

        runnerInterface.SetCurrentActiveLeaf(leafName);
        if(runnerInterface.GetDisplayText().text != "Current Action: " + GetType().Name) runnerInterface.GetDisplayText().text = "Current Action: " + GetType().Name;
        return Node.Status.Running;
    }

    public void Reset()
    {
        currentIndex = 0;
    }
    
}

