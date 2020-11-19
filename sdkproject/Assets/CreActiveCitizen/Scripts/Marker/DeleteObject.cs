using Mapbox.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteObject : MonoBehaviour
{
    private readonly AddCustomMarker Marker = new AddCustomMarker();

    public AudioSource audioSource;  //audio source

    private bool canDelete = false;

    // Use this for initialization
    void Start()
    {
        //if (Marker.loggedUserID == 0)
        //{
        this.GetComponent<MeshRenderer>().enabled = false;
        //}
    }

    // Update is called once per frame
    void Update()
    {
        foreach (UnityEngine.Touch t in Input.touches)
        {
            if (t.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(t.position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    foreach (Component tile in GetComponentsInChildren<Transform>())
                    {
                        if (hit.collider.gameObject == tile.gameObject)
                        {
                            canDelete = true;
                        }
                    }
                }
            }
        }

        if (canDelete == true)
        {
            //transform.Rotate(-Time.deltaTime * 50, 0.0f, 0.0f);
            Debug.Log("Can be destored now");
            Destroy(this.transform.root.gameObject);
        }
    }
 }