using System.Collections.Generic;
using System.Linq;
public class RandomSelector : PrioritySelector
{
    protected override List<Node> SortChildren()
    {
        System.Random random = new System.Random();
        return children.OrderBy(_ => random.Next()).ToList();
    }

    public RandomSelector(string _name, int _priority = 0) : base(_name, _priority) {}
}
public class PrioritySelector : Selector
{
    List<Node> sortedChildren;

    public PrioritySelector(string _name, int _priority = 0) : base(_name, _priority) {}
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

    public override Status Process(bool _isInterrupted)
    {
        foreach(Node child in SortedChildren())
        {
            switch(child.Process(_isInterrupted))
            {
                case Status.Running:
                    return Status.Running;
                case Status.Success:
                    Reset();
                    return Status.Success;
                default:
                    continue;
            }
        }

        Reset();
        return Status.Failure;
    }
}
