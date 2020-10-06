using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveYAxis : MonoBehaviour
{
    /// Movement speed units per second
    [SerializeField]
    public float speed;

    /// X coordinate of the initial press
    // The '?' makes the float nullable
    private float? pressY;



    /// Called once every frame
    private void Update()
    {
        // If pressed with one finger
        if (Input.GetMouseButtonDown(0))
            pressY = Input.touches[0].position.y;
        else if (Input.GetMouseButtonUp(0))
            pressY = null;


        if (pressY != null)
        {
            float currentY = Input.touches[0].position.y;

            // The finger of initial press is now left of the press position
            if (currentY < pressY)
                Move(-speed);

            // The finger of initial press is now right of the press position
            else if (currentY > pressY)
                Move(speed);

            // else is not required as if you manage (somehow)
            // move you finger back to initial X coordinate
            // you should just be staying still
        }
    }


    
    /// Moves the player
    private void Move(float velocity)
    {
        transform.position += Vector3.up * velocity * Time.deltaTime;
    }

}
