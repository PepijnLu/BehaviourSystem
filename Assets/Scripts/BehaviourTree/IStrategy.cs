using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public interface IStrategy
{
    Node.Status Process();
    void Reset()
    {
        //Noop
    }
}

public class ActionStrategy : IStrategy
{
    readonly Action doSomething;

    public ActionStrategy(Action doSomething)
    {
        this.doSomething = doSomething; 
    }

    public Node.Status Process()
    {
        doSomething();
        return Node.Status.Success;
    }
}

public class Condition : IStrategy
{
    readonly Func<bool> predicate;

    public Condition(Func<bool> predicate)
    {
        this.predicate = predicate;
    }

    public Node.Status Process()
    {
        if (predicate())
        {
            return Node.Status.Success;
        }
        else
        {
            return Node.Status.Failure;
        }
    }
}

public class MoveToTarget : IStrategy
{
    readonly Transform entity;
    readonly NavMeshAgent agent;
    readonly Transform targetPoint;
    readonly float patrolSpeed;
    bool isPathCalculated, targetReached;

    public MoveToTarget(Transform entity, NavMeshAgent agent, Transform targetPoint, float patrolSpeed)
    {
        this.entity = entity;
        this.agent = agent;
        this.targetPoint = targetPoint;
        this.patrolSpeed = patrolSpeed;
        this.agent.speed = patrolSpeed;
    }

    public Node.Status Process()
    {
        if(targetReached) return Node.Status.Success;
        agent.SetDestination(targetPoint.position);
        entity.LookAt(targetPoint);

        if(isPathCalculated && agent.remainingDistance < 0.1f)
        {
            targetReached = true;
            isPathCalculated = false;
        }

        if(agent.pathPending)
        {
            isPathCalculated = true;
        }

        return Node.Status.Running;
    }

    public void Reset()
    {
        targetReached = false;
    }

}

public class PatrolStrategy : IStrategy
{
    readonly Transform entity;
    readonly NavMeshAgent agent;
    readonly List<Transform> patrolPoints;
    readonly float patrolSpeed;
    int currentIndex;
    bool isPathCalculated;

    public PatrolStrategy(Transform entity, NavMeshAgent agent, List<Transform> patrolPoints, float patrolSpeed)
    {
        this.entity = entity;
        this.agent = agent;
        this.patrolPoints = patrolPoints;
        this.patrolSpeed = patrolSpeed;
        this.agent.speed = patrolSpeed;
    }

    public Node.Status Process()
    {
        if(currentIndex == patrolPoints.Count) return Node.Status.Success;
        Transform target = patrolPoints[currentIndex];
        agent.SetDestination(target.position);
        entity.LookAt(target);

        if(isPathCalculated && agent.remainingDistance < 0.1f)
        {
            currentIndex++;
            isPathCalculated = false;
        }

        if(agent.pathPending)
        {
            isPathCalculated = true;
        }

        return Node.Status.Running;
    }

    public void Reset()
    {
        currentIndex = 0;
    }
    
}
