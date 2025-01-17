using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BehaviourTree : Node
{
    public BehaviourTree(string _name) : base(_name) {}

    public override Status Process(bool _isInterrupted = false)
    {
        while (currentChild < children.Count)
        {
            Status status = children[currentChild].Process(_isInterrupted);
            if(status != Status.Success)
            {
                return status;
            }
            currentChild++;
        }
        return Status.Success;
    }
}
