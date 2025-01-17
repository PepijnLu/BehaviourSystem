using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrioritySequence : Sequence
{
    List<Node> sortedChildren;

    public PrioritySequence(string _name, int _priority = 0) : base(_name, _priority) {}
    List<Node> SortedChildren()
    {
        if(sortedChildren == null)
        {
            sortedChildren = SortChildren();
        }
        return sortedChildren;
    }

    protected virtual List<Node> SortChildren()
    {
        return children.OrderByDescending(child => child.priority).ToList();
    }

    public override void Reset()
    {
        base.Reset();
        currentChild = 0;
        sortedChildren = null;
    }

    public override Status Process(bool _isInterrupted)
    {
        if(currentChild < SortedChildren().Count)
        {
            switch(sortedChildren[currentChild].Process(_isInterrupted))
            {
                case Status.Running:
                    Debug.Log($"Current Node PrioSeq: {sortedChildren[currentChild].name}");
                    bool anotherSuccess = false;
                    for(int i = 0; i < currentChild; i++)
                    {
                        if(sortedChildren[i].Process(_isInterrupted) != Status.Failure)
                        {
                            anotherSuccess = true;
                        }
                    }
                    if(anotherSuccess) Reset();
                    return Status.Running;
                case Status.Success:
                    //currentChild++;
                    Debug.Log($"Node Success PrioSeq: {sortedChildren[currentChild].name}");
                    if (currentChild == children.Count)
                    {
                        return Status.Success;
                    }
                    else
                    {
                        return Status.Running;
                    }
                case Status.Failure:
                    Debug.Log($"Node Failure PrioSeq: {sortedChildren[currentChild].name}");
                    currentChild++;
                    return Status.Running;
            }
        }

        Reset();
        return Status.Failure;
    }
}
