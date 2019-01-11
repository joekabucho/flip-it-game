using UnityEngine;
using System.Collections;

public class SaveState
{
	public int Gold{set;get;}
	public int Diamond{ set; get;}
	public int SelectedImage{set;get;}

	public float LastSessionTime{set;get;}
	public float TotalPlayTime{ get { return LastSessionTime + (Time.time - lastSave);}}
	public float EasyTime{set;get;}
	public float MediumTime{set;get;}
	public float HardTime{set;get;}
    public int RollTime { set; get; }

    public int EasyCount { set; get; }
    public int MediumCount { set; get; }
    public int HardCount { set; get; }

    public ulong GiftTimestamp{set;get;}
	public ulong AdTimestamp{set;get;}

	public bool SoundEnabled{set;get;}
	public bool TutorialCompleted{set;get;}
    public bool AdDisabled { set; get; }

	private float lastSave;

	public BitArray unlockedLevel;
	public BitArray unlockedItem;
	public BitArray unlockedAchievement;

	/* Format "	LASTSESSIONTIME%
	 *			GiftTimestamp
	 *			Gold
	 *			Diamond
	 *			UnlockedLevelBitArray%
	 *			UnlockedItemBitArray
	 *			UnlockedAchievementBitArray
	 *			SelectedImage
	 *			SoundEnabled
	 *			AdTimestamp
	 *			EasyTime
	 *			MediumTime
	 *			HardTime
	 *			TutorialCompleted
     *			RollTime
     *			EasyCount
     *			MediumCount
     *			HardCount
	 */
	public SaveState():this("0%0%0%0%0%0%0%0%0%0%0%0%0%0%0%0%0%0%0"){}
	public SaveState(string saveString)
    {
        var vData = saveString.Split('%');
        float f;
        ulong u;
        int i;

        if (float.TryParse(vData[0], out f))
            LastSessionTime = f;

        if(ulong.TryParse(vData[1], out u))    	
			GiftTimestamp = u;

        if (int.TryParse(vData[2], out i))
            Gold = i;

        if (int.TryParse(vData[3], out i))
            Diamond = i;

        unlockedLevel = new BitArray(DataHelper.instance.Levels.Count);
        unlockedItem = new BitArray(DataHelper.instance.Items.Count);
        unlockedAchievement = new BitArray(DataHelper.instance.Achievements.Count);

        int x = 0;
        foreach (char c in vData[4])
            unlockedLevel.Set(x++, (c == '1') ? true : false);
		x = 0;
		foreach(char c in vData[5])
			unlockedItem.Set (x++, (c == '1') ? true : false);
		x = 0;
		foreach(char c in vData[6])
			unlockedAchievement.Set (x++, (c == '1') ? true : false);

		int selectedImg;
		if(int.TryParse(vData[7],out selectedImg))
			SelectedImage = selectedImg;
		else
			SelectedImage = 0;

		SoundEnabled = (vData[8] == "0")?true:false;

		if(ulong.TryParse(vData[9], out u))    	
			AdTimestamp = u;

		if (float.TryParse(vData[10], out f))
            EasyTime = f;

		if (float.TryParse(vData[11], out f))
            MediumTime = f;

		if (float.TryParse(vData[12], out f))
            HardTime = f;

		TutorialCompleted = (vData[13] == "0")?false:true;

        if (int.TryParse(vData[14], out x))
            RollTime = x;

        if (int.TryParse(vData[15], out x))
            EasyCount = x;

        if (int.TryParse(vData[16], out x))
            MediumCount = x;

        if (int.TryParse(vData[17], out x))
            HardCount = x;

        AdDisabled = (vData[18] == "0") ? true : false;
    }

	public bool Save(SaveMethod method)
	{
		var s = BuildSaveString ();
		if (method == SaveMethod.Local)
		{
			PlayerPrefs.SetString ("SaveString",s);
			return true;
		}

		return false;
	}
	private string BuildSaveString()
    {
        var r = "";

		r += TotalPlayTime.ToString("0") + '%';
		LastSessionTime = TotalPlayTime;
		lastSave = Time.time;
		r += GiftTimestamp.ToString() + '%';
        r += Gold.ToString() + '%';
        r += Diamond.ToString() + '%';

        for (int i = 0; i < unlockedLevel.Count; i++)
            r += (unlockedLevel.Get(i)) ? "1" : "0";
		r += '%';
		for (int i = 0; i < unlockedItem.Count; i++)
			r += (unlockedItem.Get (i)) ? "1" : "0";
		r += '%';
		for (int i = 0; i < unlockedAchievement.Count; i++)
			r += (unlockedAchievement.Get (i)) ? "1" : "0";

		r += '%';
		r += SelectedImage.ToString();

		r += '%';
		r += (SoundEnabled)?'0':'1';

		r += '%';
		r += GiftTimestamp.ToString();

		r += '%';

		r += EasyTime.ToString();
		r += '%';
		r += MediumTime.ToString();
		r += '%';
		r += HardTime.ToString();

		r += '%';
		r += (TutorialCompleted)?'1':'0';

        r += '%';
        r += RollTime.ToString();

        r += '%';
        r += EasyCount.ToString();

        r += '%';
        r += MediumCount.ToString();

        r += '%';
        r += HardCount.ToString();

        r += '%';
        r += (AdDisabled) ? '1' : '0';

        return r;
	}

    public int UnlockedItemCount()
    {
        int count = 0;
        foreach (bool b in unlockedItem)
            if (b)
                count++;

        return count;
    }

    public void UnlockAchievement(string code)
    {
        foreach (AchievementData b in DataHelper.instance.Achievements)
        {
            if (b.Key == code)
            {
                int index = DataHelper.instance.Achievements.FindIndex(x => x == b);

                    unlockedAchievement.Set(index, true);
                    Debug.Log("Unlocked " + b.AchievementName);
                    Save(SaveMethod.Local);
                    Social.ReportProgress(code, 100.0f, (bool success) =>
                    {

                    });

                return;
            }
        }

        Debug.Log("Didn't find achievement " + code);
    }

	public override string ToString ()
	{
		string r = "";

		r += "Gold: " + Gold;
		r += " ! ";
		r += "Diamond: " + Diamond;
		r += " ! ";
		r += "LastSessionTime: " + LastSessionTime;
		r += " ! ";

		r += "UnlockedLevel: ";
		for (int i = 0; i < unlockedLevel.Count; i++)
			r += (unlockedLevel.Get (i)) ? "1" : "0";
		r += " ! ";

		r += "UnlockedItem: ";
		for (int i = 0; i < unlockedItem.Count; i++)
			r += (unlockedItem.Get (i)) ? "1" : "0";
		r += " ! ";

		r += "UnlockedAchievement: ";
		for (int i = 0; i < unlockedAchievement.Count; i++)
			r += (unlockedAchievement.Get (i)) ? "1" : "0";
		r += " ! ";

		return r;
	} 
}