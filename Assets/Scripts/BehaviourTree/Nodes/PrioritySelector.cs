using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class RandomSelector : PrioritySelector
{
    protected override List<Node> SortChildren()
    {
        System.Random random = new System.Random();
        return children.OrderBy(_ => random.Next()).ToList();
    }

    public RandomSelector(string name, int priority = 0) : base(name, priority)
    {

    }
}
public class PrioritySelector : Selector
{
    List<Node> sortedChildren;

    public PrioritySelector(string name, int priority = 0) : base(name, priority)
    {

    }
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
        sortedChildren = null;
    }

    public override Status Process()
    {
        foreach(Node child in SortedChildren())
        {
            switch(child.Process())
            {
                case Status.Running:
                    return Status.Running;
                case Status.Success:
                    return Status.Success;
                default:
                    continue;
            }
        }

        return Status.Failure;
    }
}
