using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum CanvasPosition
{
	Main,
	Shop,
	Progress,
	LevelSelection
}

public enum GameMode
{
	Challenge,
	FreePlay
}

[System.Serializable]
public struct FillItemList
{
	public GameObject ItemRowPrefab;
	public GameObject ItemPrefab;
	public GameObject ItemListContainer;
}

public class MenuManager : MonoSingleton<MenuManager> 
{
	[SerializeField]
	public FillItemList[] menuFillItem;
    public Color completedLevelColor;
    public Sprite selectableLevelSprite;
    public CanvasGroup shopCanvasGroup;

	private CanvasGroup rootCanvas;
	private RectTransform thisTransform;
	private float startTransition = 0.0f;
	private float startTransitionDuration = 1.0f;
	private bool startTransitionActive = true;
	private bool isLoadingGame = false;

	private float positionTransition;
	private Vector2 positionStartPosition;
	private Vector2 desiredCanvasPosition;
	private float positionTransitionDuration = 0.5f;
	private bool isInPositionTransition;
	private Button[] allShopButtons;

	public AnimationCurve progressCurve;
	private bool isProgressPlaying;
	private bool hasProgressPlayed;
	private float progressTransition;
	private float[] desiredRatioProgress;

	public Vector2[] menuPosition;

    // GoldText
    public Text goldText;
    public Text[] bestTimes;

    // Sound
    public Button noSound;
    public Sprite[] soundSprite;

    // Progress
    public GameObject[] progressObjects;

    public void RefreshGold()
    {
        goldText.text = 'x' + DataHelper.instance.SaveState.Gold.ToString();
    }

	public override void Init()
	{
		rootCanvas = GameObject.FindGameObjectWithTag ("UIRoot").GetComponent<CanvasGroup> ();
		rootCanvas.alpha = 0;
		thisTransform = rootCanvas.GetComponent<RectTransform> ();

        RefreshGold();
        bestTimes[0].text = (DataHelper.instance.SaveState.EasyTime != 0)
			?"Easy : " + ConvertToReadable(DataHelper.instance.SaveState.EasyTime)
			:"Easy : --.--";
		bestTimes[1].text = (DataHelper.instance.SaveState.MediumTime != 0)
			?"Medium : " + ConvertToReadable(DataHelper.instance.SaveState.MediumTime)
			:"Medium : --.--";
		bestTimes[2].text = (DataHelper.instance.SaveState.HardTime != 0)
			?"Hard : " + ConvertToReadable(DataHelper.instance.SaveState.HardTime)
			:"Hard : --.--";

		// GameSpec
        SetLevelState();
        SetShopState();
		SetProgress();
	//	FillShop (menuFillItem[0]);
	//	FillLevel (menuFillItem [1]);

		DataHelper.instance.ConnectToGoogleServices ();
		noSound.image.sprite = (DataHelper.instance.SaveState.SoundEnabled) ? soundSprite[0] : soundSprite[1];
	}
	private string ConvertToReadable(float v)
	{
		string r = "";
			// Minutes
		r += ((int)v / 60).ToString("00") + "m ";
			// Secondes
		r += (v % 60).ToString("00") + 's';
		return r;
	}
	private void Update()
	{
        if (startTransitionActive)
        {
            startTransition += (Time.deltaTime) * 1 / startTransitionDuration;
            rootCanvas.alpha = Mathf.Lerp(0, 1, startTransition);

            if (startTransition > 1)
                startTransitionActive = false;
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

		if (isInPositionTransition)
		{
			positionTransition += Time.deltaTime * 1 / positionTransitionDuration;
			thisTransform.anchoredPosition = Vector2.Lerp(positionStartPosition, desiredCanvasPosition, positionTransition * (Mathf.PI / 2));

			if (positionTransition > 1)
			{
				isInPositionTransition = false;
				positionTransition = 0;
			}
		}

		if (isProgressPlaying)
		{
			PlayProgress();
		}

		if(isLoadingGame && SceneManager.GetSceneByName ("Game").isLoaded)
		{
			isLoadingGame = false;
			GameCanvas.instance.InitGameCanvas ();
		}
	}

	public void PlayButton()
	{
        DataHelper.instance.PlaySFX(2);
        DataHelper.instance.ShowDifficulty(true);
	}

	public void StartAtPosition(CanvasPosition pos)
	{
		switch (pos) 
		{
		case CanvasPosition.Main:
			desiredCanvasPosition = menuPosition [0];
			break;
		case CanvasPosition.LevelSelection:
			desiredCanvasPosition = menuPosition [1];
			break;
		case CanvasPosition.Progress:
			desiredCanvasPosition = menuPosition [2];
			if(!hasProgressPlayed)
				isProgressPlaying = true;
			break;
		case CanvasPosition.Shop:
			desiredCanvasPosition = menuPosition [3];
			break;
		}
		GameObject.FindGameObjectWithTag ("UIRoot").GetComponent<RectTransform>().anchoredPosition = desiredCanvasPosition;
	}
	public void PositionCanvas(CanvasPosition pos)
	{
		isInPositionTransition = true;
		positionStartPosition = thisTransform.anchoredPosition;


		switch (pos) 
		{
		case CanvasPosition.Main:
			desiredCanvasPosition = menuPosition [0];
			break;
		case CanvasPosition.LevelSelection:
			desiredCanvasPosition = menuPosition [1];
			break;
		case CanvasPosition.Progress:
			desiredCanvasPosition = menuPosition [2];
			if(!hasProgressPlayed)
				isProgressPlaying = true;
			break;
		case CanvasPosition.Shop:
			desiredCanvasPosition = menuPosition [3];
			break;
		}
	}

	#region GameSpecific
    private void SetLevelState()
    {
        Transform t = menuFillItem[1].ItemListContainer.transform;

        bool unlocked = true;
        int i = 0;
        foreach (Button b in t.GetComponentsInChildren<Button>())
        {
            Image img = b.GetComponent<Image>();
            b.interactable = unlocked;
            if(unlocked)
               img.sprite = selectableLevelSprite;

            unlocked = DataHelper.instance.SaveState.unlockedLevel.Get(i);
            img.color = (unlocked)?completedLevelColor:Color.white;
            i++;
        }
    }
    private void SetShopState()
	{
		allShopButtons = menuFillItem[0].ItemListContainer.transform.GetComponentsInChildren<Button>();

		// First Item
		DataHelper.instance.SaveState.unlockedItem.Set(0,true);

		for (int i = 0; i < allShopButtons.Length; i++)
			SetShopItemState(i);
    }
    private void SetProgress()
    {
		int 	item = 0,
				level = 0,
				achievement = 0;
		float 	ratio = 0,
				total = 0;

		desiredRatioProgress = new float[]{0,0,0,0};

		//Items
		for(int i = 0; i < DataHelper.instance.SaveState.unlockedItem.Length; i++)
			if(DataHelper.instance.SaveState.unlockedItem.Get(i))
				item++;

		ratio = (float)item / (float)DataHelper.instance.SaveState.unlockedItem.Length;
		total += ratio;
		progressObjects[1].GetComponentInChildren<Text>().text = (ratio * 100).ToString("0") + '%';
		desiredRatioProgress[0] = ratio;
	//	progressObjects[1].GetComponentsInChildren<Image>()[1].fillAmount = ratio;

		//Levels
		for(int i = 0; i < DataHelper.instance.SaveState.unlockedLevel.Length; i++)
			if(DataHelper.instance.SaveState.unlockedLevel.Get(i))
				level++;
	
		ratio = (float)level / (float)DataHelper.instance.SaveState.unlockedLevel.Length;
		total += ratio;
		progressObjects[2].GetComponentInChildren<Text>().text = (ratio * 100).ToString("0") + '%';
	//	progressObjects[2].GetComponentsInChildren<Image>()[1].fillAmount = ratio;
		desiredRatioProgress[1] = ratio;

		//Achievements
		for(int i = 0; i < DataHelper.instance.SaveState.unlockedAchievement.Length; i++)
			if(DataHelper.instance.SaveState.unlockedAchievement.Get(i))
				achievement++;
	
		ratio = (float)achievement / (float)DataHelper.instance.SaveState.unlockedAchievement.Length;
		total += ratio;
		progressObjects[3].GetComponentInChildren<Text>().text = (ratio * 100).ToString("0") + '%';
	//	progressObjects[3].GetComponentsInChildren<Image>()[1].fillAmount = ratio;
		desiredRatioProgress[2] = ratio;

		//Totals
		ratio = total / (progressObjects.Length-1);
		total += ratio;
		progressObjects[0].GetComponentInChildren<Text>().text = (ratio * 100).ToString("0") + '%';
	//	progressObjects[0].GetComponentsInChildren<Image>()[1].fillAmount = ratio;
		desiredRatioProgress[3] = ratio;
    }

	private void PlayProgress()
	{
		progressTransition += (1.0f / 3.0f) * Time.deltaTime;
		float t = progressCurve.Evaluate(progressTransition);
		progressObjects[1].GetComponentsInChildren<Image>()[1].fillAmount = Mathf.Lerp(0, desiredRatioProgress[0], t);
		progressObjects[2].GetComponentsInChildren<Image>()[1].fillAmount = Mathf.Lerp(0, desiredRatioProgress[1], t);
		progressObjects[3].GetComponentsInChildren<Image>()[1].fillAmount = Mathf.Lerp(0, desiredRatioProgress[2], t);
		progressObjects[0].GetComponentsInChildren<Image>()[1].fillAmount = Mathf.Lerp(0, desiredRatioProgress[3], t);

		if (t > 1)
		{
			isProgressPlaying = false;
			hasProgressPlayed = true;
        }
    }
	private void SetShopItemState(int index)
	{
		Button b = allShopButtons[index];
		Image img = b.GetComponent<Image>();
		Button btn = b.GetComponent<Button>();
		Text txt = b.GetComponentInChildren<Text>();
		ItemData item = DataHelper.instance.Items[index];
		btn.onClick.RemoveAllListeners();
		btn.onClick.AddListener(() => BuyFromShop(item));
		bool unlocked = DataHelper.instance.SaveState.unlockedItem.Get(index);

		if (unlocked)
		{
			img.sprite = DataHelper.instance.GetSpriteAt(index);
			if (index == DataHelper.instance.SaveState.SelectedImage)
			{
				txt.text = "ACTIVE";
				img.GetComponentsInChildren<Image>()[1].color = new Color(0.6235f,0.7372f,1f);
			}
			else
			{
				txt.text = item.ItemName;
				img.GetComponentsInChildren<Image>()[1].color = Color.white;
			}
		}
		else
		{
			txt.text = item.GoldCost.ToString();
		}
	}
/*	private void FillShop(FillItemList s)
	{
		int shopSizeX = 4;
		int i = 0;
		GameObject row = Instantiate (s.ItemRowPrefab);
		row.transform.SetParent (s.ItemListContainer.transform);
		row.transform.localScale = Vector3.one;

		foreach (var item in DataHelper.instance.Items)
		{
			if(i >= shopSizeX)
			{
				row = Instantiate(s.ItemRowPrefab);
				row.transform.SetParent (s.ItemListContainer.transform);
				row.transform.localScale = Vector3.one;
				i = 0;
			}
			
			GameObject go = Instantiate (s.ItemPrefab);
			go.transform.SetParent (row.transform);
			go.transform.localScale = Vector3.one;

			var img = go.GetComponent<Image> ();
			var btn = go.GetComponent<Button> ();
			var txt = go.GetComponentInChildren<Text> ();
			var itemRef = item;

			btn.onClick.AddListener (() => BuyFromShop (itemRef));

			if (DataHelper.instance.SaveState.unlockedItem.Get(i)) 
			{
				img.sprite = DataHelper.instance.GetSpriteAt(i);
			}
			else 
			{
				txt.text = item.GoldCost.ToString ();
			}

			i++;
		}
	}
	private void FillLevel(FillItemList s)
	{
		int shopSizeX = 4;
		int i = 0;
		GameObject row = Instantiate (s.ItemRowPrefab);
		row.transform.SetParent (s.ItemListContainer.transform);
		row.transform.localScale = Vector3.one;
		foreach (var level in DataHelper.instance.Levels)
		{
			if(i >= shopSizeX)
			{
				row = Instantiate(s.ItemRowPrefab);
				row.transform.SetParent (s.ItemListContainer.transform);
				row.transform.localScale = Vector3.one;
				i = 0;
			}

			GameObject go = Instantiate (s.ItemPrefab);
			go.transform.SetParent (row.transform);
			go.transform.localScale = Vector3.one;

			var img = go.GetComponent<Image> ();
			var btn = go.GetComponent<Button> ();
			var txt = go.GetComponentInChildren<Text> ();

			int id = level.Id;
			btn.onClick.AddListener (() => ToChallenge (id));

			if (level.Unlocked) 
			{

			}
			else 
			{
				txt.text = level.Id.ToString();
			}

			i++;
		}
	}*/
	private void BuyFromShop(ItemData item)
	{
		int s = DataHelper.instance.Items.FindIndex(i => i == item);
		int ps = DataHelper.instance.SaveState.SelectedImage;

		if (DataHelper.instance.SaveState.unlockedItem.Get(s))
		{
            DataHelper.instance.PlaySFX(1);
            DataHelper.instance.SaveState.SelectedImage = s;
			DataHelper.instance.SaveState.Save(SaveMethod.Local);

			SetShopItemState(s);
			SetShopItemState(ps);
		}
		else
		{
			if (DataHelper.instance.SaveState.Gold >= item.GoldCost)
			{
                DataHelper.instance.PlaySFX(1);
                DataHelper.instance.SaveState.Gold -= item.GoldCost;
				DataHelper.instance.SaveState.unlockedItem.Set(s, true);
				DataHelper.instance.SaveState.SelectedImage = s;
				DataHelper.instance.SaveState.Save(SaveMethod.Local);

				SetShopItemState(s);
				SetShopItemState(ps);
				goldText.text = 'x' + DataHelper.instance.SaveState.Gold.ToString();

                switch(DataHelper.instance.SaveState.UnlockedItemCount())
                {
                    case 2: DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_reskinning); break;
                    case 10: DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_10_skins); break;
                    case 20: DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_20_skins); break;
                    case 30: DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_30_skins); break;
                    case 40: DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_40_skins); break;
                    case 50: DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_50_skins); break;
                }

                if (item.Description == "final")
                    DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_thanks_for_playing);

                RefreshProgress();
            }
			else
            {
                DataHelper.instance.PlaySFX(0);
                DataHelper.instance.ShowError("Not enough Gold", "You will need an additional " + Mathf.Abs(DataHelper.instance.SaveState.Gold - item.GoldCost).ToString() + " gold to buy this", true);
                shopCanvasGroup.blocksRaycasts = false;
            }
		}
	}
	public void ToChallenge(int id)
	{
		ToGame (GameMode.Challenge, id);
	}
    #endregion

    private void RefreshProgress()
    {
        SetProgress();
        progressTransition = 0;
        progressObjects[1].GetComponentsInChildren<Image>()[1].fillAmount = 0;
        progressObjects[2].GetComponentsInChildren<Image>()[1].fillAmount = 0;
        progressObjects[3].GetComponentsInChildren<Image>()[1].fillAmount = 0;
        progressObjects[0].GetComponentsInChildren<Image>()[1].fillAmount = 0;

        isProgressPlaying = false;
        hasProgressPlayed = false;
    }

    #region ButtonActions
    public void ToMenu()
	{
		PositionCanvas (CanvasPosition.Main);
		DataHelper.instance.PlaySFX(3);
	}
	public void ToGame(GameMode mode, int param)
	{
        if (DataHelper.instance.InSceneTransition)
            return;

		if (mode == GameMode.Challenge)
		{
			SceneManager.LoadScene("Game", LoadSceneMode.Additive);
			DataHelper.instance.CurrentGameMode = mode;
			DataHelper.instance.CurrentLevel = param;
			isLoadingGame = true;
            DataHelper.instance.PlaySFX(3);
            DataHelper.instance.InSceneTransition = true;
        }
		else if (mode == GameMode.FreePlay)
		{
			// Generate level
			Debug.Log("Starting a level on diff : " + param);
			SceneManager.LoadScene("Game", LoadSceneMode.Additive);
			DataHelper.instance.CurrentGameMode = mode;
			DataHelper.instance.CurrentLevel = param;
			isLoadingGame = true;
            DataHelper.instance.PlaySFX(3);
            DataHelper.instance.InSceneTransition = true;
        }
	}
	public void ToLevelSelection()
	{
        DataHelper.instance.PlaySFX(3);
        PositionCanvas (CanvasPosition.LevelSelection);
	}
    public void ToTutorial()
    {
        Handheld.PlayFullScreenMovie("Tutorial.mp4", Color.black);
        //    DataHelper.instance.ShowTutorial(true);
    }
	public void ToProgress()
	{
		PositionCanvas (CanvasPosition.Progress);
        DataHelper.instance.PlaySFX(3);
    }
	public void ToShop()
	{
		PositionCanvas (CanvasPosition.Shop);
        DataHelper.instance.PlaySFX(3);
    }
	public void ToGift()
	{
		DataHelper.instance.ShowGift (true);
		DataHelper.instance.PlaySFX(2);
	}
	public void ToLeaderboard()
	{
		if (Social.localUser.authenticated)
		{
            if (Social.localUser.authenticated)
            {
                Social.ReportScore((int)(desiredRatioProgress[3] * 100), "CgkIw_PP7OYOEAIQHA", (bool success) =>
                {

                });
            }
            Social.ShowLeaderboardUI ();
		} 
		else
		{
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    DataHelper.instance.IsConnectedToGoogleServices = true;
                    Social.ShowLeaderboardUI();
                }
                else
                {
                    DataHelper.instance.PlaySFX(0);
                    DataHelper.instance.ShowError("Achievement", "Unable to connect to google play services", true);
                }
            });
        }
	}
	public void ToAchievement()
	{
		if (Social.localUser.authenticated)
		{
			Social.ShowAchievementsUI ();
		} 
		else
		{
            Social.localUser.Authenticate((bool success) => 
            {
                if (success)
                {
                    DataHelper.instance.IsConnectedToGoogleServices = true;
                    Social.ShowAchievementsUI();
                }
                else
                {
                    DataHelper.instance.PlaySFX(0);
                    DataHelper.instance.ShowError("Achievement", "Unable to connect to google play services", true);
                }
            });
		}
	}
	public void ToLike()
	{
        DataHelper.instance.PlaySFX(2);
        DataHelper.instance.ShowContact(true);
	}
	public void ToRate()
	{
		Application.OpenURL (DataHelper.instance.rateUrl);
	}
	public void NoAds()
	{
        DataHelper.instance.PlaySFX(2);
        DataHelper.instance.ShowIAP (true);
	}
	public void SoundToggle()
	{
		DataHelper.instance.SaveState.SoundEnabled = !DataHelper.instance.SaveState.SoundEnabled;
		noSound.image.sprite = (DataHelper.instance.SaveState.SoundEnabled) ? soundSprite[0] : soundSprite[1];
        DataHelper.instance.PlaySFX(2);
        DataHelper.instance.SaveState.Save(SaveMethod.Local);
	}
	#endregion
}