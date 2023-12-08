using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PointingScript_A_Desktop : MonoBehaviour
{
    private List<string> buildingNames = new List<string>();
    private List<GameObject> diamondObjects = new List<GameObject>();
    public int pointingDiamondIndex;
    public int facingDiamondIndex;
    public int targetBuildingIndex;
    public List<int> targetBuildingIndicesRemaining = new List<int>();
    public int targetBuildingIndicesRemainingIndex;
    public GameObject pointingPromptObject;
    private GameObject navigator;
    private List<string> names = new List<string>();

    public string waitingForClick;
    public Camera mainCamera;
    private Ray screenRay;
    public Vector3 currentPosition;
    public Vector3 facingDiamondPosition;
    public float pointingAngle;
    private string fileName;
    private bool isHeaderWritten = false;
    private string path;
    private string pathString;
    private string fullFileName;

    private void Start()
    {
  
        fileName = "pointing_desktop_a_" + experimentFlow.participant + DateTime.Now.ToString("_yyyy-MM-dd_hhmm") + ".txt";

        // Create file to store pointing data
        path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        pathString = System.IO.Path.Combine(path, "vr_scale_raw_data");
        System.IO.Directory.CreateDirectory(pathString);
        fullFileName = System.IO.Path.Combine(pathString, fileName);

        // Write the header information to the file only if it's not already written
        using (System.IO.StreamWriter sw = isHeaderWritten ? System.IO.File.AppendText(fullFileName) : new System.IO.StreamWriter(fullFileName))
        {
            if (!isHeaderWritten)
            {
                // Add the header with scene name and participant number
                string header = "Scene Name is " + SceneManager.GetActiveScene().name + "\n";
                header += "Participant Number is " + experimentFlow.participant + "\n";
                header += "pointingDiamondIndex,facingDiamondIndex,targetBuildingIndex,pointingAngle\n";
                sw.WriteLine(header);
            }

            isHeaderWritten = true;
        }

        pointingPromptObject = GameObject.Find("PointingPrompt");
        mainCamera = Camera.main;

        // Populate arrays
        buildingNames.Add("Window Shop");
        buildingNames.Add("Palm Business Center");
        buildingNames.Add("Library");
        buildingNames.Add("Mechanic");
        buildingNames.Add("Parking Garage");
        buildingNames.Add("Roundabout Restaurant");

        // Populate diamondObjects list
        for (int i = 0; i < buildingNames.Count; i++)
        {
            GameObject diamond = GameObject.Find(buildingNames[i]);
            if (diamond != null)
            {
                diamondObjects.Add(diamond);
            }
        }

        startPointingSet(0);
    }

    void startPointingSet(int startLandmarkIndex)
    {
        if (startLandmarkIndex < buildingNames.Count)
        {
            pointingDiamondIndex = startLandmarkIndex;

            // Move to diamond
            Debug.Log(buildingNames[startLandmarkIndex]);
            GameObject navigator = GameObject.Find("KeyboardMouseController");
            if (navigator)
            {
                Debug.Log("Navigator found");
            }
            GameObject diamond = GameObject.Find(buildingNames[startLandmarkIndex] + " Diamond");
            Vector3 diamondPosition = diamond.transform.position;
            diamondPosition.y = 2.7f;
            navigator.transform.position = diamondPosition;

            facingDiamondIndex = (startLandmarkIndex + 1) % buildingNames.Count;

            // Face the next diamond
            if (facingDiamondIndex < buildingNames.Count)
            {
                GameObject nextDiamond = GameObject.Find(buildingNames[facingDiamondIndex] + " Diamond");
                Vector3 nextDiamondPosition = nextDiamond.transform.position;
                nextDiamondPosition.y = 2.7f;
                //Take the next line and turn it into code that will calculate the (global) angle between the position of the navigator and the
                // the position of the next diamond. 

                // Calculate the angle between the navigator and the next diamond
                Vector3 direction = nextDiamondPosition - diamondPosition;

                //float newAngle = Vector3.Angle(direction, navigator.transform.forward);
                float newAngle = Vector3.SignedAngle(Vector3.forward, direction, Vector3.up);

                Debug.Log(Vector3.forward + "vector3.forward");
                Debug.Log(navigator.transform.forward + "navigator.transform.forward");
                Debug.Log(newAngle + "newAngle");

                //navigator.transform.LookAt(nextDiamondPosition);
                navigator.GetComponent<Nav_Learning_Controller_Movement>().yRot = newAngle;
            }

            targetBuildingIndicesRemaining = new List<int>(new int[] { 0, 1, 2, 3, 4, 5 });
            targetBuildingIndicesRemaining.Remove(startLandmarkIndex);
            Shuffle(targetBuildingIndicesRemaining);

            showPointingQuestion();
        }
        else
        {
            // All done -- return to browser or perform any other desired action
            Debug.Log("All done -- return to browser");
            Application.Quit();

            // Add task-ended message to the prompt
            pointingPromptObject.GetComponentInChildren<UnityEngine.UI.Text>().text = "Task ended. Thank you!";
        }
    }

    void showPointingQuestion()
    {
        if (targetBuildingIndicesRemaining.Count > 0)
        {
            targetBuildingIndex = targetBuildingIndicesRemaining[0];

            // Show instructions
            pointingPromptObject.GetComponentInChildren<UnityEngine.UI.Text>().text = "Point to " + buildingNames[targetBuildingIndex];
        }
        else
        {
            startPointingSet(pointingDiamondIndex + 1);
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("ps4x"))
        {
            // Capture joystick input for pointing direction
            currentPosition = GameObject.Find("KeyboardMouseController").transform.position;
            currentPosition.y = facingDiamondPosition.y;
            Vector3 pointingDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            facingDiamondPosition = GameObject.Find(buildingNames[pointingDiamondIndex]).transform.position;
            pointingDirection.y = facingDiamondPosition.y;
            pointingDirection.Normalize();

            // Calculate the screen ray based on pointing direction
            screenRay = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

            Vector3 targetDiamondPosition = diamondObjects[targetBuildingIndex].transform.position;

            currentPosition.y = targetDiamondPosition.y;

            Debug.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * 400, Color.red); // Draw the red ray

            // Calculate the north direction vector
            Vector3 northDirection = Vector3.forward;

            Vector3 blueLineEnd = currentPosition + northDirection.normalized * 200;

            Debug.DrawLine(currentPosition, blueLineEnd, Color.blue);

            // Calculate pointing angle in Y-axis only
            Vector3 cameraForwardXZ = new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z).normalized;
            Vector3 northDirectionXZ = new Vector3(northDirection.x, 0, northDirection.z).normalized;


            // Calculate pointing angle in XZ plane
            pointingAngle = Vector3.SignedAngle(northDirectionXZ, cameraForwardXZ, Vector3.up);


            Debug.Log("pointing angle" + pointingAngle);

            if (Input.GetButtonDown("ps4x"))
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string pathString = System.IO.Path.Combine(path, "vr_scale_raw_data");
                System.IO.Directory.CreateDirectory(pathString);
                string fullFileName = System.IO.Path.Combine(pathString, fileName);

                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fullFileName, true))
                {
                    sw.WriteLine(buildingNames[pointingDiamondIndex] + "," + buildingNames[facingDiamondIndex] + "," + buildingNames[targetBuildingIndex] + "," + pointingAngle);
                }


                targetBuildingIndicesRemaining.RemoveAt(0);


                showPointingQuestion();
            }

        }

    }


    public static List<T> Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 0)
        {
            n--;
            int k = UnityEngine.Random.Range(n, list.Count);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }
}
