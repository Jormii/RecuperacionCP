public interface IAction
{
    void Execute();
    void Cancel();
    bool CanBeCancelled
    {
        get;
    }
}
