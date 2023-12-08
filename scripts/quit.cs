using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class quit : MonoBehaviour
{
    void Update()
    {
        // Check if the X key is pressed
        if (Input.GetKeyDown(KeyCode.X))
        {
            // Quit the application
            Application.Quit();

            // Note: Application.Quit() may not work in the Unity Editor. It's best tested in a built application.
        }
    }
}
