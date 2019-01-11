using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chest : MonoBehaviour
{
    private const float ROLL_OFFSET = 300.0f;
    public float msToWait = 5000.0f;
    public Transform rollContainer;
    public AnimationCurve rollCurve;

    private Text chestTimer;
    private Button chestButton;
    private ulong lastChestOpen;
    private Transform[] rolls;
    private bool isRolling;
    private float transition;
    private string giftName;

    private void Start()
    {
        chestButton = GetComponent<Button>();

        if ((PlayerPrefs.HasKey("LastChestOpen")))
            lastChestOpen = ulong.Parse(PlayerPrefs.GetString("LastChestOpen"));
        else
            lastChestOpen = 1;
        chestTimer = GetComponentInChildren<Text>();

        if (!IsChestReady())
            chestButton.interactable = false;

        rolls = new Transform[rollContainer.childCount];
        for (int i = 0; i < rollContainer.childCount; i++)
            rolls[i] = rollContainer.GetChild(i);
    }

    private void Update()
    {
        if (!chestButton.IsInteractable())
        {
            if (IsChestReady())
            {
                chestButton.interactable = true;
                return;
            }

            // Set the timer
            ulong diff = ((ulong)DateTime.Now.Ticks - lastChestOpen);
            ulong m = diff / TimeSpan.TicksPerMillisecond;
            float secondsLeft = (float)(msToWait - m) / 1000.0f;

            string r = "";
            // Hours
            r += ((int)secondsLeft / 3600).ToString() + "h ";
            secondsLeft -= ((int)secondsLeft / 3600) * 3600;
            // Minutes
            r += ((int)secondsLeft / 60).ToString("00") + "m ";
            // Seconds
            r += (secondsLeft % 60).ToString("00") + "s"; ;
            chestTimer.text = r;
        }

        if (isRolling)
        {
            Vector3 end = (-Vector3.right * ROLL_OFFSET) * (rolls.Length - 1);
            rollContainer.transform.localPosition = Vector3.Lerp(Vector3.right * ROLL_OFFSET, end, rollCurve.Evaluate(transition));
            transition += Time.deltaTime / 3.0f;
            if (transition > 1)
            {
                isRolling = false;
                switch (giftName)
                {
                    case "Gold10":
                        Debug.Log("You've been rewarded with 10 golds!");
                        break;
                    case "Gold20":
                        Debug.Log("You've been rewarded with 20 golds!");
                        break;
                    case "Gold30":
                        Debug.Log("You've been rewarded with 30 golds!");
                        break;
                    default:
                    case "Nothing":
                        Debug.Log("You didn't win anything");
                        break;
                }
            }
        }
    }

    public void ChestClick()
    {
        lastChestOpen = (ulong)DateTime.Now.Ticks;
        PlayerPrefs.SetString("LastChestOpen", lastChestOpen.ToString());
        chestButton.interactable = false;

        // Where you would give gold to your player! 
        // This is where you would roll a dice
        transition = 0;
        isRolling = true;
        float offset = 0.0f;
        List<int> indexes = new List<int>();
        for (int i = 0; i < rolls.Length; i++)
            indexes.Add(i);

        for(int i = 0; i < rolls.Length; i++)
        {
            int index = indexes[UnityEngine.Random.Range(0, indexes.Count)];
            indexes.Remove(index);
            rolls[index].transform.localPosition = Vector3.right * offset;
            offset += ROLL_OFFSET;
            giftName = rolls[index].name;
        }
    }

    private bool IsChestReady()
    {
        ulong diff = ((ulong)DateTime.Now.Ticks - lastChestOpen);
        ulong m = diff / TimeSpan.TicksPerMillisecond;
        float secondsLeft = (float)(msToWait - m) / 1000.0f;

        if (secondsLeft < 0)
        {
            chestTimer.text = "Ready!";
            return true;
        }

        return false;
    }
}