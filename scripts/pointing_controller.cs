using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

public class pointing_controller : MonoBehaviour
{
    private List<string> buildingNames = new List<string>();
    private List<string> names = new List<string>();
    public int pointingDiamondIndex;
    public int facingDiamondIndex;
    public int targetBuildingIndex;
    public List<int> targetBuildingIndicesRemaining = new List<int>();
    public int targetBuildingIndicesRemainingIndex;
    public GameObject pointingPromptObject;

    public string waitingForClick;
    public Camera mainCamera;
    private Ray screenRay;
    public Vector3 currentPosition;
    public Vector3 facingDiamondPosition;
    public float pointingAngle;
    public float pointingAngle_wrong;
    private string fileName = "desktop_out_" + DateTime.Now.ToString("dd-MM-yyyy_hhmmss") + ".txt";

    private void Start()
    {
        // Create file to store pointing data
        string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        string pathString = System.IO.Path.Combine(path, "VRScaleData");
        System.IO.Directory.CreateDirectory(pathString);
        string fullFileName = System.IO.Path.Combine(pathString, fileName);

        string header = "pointingDiamondIndex,facingDiamondIndex,targetBuildingIndex,pointingAngle\n";

        using (System.IO.StreamWriter sw = System.IO.File.CreateText(fullFileName))
        {
            sw.WriteLine(header);
        }

        pointingPromptObject = GameObject.Find("PointingPrompt");
        mainCamera = Camera.main;

        // Populate arrays
        buildingNames.Add("Roundabout Restaurant");
        buildingNames.Add("Parking Garage");
        buildingNames.Add("Mechanic");
        buildingNames.Add("Library");
        buildingNames.Add("Palm Business Center");
        buildingNames.Add("Window Shop");

        for (int i = 0; i < buildingNames.Count; i++)
        {
            names.Add(buildingNames[i] + " Diamond");
        }

        startPointingSet(0);
    }

    void startPointingSet(int startLandmarkIndex)
    {
        if (startLandmarkIndex < buildingNames.Count)
        {
            pointingDiamondIndex = startLandmarkIndex;

            // Move to diamond
            Debug.Log(names[startLandmarkIndex]);
            GameObject navigator = GameObject.Find("KeyboardMouseController");
            if (navigator)
            {
                Debug.Log("Navigator found");
            }
            Vector3 diamondPosition = GameObject.Find(names[startLandmarkIndex]).transform.position;
            diamondPosition.y = 2.7f;
            navigator.transform.position = diamondPosition;
            if (startLandmarkIndex == 0) facingDiamondIndex = 0;
            else if (startLandmarkIndex == 1) facingDiamondIndex = 0;
            else if (startLandmarkIndex == 2) facingDiamondIndex = 0;
            else if (startLandmarkIndex == 3) facingDiamondIndex = 0;
            else if (startLandmarkIndex == 4) facingDiamondIndex = 0;
            else if (startLandmarkIndex == 5) facingDiamondIndex = 0;

            targetBuildingIndicesRemaining = new List<int>(new int[] { 0, 1, 2, 3, 4, 5 });
            targetBuildingIndicesRemaining.RemoveAt(startLandmarkIndex);
            Shuffle(targetBuildingIndicesRemaining);

            showPointingQuestion();
        }
        else
        {
            // All done -- return to browser or perform any other desired action
            Debug.Log("All done -- return to browser");
            Application.Quit();
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
        if (Input.GetButtonDown("joystick button 1"))
        {
            // Button press detected, perform your pointing action here
            Debug.Log("PS4 X Button Pressed");

            screenRay = mainCamera.ScreenPointToRay(Input.mousePosition);
            currentPosition = GameObject.Find("KeyboardMouseController").transform.position;
            Vector3 targetDiamondPosition = GameObject.Find(names[targetBuildingIndex]).transform.position;
            currentPosition.y = facingDiamondPosition.y;
            Debug.DrawRay(currentPosition, screenRay.direction * 2000, Color.red);
            Debug.DrawLine(currentPosition, facingDiamondPosition, Color.blue);

            // Calculate pointing angle
            pointingAngle = Vector3.SignedAngle((targetDiamondPosition - currentPosition), screenRay.direction, Vector3.up);

            // Send pointing angle to the file
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string pathString = System.IO.Path.Combine(path, "VRScaleData");
            System.IO.Directory.CreateDirectory(pathString);
            string fullFileName = System.IO.Path.Combine(pathString, fileName);

            using (System.IO.StreamWriter sw = System.IO.File.AppendText(fullFileName))
            {
                sw.WriteLine(buildingNames[pointingDiamondIndex] + "," + buildingNames[facingDiamondIndex] + "," + buildingNames[targetBuildingIndex] + "," + pointingAngle + "\n");
            }

            showPointingQuestion();
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
