public interface IStrategy
{
    Node.Status Process(bool isInterrupted, string leafName);
    void Reset()
    {
        //Noop
    }
}
