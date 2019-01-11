using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameCanvas : MonoSingleton<GameCanvas> 
{
	public Image bg;

	private Color bgColor;
	private float bgTransition = 1;
	private RectTransform thisTransform;
	private float startTransition = 0.0f;
	private float startTransitionDuration = 1.0f;
	private bool startTransitionActive = true;
	private Vector3 startPosition;

	public void InitGameCanvas()
	{
		thisTransform = GameObject.FindGameObjectsWithTag ("UIRoot")[1].GetComponent<RectTransform> ();
		startPosition = thisTransform.anchoredPosition;
		bgColor = bg.color;
	}

	private void Update()
	{
		if (startTransitionActive)
		{
			startTransition += (1 / startTransitionDuration) * 0.02f;
			thisTransform.anchoredPosition = Vector3.Lerp(startPosition, Vector3.zero, startTransition);

			if (startTransition > 1)
			{
				startTransitionActive = false;
				SceneManager.UnloadScene("Menu");
                DataHelper.instance.InSceneTransition = false;
            }
		}
		else
		{
			if (bgTransition > 0)
			{
				bg.color = new Color(bgColor.r, bgColor.g, bgColor.b, Mathf.Lerp(0.6f,1,bgTransition));
				bgTransition -= 0.02f * 1.0f / 3.0f;
			}
		}
	}
}