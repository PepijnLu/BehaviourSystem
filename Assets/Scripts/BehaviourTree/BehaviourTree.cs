using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BehaviourTree : Node
{
    public BehaviourTree(string name) : base(name)
    {

    }

    public override Status Process(bool isInterrupted = false)
    {
        while (currentChild < children.Count)
        {
            Status status = children[currentChild].Process(isInterrupted);
            if(status != Status.Success)
            {
                return status;
            }
            currentChild++;
        }
        return Status.Success;
    }
}
