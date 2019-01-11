using UnityEngine;
using System.Collections;

public abstract class BaseModal : MonoBehaviour 
{
	protected CanvasGroup canvasGroup;
	protected bool isExitable;

	protected float transition;
	protected bool inTransition;
	protected bool isPopping;
	protected float transitionDuration = 0.25f;

	public virtual void Init()
	{
		canvasGroup = GetComponent<CanvasGroup> ();
		canvasGroup.alpha = 0;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
	}
	protected virtual void Update()
	{
		if (inTransition)
		{
			float f = (Time.deltaTime * 1 / transitionDuration);
			transition += (isPopping) ? f: -f;

			if (transition < 0 && !isPopping)
			{
				inTransition = false;
				Destroy (gameObject);
			}
			else if (transition > 1 && isPopping)
			{
				inTransition = false;
			}

			canvasGroup.alpha = transition;
		}
	}
	protected virtual void Show()
	{
		Transform t = GameObject.FindGameObjectWithTag ("UIRoot").transform;
		transform.SetParent (t,true);

		transform.localScale = Vector3.one;
		((RectTransform)transform).sizeDelta = Vector2.zero;
		((RectTransform)transform).anchoredPosition = -transform.parent.GetComponent<RectTransform>().anchoredPosition;

		inTransition = true;
		isPopping = true;

		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
	}
	public void HideModal()
	{
		inTransition = true;
		isPopping = false;

		canvasGroup.alpha = 0;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
	}

	public void Command(string s)
	{
		switch (s) {
		case "shop":
                DataHelper.instance.PlaySFX(3);
                DataHelper.instance.LoadMenuAt (CanvasPosition.Shop);
			break;
		case "home":
                DataHelper.instance.PlaySFX(3);
                DataHelper.instance.LoadMenuAt (CanvasPosition.Main);
			break;
		case "progress":
                DataHelper.instance.PlaySFX(3);
                DataHelper.instance.LoadMenuAt (CanvasPosition.Progress);
			break;
		case "like":
                Application.OpenURL (DataHelper.instance.likeUrl);
			break;
		case "rate":
                Application.OpenURL (DataHelper.instance.rateUrl);
			break;
		case "youtube":
                Application.OpenURL (DataHelper.instance.youtubeUrl);
			break;
		case "achievement":
			if (DataHelper.instance.IsConnectedToGoogleServices)
				Debug.Log ("Show Achievements");
			else
				Command ("home");
			break;
		case "launchNextLevel":
                if (GameManager.instance.NextLevel ())
                {
                    DataHelper.instance.PlaySFX(3);
                    HideModal();
                }
			break;
			
		default:
			break;
		}
	}
	public void OutsideButton()
	{
		if (isExitable)
			HideModal ();

        if (GameObject.Find("MaskSHOP"))
            GameObject.Find("MaskSHOP").GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}
