using UnityEngine;

public class TutorialModal : BaseModal
{
#if UNITY_ANDROID

 #else
        public MovieTexture tutorialVideo;
 #endif


    public void ShowModal(bool exitable)
    {
        Show();

#if UNITY_ANDROID
        Handheld.PlayFullScreenMovie("Tutorial.mp4",Color.black);
#else
        tutorialVideo.loop = true;
        if (tutorialVideo.isPlaying)
            tutorialVideo.Stop();
        tutorialVideo.Play();
#endif


        this.isExitable = exitable;
    }
}
