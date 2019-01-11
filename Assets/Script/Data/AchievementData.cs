using UnityEngine;
using System.Collections;

public class AchievementData
{
	public string AchievementName{ set; get; }
	public string Key{ set; get; }

	public AchievementData(string name,string key)
	{
        AchievementName = name;
        Key = key;
    }
}