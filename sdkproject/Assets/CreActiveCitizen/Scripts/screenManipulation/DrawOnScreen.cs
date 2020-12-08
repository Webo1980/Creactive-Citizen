using Mapbox.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawOnScreen : MonoBehaviour
{
    public static GameObject trailPrefab;
    static GameObject thisTrail;
    static Vector3 startPos;
    static Plane objPlane;
    public AudioSource audioSource;  //audio source
    public static bool isDrawing = false;

    private void Start()
    {
       
    }

    public void SetIsDrawing()
    {
        isDrawing = true;  // change the drawing flag to true
        objPlane = new Plane(Camera.main.transform.forward * -1, this.transform.position);
        AddCustomMarker.PlayClip(audioSource, "Paint");
        // show the painting colors
        AddCustomMarker.ChangeVisibility("SetRed", 1f, true, true);
        AddCustomMarker.ChangeVisibility("SetGreen", 1f, true, true);
        AddCustomMarker.ChangeVisibility("SetBlue", 1f, true, true);
    }

    public static void StartDrawing()
    {
        Debug.Log("I will start drawing because isDrawing is: "+ isDrawing);
        trailPrefab = GameObject.Find("DrawObject");
        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
        {
            Debug.Log("I am on start Phase");
            Ray mRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            float rayDistance;
            if (objPlane.Raycast(mRay, out rayDistance))
                startPos = mRay.GetPoint(rayDistance);
            thisTrail = (GameObject)Instantiate(trailPrefab, startPos, Quaternion.identity);
        }
        else if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
                 || Input.GetMouseButton(0)))
        {
            Debug.Log("I am moving");
            Ray mRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            float rayDistance;
            if (objPlane.Raycast(mRay, out rayDistance))
                thisTrail.transform.position = mRay.GetPoint(rayDistance);
        }
        else if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) ||
                Input.GetMouseButtonUp(0))
        {
            Debug.Log("I am at the end Phase");
            //Debug.Log("ThisTrail: " + thisTrail.transform.position);
            //Debug.Log("start: " + startPos);
            if (Vector3.Distance(thisTrail.transform.position, startPos) < 0.1)
                Destroy(thisTrail);
        }
    }
}