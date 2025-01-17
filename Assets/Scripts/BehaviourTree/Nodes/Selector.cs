using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : Node
{
    public Selector(string _name, int _priority = 0) : base(_name, _priority) {}

    public override Status Process(bool _isInterrupted)
    {
        if(currentChild < children.Count)
        {
            switch(children[currentChild].Process(_isInterrupted))
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
