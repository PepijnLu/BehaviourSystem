using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public interface IStrategy
{
    Node.Status Process(bool isInterrupted, string leafName);
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

    public Node.Status Process(bool isInterrupted, string leafName)
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

    public Node.Status Process(bool isInterrupted, string leafName)
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
    readonly IAgent runner;
    readonly Transform entity;
    readonly NavMeshAgent agent;
    readonly Transform targetPoint;
    readonly float patrolSpeed;
    bool isPathCalculated;

    public MoveToTarget(IAgent runner, Transform entity, NavMeshAgent agent, Transform targetPoint, float patrolSpeed)
    {
        this.entity = entity;
        this.agent = agent;
        this.targetPoint = targetPoint;
        this.patrolSpeed = patrolSpeed;
        this.agent.speed = patrolSpeed;
        this.runner = runner;

        Debug.Log("MoveToTarget constructor");
    }

    public Node.Status Process(bool isInterrupted, string leafName)
    {
        if(isInterrupted)
        {
            Reset();
            return Node.Status.Failure; 
        }

        if(Vector3.Distance(entity.position, targetPoint.position) < 1f) 
        {
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

        Debug.Log("Current Strategy Name: " + leafName);
        runner.setCurrentActiveLeaf(leafName);
        if(runner.getDisplayText().text != "Current Action: " + GetType().Name) runner.getDisplayText().text = "Current Action: " + GetType().Name;
        return Node.Status.Running;
    }

    public void Reset()
    {
        runner.setCurrentActiveLeaf("");
        if(runner.getDisplayText().text != "") runner.getDisplayText().text = "";
        isPathCalculated = false;
    }

}

public class AttackTarget : IStrategy
{
    readonly IAgent runner;
    readonly Transform entity;
    readonly NavMeshAgent agent;
    readonly Transform targetPoint;
    readonly IEnemyAttackable enemyAttackable;
    readonly float patrolSpeed;
    bool isPathCalculated;

    public AttackTarget(IAgent runner, Transform entity, NavMeshAgent agent, Transform targetPoint, float patrolSpeed)
    {
        this.entity = entity;
        this.agent = agent;
        this.targetPoint = targetPoint;
        this.patrolSpeed = patrolSpeed;
        this.agent.speed = patrolSpeed;
        this.runner = runner;

        if(targetPoint.TryGetComponent(out IEnemyAttackable _enemyAttackable))
        {   
            enemyAttackable = _enemyAttackable;
        }
        else throw new Exception("Enemy attacking something they shouldn't be");

        //Debug.Log("MoveToTarget constructor");
    }

    public Node.Status Process(bool isInterrupted, string leafName)
    {
        if(isInterrupted)
        {
            Reset();
            return Node.Status.Failure; 
        }

        if(Vector3.Distance(entity.position, targetPoint.position) < 1f) 
        {
            Reset();
            return Node.Status.Success;
        }

        enemyAttackable.SetAttackingAgent(runner, true);

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

        Debug.Log("Current Strategy Name: " + leafName);
        runner.setCurrentActiveLeaf(leafName);
        if(runner.getDisplayText().text != "Current Action: " + GetType().Name) runner.getDisplayText().text = "Current Action: " + GetType().Name;
        return Node.Status.Running;
    }

    public void Reset()
    {
        enemyAttackable.SetAttackingAgent(runner, false);
        runner.setCurrentActiveLeaf("");
        if(runner.getDisplayText().text != "") runner.getDisplayText().text = "";
        isPathCalculated = false;
    }

}

public class PatrolStrategy : IStrategy
{
    readonly Transform entity;
    readonly NavMeshAgent agent;
    readonly List<Transform> patrolPoints;
    readonly float patrolSpeed;
    int currentIndex;
    bool isPathCalculated, onFirstRoute;
    IAgent runner;

    public PatrolStrategy(IAgent runner, Transform entity, NavMeshAgent agent, List<Transform> patrolPoints, float patrolSpeed)
    {
        this.entity = entity;
        this.agent = agent;
        this.patrolPoints = patrolPoints;
        this.patrolSpeed = patrolSpeed;
        this.agent.speed = patrolSpeed;
        this.runner = runner;
        onFirstRoute = true;
    }

    public Node.Status Process(bool isInterrupted, string leafName)
    {
        if(isInterrupted)
        {
            Reset();
            return Node.Status.Failure; 
        }

        if(currentIndex == patrolPoints.Count) currentIndex = 0;
        Transform target = patrolPoints[currentIndex];
        agent.SetDestination(target.position);
        entity.LookAt(target);

        Debug.Log("Current patrol isPathCalculated: " + isPathCalculated);

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

        Debug.Log("Current patrol point index: " + currentIndex);
        runner.setCurrentActiveLeaf(leafName);
        if(runner.getDisplayText().text != "Current Action: " + GetType().Name) runner.getDisplayText().text = "Current Action: " + GetType().Name;
        return Node.Status.Running;
    }

    public void Reset()
    {
        runner.setCurrentActiveLeaf("");
        if(runner.getDisplayText().text != "") runner.getDisplayText().text = "";
        currentIndex = 0;
    }
    
}
