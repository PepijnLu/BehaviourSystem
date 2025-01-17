using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UntilFail : Node
{
    public UntilFail(string _name) : base(_name) {}

    public override Status Process(bool _isInterrupted)
    {
        if(children[0].Process(_isInterrupted) == Status.Failure)
        {
            Reset();
            return Status.Failure;
        }

        return Status.Running;
    }
}
