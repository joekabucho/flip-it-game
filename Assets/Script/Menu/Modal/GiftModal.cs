using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class GiftModal : BaseModal
{
	public Button btn;
	public Transform roll;
	public AnimationCurve speedCurve;

	private Text timerText;
	private bool isUnlocked;
	private bool isRolling;

	private Transform[] rolls;
	private float t;
	private string giftName;

	private void Start()
	{
		timerText = GameObject.Find("GiftTimer").GetComponent<Text>();
		ToggleGift(false);
		rolls = new Transform[roll.childCount-1];
		for(int i=0;i<roll.childCount-1;i++)
			rolls[i] = roll.GetChild(i+1);
	}

	public void ShowModal(bool exitable)
	{
		Show ();

		this.isExitable = exitable;
	}

	protected override void Update()
	{
		base.Update();

		if (!isUnlocked)
		{
			long timestamp = (long)DataHelper.instance.SaveState.GiftTimestamp;
			long diff = (DateTime.Now.Ticks - timestamp);
			long m = diff / TimeSpan.TicksPerMillisecond;
			float ms = /*12h*/ ((3600000 * 12) - m) / 1000;

			if (ms < 0 || timestamp == 0)
			{
				ToggleGift(true);
				isUnlocked = true;
				return;
			}

			string r = "";
			// Hours
			r += ((int)ms / 3600).ToString() + "h ";
			ms -= ((int)ms / 3600) * 3600;
			// Minutes
			r += ((int)ms / 60).ToString("00") + "m ";
			// Secondes
			r += (ms % 60).ToString("00") + 's';
			timerText.text = r;
		}

		if (isRolling)
		{
			Vector3 end = (-Vector3.right * 250) * (rolls.Length - 1);
			roll.transform.localPosition = Vector3.Lerp(Vector3.right * 250, end, speedCurve.Evaluate(t));
			t += Time.deltaTime / 3;
			if (t > 1)
			{
				isRolling = false;
				GiveGift();
			}
		}
	}

	private void GiveGift()
	{
        DataHelper.instance.SaveState.RollTime++;
        if (DataHelper.instance.SaveState.RollTime == 3)
            DataHelper.instance.SaveState.UnlockAchievement(GPResources.achievement_high_roller);

		switch (giftName)
		{
			case "Gold1":
				Debug.Log("Gold : 1");
				DataHelper.instance.SaveState.Gold += 1;
                DataHelper.instance.PlaySFX(1);
                break;
			case "Gold10":
				Debug.Log("Gold : 10");
				DataHelper.instance.SaveState.Gold += 10;
                DataHelper.instance.PlaySFX(1);
                break;
			case "Gold15":
				Debug.Log("Gold : 15");
				DataHelper.instance.SaveState.Gold += 15;
                DataHelper.instance.PlaySFX(1);
                break;
			case "Gold30":
				Debug.Log("Gold : 30");
				DataHelper.instance.SaveState.Gold += 30;
                DataHelper.instance.PlaySFX(1);
                break;
			case "Nothing":
				Debug.Log("Gift : Nothing");
                DataHelper.instance.PlaySFX(0);
                this.isExitable = true;
				return;
		}

        MenuManager.instance.RefreshGold();
		DataHelper.instance.SaveState.Save(SaveMethod.Local);
		this.isExitable = true;
	}

	public void GiftClick()
	{
		this.isExitable = false;
		ToggleGift(false);
		isUnlocked = false;
		DataHelper.instance.SaveState.GiftTimestamp = (ulong)DateTime.Now.Ticks;
		DataHelper.instance.SaveState.Save(SaveMethod.Local);
        DataHelper.instance.PlaySFX(3);
        isRolling = true;
		roll.transform.localPosition = Vector3.right * 250;
		t = 0;

		int offset = 0;
		List<int> indexes = new List<int>();
		for (int i = 0; i < rolls.Length; i++)
			indexes.Add(i);

		for (int i = 0; i < rolls.Length; i++)
		{
			int index = indexes[UnityEngine.Random.Range(0,indexes.Count)];
			indexes.Remove(index);
			rolls[index].transform.localPosition = Vector3.right * offset;
			offset+=250;
			giftName = rolls[index].name;
		}
	}

	private void ToggleGift(bool inter)
	{
		btn.interactable = inter;
		if(inter)
			timerText.text = "Ready!";
	}
}
