using System;
using UnityEngine;
using UnityEngine.AI;

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

        runner = _runner;
    }

    public Node.Status Process(bool isInterrupted, string leafName)
    {
        if(isInterrupted)
        {
            runnerInterface.SetCurrentActiveLeaf("");
            if(runnerInterface.GetDisplayText().text != "") runnerInterface.GetDisplayText().text = "";
            Reset();
            return Node.Status.Success; 
        }

        runnerInterface.SetCurrentActiveLeaf(leafName);
        if(runnerInterface.GetDisplayText().text != "Current Action: " + GetType().Name) runnerInterface.GetDisplayText().text = "Current Action: " + GetType().Name;
        return Node.Status.Running;
    }

    public void Reset()
    {
        //Noop
    }
    
}

