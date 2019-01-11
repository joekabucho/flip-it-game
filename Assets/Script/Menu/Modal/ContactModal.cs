using UnityEngine;
using System.Collections;

public class ContactModal : BaseModal
{
	public void ShowModal(bool exitable)
    {
        Show ();

        this.isExitable = exitable;
        DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_curious);
    }
}