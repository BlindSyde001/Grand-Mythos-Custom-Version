using System.Collections;
using UnityEngine;

public class JournalMenuActions : MenuContainer
{
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        gameObject.SetActive(true);
        yield return new WaitForSeconds(menuInputs.Speed);
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
    }
}
