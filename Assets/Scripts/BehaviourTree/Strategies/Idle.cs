using System;
using UnityEngine;
using UnityEngine.AI;

public class Idle : IStrategy
{
    IAgent runnerInterface;
    public Idle(GameObject runner, NavMeshAgent _agent)
    {
        if(_agent.TryGetComponent(out IAgent _runnerInterface))
        {
            runnerInterface = _runnerInterface;
        }
        else throw new Exception($"Runner {runner.name} isn't an agent");
    }

    public Node.Status Process(bool isInterrupted, string leafName)
    {
        Debug.Log("Idle");

        if(isInterrupted)
        {
            runnerInterface.SetCurrentActiveLeaf("");
            if(runnerInterface.GetDisplayText().text != "") runnerInterface.GetDisplayText().text = "";
            Reset();
            return Node.Status.Failure; 
        }

        runnerInterface.SetCurrentActiveLeaf(leafName);
        if(runnerInterface.GetDisplayText().text != "Current Action: " + GetType().Name) runnerInterface.GetDisplayText().text = "Current Action: " + GetType().Name;
        Debug.Log("Text should be: " + runnerInterface.GetDisplayText().text);
        return Node.Status.Running;
    }

    public void Reset()
    {
        //Noop
    }
    
}

