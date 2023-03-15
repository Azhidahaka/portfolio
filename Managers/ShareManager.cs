using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FacebookGames;
using System.IO;

public class ShareManager : MonoBehaviour {
    public static ShareManager instance;

    private void Awake() {
        instance = this;
    }

    public void Share() {
        StartCoroutine(TakeScreenshotAndShare());
    }

    private IEnumerator TakeScreenshotAndShare() {
        string subject = "MatchBlocks";
        string content = "ScreenShot";

        yield return new WaitForEndOfFrame();

        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenShot.Apply();

        string filePath = Path.Combine(Application.temporaryCachePath, "shared_img.png");
        File.WriteAllBytes(filePath, screenShot.EncodeToPNG());

        // To avoid memory leaks
        Destroy(screenShot);

        new NativeShare().AddFile(filePath)
            .SetSubject(subject).SetText(content)
            .SetCallback((result, shareTarget) => {
                switch (result) {
                    case NativeShare.ShareResult.Unknown: {
                        Debug.Log("Share result: " + result + ", selected app: " + shareTarget);
                        break;
                    }
                    case NativeShare.ShareResult.NotShared: {
                        Debug.Log("Share result: " + result + ", selected app: " + shareTarget);
                        break;
                    }
                    case NativeShare.ShareResult.Shared: {
                        Debug.Log("Share result: " + result + ", selected app: " + shareTarget);
                        break;
                    }
                }
            })
            .Share();

        // Share on WhatsApp only, if installed (Android only)
        //if( NativeShare.TargetExists( "com.whatsapp" ) )
        //	new NativeShare().AddFile( filePath ).AddTarget( "com.whatsapp" ).Share();
    }
}
