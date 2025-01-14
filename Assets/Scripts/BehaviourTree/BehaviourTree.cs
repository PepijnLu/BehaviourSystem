using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BehaviourTree : Node
{
    public BehaviourTree(string name) : base(name)
    {

    }

    public override Status Process()
    {
        while (currentChild < children.Count)
        {
            Status status = children[currentChild].Process();
            if(status != Status.Success)
            {
                return status;
            }
            currentChild++;
        }
        return Status.Success;
    }
}
