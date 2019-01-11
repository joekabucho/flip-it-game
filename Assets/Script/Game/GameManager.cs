using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Advertisements;
using UnityEngine.EventSystems;

public class GameManager : MonoSingleton<GameManager> 
{
    public GameObject tilePrefab;
    public Transform tileContainer;
    public Color rightColor;
    public Color wrongColor;
    public float gridSideMargin = 10;
    public Text goldAmount;
    public Text timerText;
    public GameObject bgImg;

    public Sprite rightImage;
    public Sprite wrongImage;

    public GameObject forceObject;
    public GameObject timerObject;
    public Button forceButton;
    public GameObject goldPrefab;

    // Ad
    public Text adTimer;
	private bool isUnlocked;
	private float lastUpdate;
	private float lastTimer;
	private float startTimer;
	private bool isHighscore;

	private LevelData currentLevel;
	private Image[,] imgData;
	private float sizeX,sizeY;
    private bool isForcing;
    private float tileSize;
    private List<GameObject> tilePool;
    private EventSystem currentEventSystem;

    private RectTransform iconTransform;
    private float iconTransition = 0;
    private bool iconAnimation = false;
    private Vector3 iconStartPosition;
    public AnimationCurve iconTransitionCurve;
    public Vector3 iconDesiredPosition;

    public override void Init()
	{
		startTimer = Time.time;
		isHighscore = true;
		sizeX = 700;//Screen.width - (gridSideMargin * 2);
		sizeY = 700;
		tilePool = new List<GameObject>();
		goldAmount.text = DataHelper.instance.SaveState.Gold.ToString();

		if (DataHelper.instance.CurrentGameMode == GameMode.Challenge)
		{
			currentLevel = DataHelper.instance.Levels[DataHelper.instance.CurrentLevel].Clone();
			forceObject.SetActive(true);
			CreateGrid(currentLevel);
		}
		else if (DataHelper.instance.CurrentGameMode == GameMode.FreePlay)
		{
			CreateRandomLevel(DataHelper.instance.CurrentLevel);
			timerObject.SetActive(true);
		}

		bgImg.GetComponent<Image>().sprite = DataHelper.instance.GetSpriteAt(DataHelper.instance.SaveState.SelectedImage);
    }

    public void CreateGrid(LevelData level)
    {
        tileSize = sizeX / currentLevel.SizeX;
        imgData = new Image[currentLevel.SizeX,currentLevel.SizeY];
        float offsetY = (sizeY - (currentLevel.SizeY* tileSize)) / 2;

        for (int i = 0; i < currentLevel.SizeX; i++) 
        {
            for (int j = 0; j < currentLevel.SizeY; j++) 
            {
                var go = GetTilePrefab ();
                go.transform.SetParent (tileContainer);
                go.transform.localScale = Vector3.one * tileSize/100;
                go.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (i * tileSize,-j * tileSize - offsetY);
                imgData [i, j] = go.GetComponent<Image> ();
				imgData [i, j].sprite = (currentLevel.Data [i, j]) ? rightImage : wrongImage;
				imgData [i, j].color = (currentLevel.Data [i, j]) ? rightColor : wrongColor;
                int x = i;
                int y = j;
                go.GetComponent<Button> ().onClick.AddListener (() => FlipTile (x,y));
            }
        }
    }

    public void ForceTile()
	{
		if (DataHelper.instance.SaveState.Gold > 24)
		{
			if (!isForcing)
			{
				DataHelper.instance.PlaySFX(1);
				isForcing = true;
				forceButton.image.color = Color.cyan;
			}
			else
			{
				DataHelper.instance.PlaySFX(0);
				isForcing = false;
				forceButton.image.color = Color.white;
			}
		}
		else
			DataHelper.instance.PlaySFX(0);
    }

	private void FlipTile(int x,int y,bool silent = false)
	{
		// Center Tile
		currentLevel.Data [x, y] = !currentLevel.Data [x, y];
		imgData [x, y].color = (currentLevel.Data [x, y]) ? rightColor : wrongColor;
		imgData [x, y].sprite = (currentLevel.Data [x, y]) ? rightImage : wrongImage;
		if(!silent)
			DataHelper.instance.PlaySFX(2);

		if (!isForcing)
		{
			// Top Tile
			y--;
			if (y >= 0) {
				currentLevel.Data [x, y] = !currentLevel.Data [x, y];
				imgData [x, y].color = (currentLevel.Data [x, y]) ? rightColor : wrongColor;
				imgData [x, y].sprite = (currentLevel.Data [x, y]) ? rightImage : wrongImage;
			}

			// Left Tile
			y++;
			x--;
			if (x >= 0) {
				currentLevel.Data [x, y] = !currentLevel.Data [x, y];
				imgData [x, y].color = (currentLevel.Data [x, y]) ? rightColor : wrongColor;
				imgData [x, y].sprite = (currentLevel.Data [x, y]) ? rightImage : wrongImage;
			}

			// Right Tile
			x++;
			x++;
			if (x < currentLevel.SizeX) {
				currentLevel.Data [x, y] = !currentLevel.Data [x, y];
				imgData [x, y].color = (currentLevel.Data [x, y]) ? rightColor : wrongColor;
				imgData [x, y].sprite = (currentLevel.Data [x, y]) ? rightImage : wrongImage;
			}

			// Down Tile
			x--;
			y++;
			if (y < currentLevel.SizeY) {
				currentLevel.Data [x, y] = !currentLevel.Data [x, y];
				imgData [x, y].color = (currentLevel.Data [x, y]) ? rightColor : wrongColor;
				imgData [x, y].sprite = (currentLevel.Data [x, y]) ? rightImage : wrongImage;
			}
		}
		else
		{
            DataHelper.instance.SaveState.Gold -= 25;
            DataHelper.instance.SaveState.Save(SaveMethod.Local);
            goldAmount.text = DataHelper.instance.SaveState.Gold.ToString();
			isForcing = false;
			forceButton.image.color = Color.white;
		}


		CheckVictory ();
	}

	private void CheckVictory()
	{
		for (int i = 0; i < currentLevel.SizeX; i++)
		{
			for (int j = 0; j < currentLevel.SizeY; j++)
			{
				if (!currentLevel.Data[i, j])
					return;
			}
		}

        DataHelper.instance.PlaySFX(1);
        if (DataHelper.instance.CurrentGameMode == GameMode.Challenge)
		{
            if (!DataHelper.instance.SaveState.unlockedLevel.Get(currentLevel.Id))
            {
                DataHelper.instance.SaveState.unlockedLevel.Set(currentLevel.Id, true);
                AdManager.instance.ShowAd();
                DataHelper.instance.ShowRecap("", false, -2);
                DataHelper.instance.SaveState.Gold += 5;
                goldAmount.text = DataHelper.instance.SaveState.Gold.ToString();
            }
            else
            {
                DataHelper.instance.ShowRecap("", false);
                AdManager.instance.ShowAd();
            }

            switch (currentLevel.Id)
            {
                case 0: DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_beginner); break;
                case 5: DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_apprentice); break;
                case 9: DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_10_challenges); break;
                case 19: DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_20_challenges); break;
                case 20: DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_expert); break;
                case 29: DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_30_challenges); break;
                case 38: DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_master); break;
                case 39: DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_40_challenges); break;
                case 49:
                    DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_50_challenges);
                    DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_challenger);
                    DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_grand_master);
                    break;
            }

            DataHelper.instance.SaveState.Save(SaveMethod.Local);
		}
		else if (DataHelper.instance.CurrentGameMode == GameMode.FreePlay)
		{
			int diff = DataHelper.instance.CurrentLevel;
			float t = Time.time - startTimer;

            switch (diff)
			{
				case 0:
                    DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_its_easy);
                    if(t < 5.0f)
                        DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_piece_of_cake);
                    if (DataHelper.instance.SaveState.EasyTime == 0|| t < DataHelper.instance.SaveState.EasyTime )
						DataHelper.instance.SaveState.EasyTime = t;
				break;
				case 1:
                    DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_learning_the_ropes);
                    if(t < 15.0f)
                        DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_getting_the_hang_of_it);
                    if (DataHelper.instance.SaveState.MediumTime == 0 || t < DataHelper.instance.SaveState.MediumTime)
						DataHelper.instance.SaveState.MediumTime = t;
				break;
				case 2:
                    DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_genius);
                    if(t < 30.0f)
                        DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_got_lucky);
                    if (DataHelper.instance.SaveState.HardTime == 0 || t < DataHelper.instance.SaveState.HardTime)
						DataHelper.instance.SaveState.HardTime = t;
				break;
			}

			DataHelper.instance.ShowRecap("", false,diff);
            AdManager.instance.ShowAd();
            DataHelper.instance.SaveState.Gold += (diff == 0)?1:(diff == 1)?5:25;
			DataHelper.instance.SaveState.Save(SaveMethod.Local);
			goldAmount.text = DataHelper.instance.SaveState.Gold.ToString();

            GameObject go = Instantiate(goldPrefab) as GameObject;
            go.transform.SetParent(GameObject.FindGameObjectWithTag("UIRoot").transform);
            go.GetComponent<RectTransform>().anchoredPosition = Vector3.up * 450;

            iconTransform = go.GetComponent<RectTransform>();
            iconStartPosition = iconTransform.anchoredPosition;
            iconAnimation = true;
        }
	}

	private GameObject GetTilePrefab()
    {
        GameObject go = tilePool.Find(x => !x.activeSelf);

        if (go == null)
        {
            go = Instantiate(tilePrefab) as GameObject;
            tilePool.Add(go);
        }
        else
            go.SetActive(true);

        return go;
	}

	public void ToPause()
	{
		DataHelper.instance.ShowPause ("Pause", "", true);
	}

	public void CleanLevel()
    {
        foreach (GameObject go in tilePool)
        {
            go.SetActive(false);
            go.GetComponent<Button>().onClick.RemoveAllListeners();
        }
	}

	private void Update()
	{
        if (Input.GetKeyDown(KeyCode.Escape) && startTimer > 2.0f)
        {
            DataHelper.instance.PlaySFX(3);
            DataHelper.instance.LoadMenuAt(CanvasPosition.Main);
        }

        if (!isUnlocked && Time.time - lastUpdate > 1)
		{
			lastUpdate = Time.time;
			long timestamp = (long)DataHelper.instance.SaveState.AdTimestamp;
			long diff = (DateTime.Now.Ticks - timestamp);
			long m = diff / TimeSpan.TicksPerMillisecond;
			float ms = /*3m*/ ((180 * 1000) - m) / 1000;

			string r = "";
			// Minutes
			r += ((int)ms / 60).ToString("00") + "m ";
			// Secondes
			r += (ms % 60).ToString("00") + 's';
			adTimer.text = r;

			if (ms < 0 || timestamp == 0)
			{
				adTimer.text = "Watch Ad!";
				adTimer.transform.parent.GetComponent<Button>().interactable = true;
				isUnlocked = true;
				return;
			}
		}

		if (DataHelper.instance.CurrentGameMode == GameMode.FreePlay && Time.time - lastTimer > 1)
		{
			lastTimer = Time.time;
			float ms = Time.time - startTimer;
			int diff = DataHelper.instance.CurrentLevel;
			float hs = (diff == 0) ? DataHelper.instance.SaveState.EasyTime : (diff == 1) ? DataHelper.instance.SaveState.MediumTime : DataHelper.instance.SaveState.HardTime;

			if(isHighscore && ms >= hs && hs != 0)
			{
				isHighscore = false;
				timerText.color = Color.yellow;
			}

			string r = "";
			// Minutes
			r += ((int)ms / 60).ToString("00") + "m ";
			// Secondes
			r += (ms % 60).ToString("00") + 's';
			timerText.text = r;
		}

        if (iconAnimation)
        {
            iconTransition += Time.deltaTime/3;
            float t = iconTransitionCurve.Evaluate(iconTransition);
        //    iconTransform.anchoredPosition = Vector3.Slerp(iconStartPosition, iconDesiredPosition, t);
        }
	}

    public void WatchAd()
    {
        DataHelper.instance.LaunchAd(HandleAdResult);
        currentEventSystem = GameObject.Find ("EventSystem").GetComponent<EventSystem> ();
        currentEventSystem.enabled = false;
    }

    private void HandleAdResult(ShowResult r)
    {
        switch (r) 
        {
        case ShowResult.Finished:
            DataHelper.instance.SaveState.Gold += 20;
            goldAmount.text = DataHelper.instance.SaveState.Gold.ToString();
            isUnlocked = false;
			DataHelper.instance.SaveState.AdTimestamp = (ulong)DateTime.Now.Ticks;
			DataHelper.instance.SaveState.Save(SaveMethod.Local);
            DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_banking);
			adTimer.transform.parent.GetComponent<Button>().interactable = false;
            currentEventSystem.enabled = true;
                break;
        case ShowResult.Failed:
            currentEventSystem.enabled = true;
            DataHelper.instance.ShowError("Ad Error","We were unable to launch the ad, are you sure you are connected to the internet?",true,2.0f);
                break;
        }
    }

    private void CreateRandomLevel(int diff)
    {
		currentLevel = new LevelData(){ LevelName = "Free Play" };
		currentLevel.SizeX = (diff == 0) ? 4 : (diff == 1) ? 5 : 6;
		currentLevel.SizeY = currentLevel.SizeX;
		currentLevel.Id = diff;
		currentLevel.Data = new bool[currentLevel.SizeX, currentLevel.SizeY];
		for (int i = 0; i < currentLevel.SizeX; i++)
			for (int j = 0; j < currentLevel.SizeY; j++)
				currentLevel.Data[i, j] = true;

		CreateGrid(currentLevel);

		Vector2 lastMove = new Vector2(-1, -1);
		for (int i = 0; i < (currentLevel.SizeX * 5); i++)
		{
			int x = 0, y = 0;
			x = UnityEngine.Random.Range(0, currentLevel.SizeX);
			y = UnityEngine.Random.Range(0, currentLevel.SizeY);
			while (lastMove == new Vector2(x, y))
			{
				x = UnityEngine.Random.Range(0, currentLevel.SizeX);
				y = UnityEngine.Random.Range(0, currentLevel.SizeY);
			}
			FlipTile(x, y,true);
		}
    }

	public bool NextLevel()
	{
        if (iconTransform)
        {
            Destroy(iconTransform.gameObject);
            iconTransition = 0;
            iconAnimation = false;
        }

		if (DataHelper.instance.CurrentGameMode == GameMode.Challenge)
		{
			if (currentLevel.Id == DataHelper.instance.Levels.Count - 1)
			{
				DataHelper.instance.LoadMenuAt(CanvasPosition.Progress);
				return false;
			}

			CleanLevel();
			DataHelper.instance.CurrentLevel++;
            currentLevel = DataHelper.instance.Levels[DataHelper.instance.CurrentLevel].Clone();
            CreateGrid(currentLevel);
		}
		else if (DataHelper.instance.CurrentGameMode == GameMode.FreePlay)
		{
			CleanLevel();
			CreateRandomLevel(DataHelper.instance.CurrentLevel);
			startTimer = Time.time;
			timerText.color = Color.white;
			isHighscore = true;
			lastUpdate = Time.time - 1;
		}

        return true;
	}
}
