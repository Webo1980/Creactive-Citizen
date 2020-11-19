using Mapbox.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeScreenshot : MonoBehaviour {

    //public AudioSource audioSource;
    //public AudioClip audioClip;

    private readonly AddCustomMarker Marker = new AddCustomMarker();
    public AudioSource audioSource;  //audio source

    public void TakeAShot()
	{
		StartCoroutine ("CaptureIt");
        Marker.PlayClip(audioSource, "Screenshot");
        StartCoroutine("ShowMessage");
    }

	IEnumerator CaptureIt()
	{
		string timeStamp = System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
		string fileName = "Screenshot" + timeStamp + ".png";
		string pathToSave = fileName;
		ScreenCapture.CaptureScreenshot(pathToSave);
        yield return new WaitForEndOfFrame();
		//Instantiate (blink, new Vector2(0f, 0f), Quaternion.identity);
	}

    public void playClip()
    {
        //audioSource.clip = audioClip;
        //audioSource.Play();
    }

    IEnumerator ShowMessage()
    {
        Marker.ChangeVisibility("ScreenShotMessage", 1.0f, true, true);
        yield return new WaitForSeconds(3);
        Marker.ChangeVisibility("ScreenShotMessage", 0.0f, false, false);
    }

}
