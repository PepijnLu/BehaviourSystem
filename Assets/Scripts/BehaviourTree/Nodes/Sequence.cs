using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequence : Node
{
    public Sequence(string name, int priority = 0) : base(name, priority)
    {

    }

    public override Status Process(bool isInterrupted)
    {
        if(currentChild < children.Count)
        {
            switch(children[currentChild].Process(isInterrupted))
            {
                case Status.Running:
                    return Status.Running;
                case Status.Failure:
                    Reset();
                    return Status.Failure;
                default:
                    currentChild++;
                    //return currentChild == children.Count ? Status.Success : Status.Running;
                    if (currentChild == children.Count)
                    {
                        Debug.Log($"Sequence {name} completed");
                        return Status.Success;
                    }
                    else
                    {
                        return Status.Running;
                    }
            }
        }

        Reset();
        Debug.Log($"Sequence {name} completed");
        return Status.Success;
    }
}

