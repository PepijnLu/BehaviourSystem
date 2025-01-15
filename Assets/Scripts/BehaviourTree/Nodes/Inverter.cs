using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inverter : Node
{
    public Inverter(string name) : base(name)
    {

    }   

    public override Status Process(bool isInterrupted)
    {
        //Possibly extend this to work for multiple children
        switch(children[0].Process(isInterrupted))
        {
            case Status.Running:
                return Status.Running;
            case Status.Failure:
                return Status.Success;
            default:
                return Status.Failure;
        }
    }
}
