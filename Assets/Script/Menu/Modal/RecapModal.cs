using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RecapModal : BaseModal 
{   
	public Text description;

    public void ShowModal(bool exitable, int param)
	{
		Show();

		switch (param)
		{
			case -2: description.text = "First time completing the challenge!\n\n+5 Gold"; break;
			case -1: description.text = "You have already completed this challenge."; break;
			case 0: description.text = "Easy puzzle completed!\n\n+1 Gold"; break;
			case 1: description.text = "Medium puzzle completed!\n\n+5 Gold"; break;
			case 2: description.text = "Hard puzzle completed!\n\n+25 Gold"; break;
		}

        this.isExitable = exitable;
    }
}