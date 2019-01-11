using UnityEngine;
using System.Collections;

public class DifficultyModal : BaseModal
{
	public void ShowModal(bool exitable)
    {
        Show ();

        this.isExitable = exitable;
    }

    public void ToGame(int diff)
    {
    	MenuManager.instance.ToGame(GameMode.FreePlay,diff);
    }
}