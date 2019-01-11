using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;

public class AdManager : MonoSingleton<AdManager>
{
    private const int AD_TICK = 7;
    private int currentTick = 0;

    public void ShowAd()
    {
        if (DataHelper.instance.SaveState.AdDisabled)
            return;

        currentTick++;
        if (currentTick == AD_TICK)
        {
            if (Advertisement.IsReady())
            {
                Advertisement.Show("rewardedVideo", new ShowOptions() { resultCallback = HandleAdResult });
            }
            currentTick = 0;
        }
    }

    private void HandleAdResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
            case ShowResult.Failed:
            default:
                break;
        }
    }
}