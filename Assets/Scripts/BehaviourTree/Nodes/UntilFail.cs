using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UntilFail : Node
{
    public UntilFail(string name) : base(name)
    {

    }

    public override Status Process(bool isInterrupted)
    {
        if(children[0].Process(isInterrupted) == Status.Failure)
        {
            Reset();
            return Status.Failure;
        }

        return Status.Running;
    }
}
