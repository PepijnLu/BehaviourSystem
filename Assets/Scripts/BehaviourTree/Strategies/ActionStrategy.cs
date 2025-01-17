using System;
public class ActionStrategy : IStrategy
{
    readonly Action doSomething;

    public ActionStrategy(Action doSomething)
    {
        this.doSomething = doSomething; 
    }

    public Node.Status Process(bool isInterrupted, string leafName)
    {
        doSomething();
        return Node.Status.Success;
    }
}

