using UnityEngine;

public class Sequence : Node
{
    public Sequence(string _name, int _priority = 0) : base(_name, _priority) {}

    public override Status Process(bool _isInterrupted)
    {
        if(currentChild < children.Count)
        {
            switch(children[currentChild].Process(_isInterrupted))
            {
                case Status.Running:
                    return Status.Running;
                case Status.Failure:
                    Reset();
                    Debug.Log($"Sequence {name} failed at {children[currentChild].name}");
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

