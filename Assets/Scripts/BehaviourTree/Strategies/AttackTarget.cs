using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

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
            Debug.Log("Attack Player Interrupted");

            agent.ResetPath();
            runnerInterface.DropWeapon();
            enemyAttackable.SetAttackingAgent(runner, false);
            enemyAttackable.SetIsBeingAttacked(false);
            runnerInterface.SetCurrentActiveLeaf("");
            if(runnerInterface.GetDisplayText().text != "") runnerInterface.GetDisplayText().text = "";
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

        runnerInterface.GetWeapon().Fire(enemyAttackable);

        runnerInterface.SetCurrentActiveLeaf(leafName);
        if(runnerInterface.GetDisplayText().text != "Current Action: " + leafName) runnerInterface.GetDisplayText().text = "Current Action: " + leafName;
        return Node.Status.Running;
    }

    public void Reset()
    {
        isPathCalculated = false;
    }

}

