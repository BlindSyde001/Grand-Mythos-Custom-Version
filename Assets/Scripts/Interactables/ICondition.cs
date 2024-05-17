namespace Interactables
{
    public interface ICondition
    {
        bool Evaluate();
        bool IsValid(out string error);
    }
}