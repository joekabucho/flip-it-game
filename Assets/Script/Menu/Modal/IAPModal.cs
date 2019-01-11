using UnityEngine;
using System.Collections;

public class IAPModal : BaseModal
{
    public void ShowModal(bool exitable)
    {
        Show ();

        this.isExitable = exitable;
    }

    public void Buy50()
    {
        IAPManager.Instance.Buy50Gold();
    }

    public void Buy100()
    {
        IAPManager.Instance.Buy100Gold();
    }

    public void Buy1000()
    {
        IAPManager.Instance.Buy1000Gold();
    }

    public void BuyNoAds()
    {
        IAPManager.Instance.BuyNoAds();
    }
}