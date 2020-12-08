using Mapbox.Examples;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScreenShare : MonoBehaviour
{
    public AudioSource audioSource;  //audio source

    private readonly AddCustomMarker Marker = new AddCustomMarker();
    
    public void ClickShare()
    {
        AddCustomMarker.PlayClip(audioSource, "SceenShare");
        StartCoroutine("TakeScreenshotAndShare");
    }

    private IEnumerator TakeScreenshotAndShare()
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenShare = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenShare.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenShare.Apply();

        string filePath = Path.Combine(Application.temporaryCachePath, "shared img.png");
        File.WriteAllBytes(filePath, screenShare.EncodeToPNG());

        // To avoid memory leaks
        Destroy(screenShare);

        new NativeShare().AddFile(filePath)
            .SetSubject("Subject goes here").SetText("Hello world!")
            .SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
            .Share();
    }
}
