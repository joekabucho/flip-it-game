using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class PauseModal : BaseModal
{
	private Text title;
	private Text description;

	public override void Init()
	{
		base.Init ();

		var t = GetComponentsInChildren<Text> ();
		title = t [0];
		description = t [1];
	}
	public void ShowModal(string title,string description,bool exitable)
	{
		Show ();

		this.title.text = title;
		this.description.text = description;
		this.isExitable = exitable;
	}
}