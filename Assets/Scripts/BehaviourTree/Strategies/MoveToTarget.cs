using System;
using UnityEngine;
using UnityEngine.AI;

public class MoveToTarget : IStrategy
{
    readonly Transform entity;
    readonly NavMeshAgent agent;
    readonly Transform targetPoint;
    bool isPathCalculated;
    IAgent runnerInterface;

    public MoveToTarget(GameObject runner, Transform entity, NavMeshAgent agent, Transform targetPoint, float patrolSpeed)
    {
        this.entity = entity;
        this.agent = agent;
        this.targetPoint = targetPoint;
        this.agent.speed = patrolSpeed;

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
            agent.ResetPath();
            runnerInterface.SetCurrentActiveLeaf("");
            if(runnerInterface.GetDisplayText().text != "") runnerInterface.GetDisplayText().text = "";
            Reset();
            return Node.Status.Failure; 
        }

        if(entity == null) throw new Exception("Entity is null");
        
        if(Vector3.Distance(entity.position, targetPoint.position) < 1f) 
        {
            agent.ResetPath();
            Reset();
            return Node.Status.Success;
        }

        agent.SetDestination(targetPoint.position);
        entity.LookAt(targetPoint.position);
        
        if(isPathCalculated && agent.remainingDistance < 0.1f)
        {
            isPathCalculated = false;
        }

        if(agent.pathPending)
        {
            isPathCalculated = true;
        }

        runnerInterface.SetCurrentActiveLeaf(leafName);
        if(runnerInterface.GetDisplayText().text != "Current Action: " + leafName) runnerInterface.GetDisplayText().text = "Current Action: " + leafName;
        return Node.Status.Running;
    }

    public void Reset()
    {
        isPathCalculated = false;
    }

}
