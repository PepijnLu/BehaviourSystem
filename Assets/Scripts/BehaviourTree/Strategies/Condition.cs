using System;
public class Condition : IStrategy
{
    readonly Func<bool> predicate;

    public Condition(Func<bool> predicate)
    {
        this.predicate = predicate;
    }

    public Node.Status Process(bool isInterrupted, string leafName)
    {
        if (predicate())
        {
            return Node.Status.Success;
        }
        else
        {
            return Node.Status.Failure;
        }
    }
}

