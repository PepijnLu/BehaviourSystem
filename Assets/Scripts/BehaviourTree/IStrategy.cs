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
    bool moving;
    readonly Vector3 targetPosition;
    readonly GameObject runner;
    readonly Transform entity;
    readonly NavMeshAgent agent;
    readonly Transform targetPoint;
    readonly float patrolSpeed;
    bool isPathCalculated;
    IAgent runnerInterface;

    public MoveToTarget(GameObject runner, Transform entity, NavMeshAgent agent, Transform targetPoint, float patrolSpeed)
    {
        this.entity = entity;
        this.agent = agent;
        this.targetPoint = targetPoint;
        this.patrolSpeed = patrolSpeed;
        this.agent.speed = patrolSpeed;
        this.runner = runner;

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
            runnerInterface.setCurrentActiveLeaf("");
            if(runnerInterface.getDisplayText().text != "") runnerInterface.getDisplayText().text = "";
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

        runnerInterface.setCurrentActiveLeaf(leafName);
        if(runnerInterface.getDisplayText().text != "Current Action: " + leafName) runnerInterface.getDisplayText().text = "Current Action: " + leafName;
        return Node.Status.Running;
    }

    public void Reset()
    {
        isPathCalculated = false;
    }

}


public class AttackTarget : IStrategy
{
    readonly GameObject runner;
    readonly Transform entity;
    readonly NavMeshAgent agent;
    readonly Transform targetPoint;
    readonly IEnemyAttackable enemyAttackable;
    readonly float patrolSpeed;
    bool isPathCalculated;
    IAgent runnerInterface;

    public AttackTarget(GameObject runner, Transform entity, NavMeshAgent agent, Transform targetPoint, float patrolSpeed)
    {
        this.entity = entity;
        this.agent = agent;
        this.targetPoint = targetPoint;
        this.patrolSpeed = patrolSpeed;
        this.agent.speed = patrolSpeed;
        this.runner = runner;

        if(agent.TryGetComponent(out IAgent _runnerInterface))
        {
            runnerInterface = _runnerInterface;
        }
        else throw new Exception($"Runner {runner.name} isn't an agent");

        if(targetPoint.TryGetComponent(out IEnemyAttackable _enemyAttackable))
        {   
            enemyAttackable = _enemyAttackable;
        }
        else throw new Exception("Enemy attacking something they shouldn't be");
    }

    public Node.Status Process(bool isInterrupted, string leafName)
    {
        if(isInterrupted)
        {
            agent.ResetPath();
            runnerInterface.DropWeapon();
            enemyAttackable.SetAttackingAgent(runner, false);
            enemyAttackable.setIsBeingAttacked(false);
            runnerInterface.setCurrentActiveLeaf("");
            if(runnerInterface.getDisplayText().text != "") runnerInterface.getDisplayText().text = "";
            Reset();
            return Node.Status.Failure; 
        }

        if(Vector3.Distance(entity.position, targetPoint.position) < 1f) 
        {
            agent.ResetPath();
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

        runnerInterface.GetWeapon().Fire();

        runnerInterface.setCurrentActiveLeaf(leafName);
        if(runnerInterface.getDisplayText().text != "Current Action: " + GetType().Name) runnerInterface.getDisplayText().text = "Current Action: " + GetType().Name;
        return Node.Status.Running;
    }

    public void Reset()
    {
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
    GameObject runner;
    IAgent runnerInterface;

    public PatrolStrategy(GameObject runner, Transform entity, NavMeshAgent agent, List<Transform> patrolPoints, float patrolSpeed)
    {
        this.entity = entity;
        this.agent = agent;
        this.patrolPoints = patrolPoints;
        this.patrolSpeed = patrolSpeed;
        this.agent.speed = patrolSpeed;
        this.runner = runner;
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
            runnerInterface.setCurrentActiveLeaf("");
            if(runnerInterface.getDisplayText().text != "") runnerInterface.getDisplayText().text = "";
            Reset();
            return Node.Status.Failure; 
        }

        if(currentIndex == patrolPoints.Count) currentIndex = 0;
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

        runnerInterface.setCurrentActiveLeaf(leafName);
        if(runnerInterface.getDisplayText().text != "Current Action: " + GetType().Name) runnerInterface.getDisplayText().text = "Current Action: " + GetType().Name;
        return Node.Status.Running;
    }

    public void Reset()
    {
        currentIndex = 0;
    }
    
}

public class Idle : IStrategy
{
    IAgent runnerInterface;
    GameObject runner;
    public Idle(GameObject runner, NavMeshAgent _agent)
    {
        if(_agent.TryGetComponent(out IAgent _runnerInterface))
        {
            runnerInterface = _runnerInterface;
        }
        else throw new Exception($"Runner {runner.name} isn't an agent");

        this.runner = runner;
    }

    public Node.Status Process(bool isInterrupted, string leafName)
    {
        Debug.Log("Idle");

        if(isInterrupted)
        {
            runnerInterface.setCurrentActiveLeaf("");
            if(runnerInterface.getDisplayText().text != "") runnerInterface.getDisplayText().text = "";
            Reset();
            return Node.Status.Failure; 
        }

        runnerInterface.setCurrentActiveLeaf(leafName);
        if(runnerInterface.getDisplayText().text != "Current Action: " + GetType().Name) runnerInterface.getDisplayText().text = "Current Action: " + GetType().Name;
        Debug.Log("Text should be: " + runnerInterface.getDisplayText().text);
        return Node.Status.Running;
    }

    public void Reset()
    {

    }
    
}

public class BeStunned : IStrategy
{
    IAgent runnerInterface;
    GameObject runner;
    public BeStunned(GameObject _runner, NavMeshAgent _agent)
    {
        if(_agent.TryGetComponent(out IAgent _runnerInterface))
        {
            runnerInterface = _runnerInterface;
        }
        else throw new Exception($"Runner {runner.name} isn't an agent");

        this.runner = _runner;
    }

    public Node.Status Process(bool isInterrupted, string leafName)
    {
        if(isInterrupted)
        {
            runnerInterface.setCurrentActiveLeaf("");
            if(runnerInterface.getDisplayText().text != "") runnerInterface.getDisplayText().text = "";
            Reset();
            return Node.Status.Success; 
        }

        runnerInterface.setCurrentActiveLeaf(leafName);
        if(runnerInterface.getDisplayText().text != "Current Action: " + GetType().Name) runnerInterface.getDisplayText().text = "Current Action: " + GetType().Name;
        return Node.Status.Running;
    }

    public void Reset()
    {

    }
    
}
