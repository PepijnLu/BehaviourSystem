using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UntilFail : Node
{
    public UntilFail(string name) : base(name)
    {

    }

    public override Status Process()
    {
        if(children[0].Process() == Status.Failure)
        {
            Reset();
            return Status.Failure;
        }

        return Status.Running;
    }
}
