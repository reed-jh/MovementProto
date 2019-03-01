using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseHandler : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // Toggle pause on pressing P
        if (Input.GetKeyDown(KeyCode.P))
        {
            ObjectController.PAUSED ^= true;
        }
    }
}
