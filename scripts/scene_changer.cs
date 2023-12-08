using UnityEngine;
using UnityEngine.SceneManagement;

public class scene_changer : MonoBehaviour
{
    // Define the name of the next scene you want to load
    public string nextScene;

    // Update is called once per frame
    void Update()
    {
        // Check if the Enter key is pressed
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {

            Debug.Log("Enter key pressed. Loading next scene: " + nextScene);
            // Call the LoadNextScene function to change the scene
            LoadNextScene();
        }
    }

    public void LoadNextScene()
    {
        Debug.Log("Loading next scene: " + nextScene);

        // Load the next scene using SceneManager
        SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
    }
}