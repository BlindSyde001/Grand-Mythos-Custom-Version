using System.Collections;

public class QuitMenuActions : MenuContainer
{
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        gameObject.SetActive(true);
        yield break;
    }

    public override IEnumerable Close(MenuInputs menuInputs)
    {
        gameObject.SetActive(false);
        yield break;
    }
}
