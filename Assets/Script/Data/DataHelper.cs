using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum ModalType
{
	Error,
	Pause,
}

public enum SaveMethod
{
	Local,
	GoogleCloud,
}

public enum Local
{
	EN = 1,
	FR = 2,
}

public class DataHelper : MonoSingleton<DataHelper>
{
	public Local TestLocal = Local.EN;

	public Local CurrentLocal{ set; get;}
	public SaveMethod SaveMethod{ set; get;}
	public bool IsConnectedToGoogleServices{set;get;}
	public bool IsConnectedToUnityAds{set;get;}

	public string rateUrl;
	public string likeUrl;
	public string youtubeUrl;

	//
	public Sprite[] uiAtlas;
    private bool firstTime =true;

    // Modal
    public GameObject errorModal;
	public GameObject pauseModal;
	public GameObject giftModal;
    public GameObject recapModal;
    public GameObject iapModal;
    public GameObject difficutlyModal;
    public GameObject contactModal;
    public GameObject tutorialModal;

    // Actual game data
    public TextAsset levelAsset;
	public TextAsset itemAsset;
	public TextAsset achievementAsset;
	public TextAsset localizationAsset;
	public List<LevelData> Levels{ set; get; }
	public List<ItemData> Items{ set; get;}
	public List<AchievementData> Achievements { set; get;}
	public List<string[]> Locals{set;get;}
	public SaveState SaveState{ set; get; }

	public float TimeSinceStart{ set; get; }
	public float TotalGameTime{ set; get; }

	public GameMode CurrentGameMode{set;get;}
	public int CurrentLevel{set;get;}
    public bool InSceneTransition { get; internal set; }

    // private fields
    private float introDuration = 2.0f;
	private bool isDoneInitializating;
	private Sprite[] allItems;
	private bool isLoadMenuAt;

	private AudioSource aSource;
	public AudioClip[] aClips;

	private CanvasPosition menuPos;

	public override void Init ()
	{
		PlayGamesPlatform.Activate ();
	//	PlayGamesPlatform.DebugLogEnabled = false;

		SaveMethod = SaveMethod.Local;
		CurrentLocal = TestLocal;
		TimeSinceStart = Time.time;
		aSource = GetComponent<AudioSource>();

		// Build all our data
		Levels = ReadLevelData (levelAsset.text);
		Items = ReadItemData (itemAsset.text);
		Achievements = ReadAchievementData ();
		Locals = ReadLocalString (CurrentLocal,localizationAsset.text);

		// Load Save State
		SaveState = ReadSaveData(SaveMethod);

		// Connect to Services
	//	ConnectToGoogleServices();
	//	ConnectToUnityAds ();

		// Done loading everything, ready to boot the game after the intro screen
		isDoneInitializating = true;

		DontDestroyOnLoad (gameObject);
	}	

	private void Update()
	{
        if (Input.GetKeyDown(KeyCode.T))
            Debug.Log(SaveState.ToString());

		if (isDoneInitializating && Time.time - TimeSinceStart > introDuration)
		{
			isDoneInitializating = false;
			SceneManager.LoadScene ("Menu");
		}

		if (isLoadMenuAt && SceneManager.GetSceneByName("Menu").isLoaded) 
		{
			isLoadMenuAt = false;
			MenuManager.instance.Init ();
			MenuManager.instance.StartAtPosition(menuPos);
		}
	}

	private List<LevelData> ReadLevelData(string data)
	{
		var r = new List<LevelData> ();

		var vData = data.Split ('\n');
		foreach (string s in vData)
			r.Add (new LevelData (s));

		return r;
	}
	private List<ItemData> ReadItemData(string data)
	{
		var r = new List<ItemData> ();

		var vData = data.Split ('\n');
		foreach (string s in vData)
			r.Add (new ItemData (s));

		return r;
	}
	private List<AchievementData> ReadAchievementData()
	{
		var r = new List<AchievementData> ();

        r.Add(new AchievementData("achievement_apprentice", GPResources.achievement_apprentice));
        r.Add(new AchievementData("achievement_curious", GPResources.achievement_curious));
        r.Add(new AchievementData("achievement_thanks_for_playing", GPResources.achievement_thanks_for_playing));
        r.Add(new AchievementData("achievement_40_challenges", GPResources.achievement_40_challenges));
        r.Add(new AchievementData("achievement_genius", GPResources.achievement_genius));
        r.Add(new AchievementData("achievement_piece_of_cake", GPResources.achievement_piece_of_cake));
        r.Add(new AchievementData("achievement_30_challenges", GPResources.achievement_30_challenges));
        r.Add(new AchievementData("achievement_getting_the_hang_of_it", GPResources.achievement_getting_the_hang_of_it));
        r.Add(new AchievementData("achievement_30_skins", GPResources.achievement_30_skins));
        r.Add(new AchievementData("achievement_banking", GPResources.achievement_banking));
        r.Add(new AchievementData("achievement_learning_the_ropes", GPResources.achievement_learning_the_ropes));
        r.Add(new AchievementData("achievement_challenger", GPResources.achievement_challenger));
        r.Add(new AchievementData("achievement_20_challenges", GPResources.achievement_20_challenges));
        r.Add(new AchievementData("achievement_got_lucky", GPResources.achievement_got_lucky));
        r.Add(new AchievementData("achievement_beginner", GPResources.achievement_beginner));
        r.Add(new AchievementData("achievement_expert", GPResources.achievement_expert));
        r.Add(new AchievementData("achievement_40_skins", GPResources.achievement_40_skins));
        r.Add(new AchievementData("achievement_grand_master", GPResources.achievement_grand_master));
        r.Add(new AchievementData("achievement_its_easy", GPResources.achievement_its_easy));
        r.Add(new AchievementData("achievement_10_skins", GPResources.achievement_10_skins));
        r.Add(new AchievementData("achievement_20_skins", GPResources.achievement_20_skins));
        r.Add(new AchievementData("achievement_master", GPResources.achievement_master));
        r.Add(new AchievementData("achievement_10_challenges", GPResources.achievement_10_challenges));
        r.Add(new AchievementData("achievement_reskinning", GPResources.achievement_reskinning));
        r.Add(new AchievementData("achievement_50_skins", GPResources.achievement_50_skins));
        r.Add(new AchievementData("achievement_high_roller", GPResources.achievement_high_roller));
        r.Add(new AchievementData("achievement_50_challenges", GPResources.achievement_50_challenges));

        return r;
	}
	private List<string[]> ReadLocalString(Local loc,string data)
	{
		var r = new List<string[]> ();

		string[] vData = data.Split ('\n');
		foreach (string s in vData) 
		{
			string[] i = s.Split (',');
			r.Add (i);
		}

	//	localizationAsset

		return r;
	}
	private SaveState ReadSaveData(SaveMethod method)
	{
		SaveState r;
		if (method == SaveMethod.Local) 
		{
			var registry = PlayerPrefs.GetString ("SaveString");
			if (registry != "")
				r = new SaveState (registry);
			else
				r = new SaveState ();
		}
		else
		{
			r = new SaveState ("999%999%0%11%11%11");
		}
			
		return r;
	}

	public void LoadMenuAt(CanvasPosition pos)
	{
		SceneManager.LoadScene ("Menu");
		isLoadMenuAt = true;
		menuPos = pos;
	}
	public Sprite GetSpriteAt(int i)
	{
		if(allItems == null)
			allItems = Resources.LoadAll<Sprite>("item/Items");
		return allItems[i];
	}

    // Modals
    public void ShowError(string title, string description, bool exitable)
    {
        ErrorModal m = (Instantiate(errorModal)).GetComponent<ErrorModal>();
        m.Init();
        m.ShowModal(title, description, exitable);
    }
    public void ShowError(string title, string description, bool exitable,float duration)
    {
        ErrorModal m = (Instantiate(errorModal)).GetComponent<ErrorModal>();
        m.Init();
        m.ShowModal(title, description, exitable,duration);
    }
    public void ShowPause(string title,string description,bool exitable)
	{
		PauseModal m = (Instantiate (pauseModal)).GetComponent<PauseModal> ();
		m.Init ();
		m.ShowModal (title, description, exitable);
	}
	public void ShowGift(bool exitable)
	{
		GiftModal m = (Instantiate (giftModal)).GetComponent<GiftModal> ();
		m.Init ();
		m.ShowModal(exitable);
	}
    public void ShowRecap(string title, bool exitable,int param = -1)
    {
        RecapModal m = (Instantiate (recapModal)).GetComponent<RecapModal> ();
        m.Init ();
		m.ShowModal(exitable,param);
    }
	public void ShowIAP(bool exitable)
    {
        IAPModal m = (Instantiate (iapModal)).GetComponent<IAPModal> ();
        m.Init();
        m.ShowModal(exitable);
    }
	public void ShowDifficulty(bool exitable)
    {
		DifficultyModal m = (Instantiate (difficutlyModal)).GetComponent<DifficultyModal> ();
        m.Init();
        m.ShowModal(exitable);
    }
    public void ShowContact(bool exitable)
	{
		ContactModal m = (Instantiate (contactModal)).GetComponent<ContactModal> ();
        m.Init();
        m.ShowModal(exitable);
	}
    public void ShowTutorial(bool exitable)
    {
        TutorialModal m = (Instantiate(tutorialModal)).GetComponent<TutorialModal>();
        m.Init();
        m.ShowModal(exitable);
    }

	public bool ConnectToGoogleServices(bool force=false)
	{
		if ((!IsConnectedToGoogleServices && firstTime )|| !IsConnectedToGoogleServices && force)
		{
            firstTime = false;
			Social.localUser.Authenticate ((bool success) => {
                IsConnectedToGoogleServices = success;

                int i = 0;
                foreach (AchievementData a in Achievements)
                {
                    if (SaveState.unlockedAchievement.Get(i))
                        SaveState.UnlockAchievement(a.Key);
                    i++;
                }
			});
		}

		return IsConnectedToGoogleServices;
	}
	public bool ConnectToUnityAds()
	{
		return true;
	}
		
    public bool LaunchAd(System.Action<UnityEngine.Advertisements.ShowResult> callback)
	{
        if (Advertisement.IsReady ()) 
		{
            Advertisement.Show ("rewardedVideo",new ShowOptions(){resultCallback = callback});
			return true;
		}
		ShowError ("Ad", "Unable to play ad, are you connected to the internet?", true);
		return false;
	}

	public void PlaySFX(int clipIndex)
	{
		if(SaveState.SoundEnabled)
			aSource.PlayOneShot(aClips[clipIndex]);
	}

	private Local FindPhoneLocal()
	{
		return Local.EN;
	}
	public string GetStringText(string key)
	{
		return Locals.Find (s => s [0] == key)[(int)CurrentLocal];
	}
}