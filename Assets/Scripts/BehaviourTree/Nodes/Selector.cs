using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : Node
{
    public Selector(string name, int priority = 0) : base(name, priority)
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
                case Status.Success:
                    Reset();
                    return Status.Success;
                default:
                    currentChild++;
                    return Status.Running;
            }
        }

        Reset();
        return Status.Failure;
    }
}
;
