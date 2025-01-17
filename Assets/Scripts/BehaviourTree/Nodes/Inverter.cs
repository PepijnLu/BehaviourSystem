public class Inverter : Node
{
    public Inverter(string _name) : base(_name) {}   

    public override Status Process(bool _isInterrupted)
    {
        switch(children[0].Process(_isInterrupted))
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
