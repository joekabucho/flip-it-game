using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ErrorModal : BaseModal
{
	private Text title;
	private Text description;
    private float duration;
    private float startTime;

	public override void Init()
	{
		base.Init ();

		var t = GetComponentsInChildren<Text> ();
		title = t [0];
		description = t [1];
	}
    public void ShowModal(string title, string description, bool exitable)
    {
        Show();

        this.title.text = title;
        this.description.text = description;
        this.isExitable = exitable;
    }

    public void ShowModal(string title, string description, bool exitable,float duration)
    {
        Show();

        this.title.text = title;
        this.description.text = description;
        this.isExitable = exitable;
        this.duration = duration;
        startTime = Time.time;
    }

    protected override void Update()
    {
        if (duration != 0)
        {
            if (Time.time - startTime > duration)
                HideModal();
        }
    }
}