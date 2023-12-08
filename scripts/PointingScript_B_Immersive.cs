using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.XR;
using Valve.VR;
using UnityEngine.Events;
using UnityEngine.SceneManagement;



public class PointingScript_B_Immersive : MonoBehaviour
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
    private bool isHeaderWritten = false;
    private GameObject navigator;
    private string fileName = "pointing_immersive_b_" + experimentFlow.participant + DateTime.Now.ToString("_yyyy-MM-dd_hhmm") + ".txt";

    private GameObject controller;
    private List<InputDevice> controllers = new List<InputDevice>();
    private float lastDown = 0.0f;
    private bool down = false; //track first down
    private bool pressed = false; //track is down

    public SteamVR_Action_Boolean grabPinch;
    public SteamVR_Input_Sources inputSource = SteamVR_Input_Sources.Any;

    private SteamVR_Action_Pose controllerPoseAction;

    private void Start()
    {

        // Create file to store pointing data
        string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        string pathString = System.IO.Path.Combine(path, "vr_scale_raw_data");
        System.IO.Directory.CreateDirectory(pathString);
        string fullFileName = System.IO.Path.Combine(pathString, fileName);

        //controllerPoseAction = SteamVR_Actions.default_Pose;
        controller = GameObject.Find("Controller (right)");

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

        // populate arrays

        buildingNames.Add("Mountain Overlook Office");
        buildingNames.Add("Accordion Business Center");
        buildingNames.Add("Riverview");
        buildingNames.Add("Jenga Building");
        buildingNames.Add("Reflection Theater");
        buildingNames.Add("Lockdown Building");

        for (int i = 0; i < buildingNames.Count; i++)
        {
            names.Add(buildingNames[i] + " Diamond");
        }


        List<InputDevice> inputDevices = new List<InputDevice>();
        InputDevices.GetDevices(inputDevices);
        Debug.Log("input devices: " + inputDevices.Count);
        foreach (InputDevice device in inputDevices)
        {
            bool discard;
            if (device.TryGetFeatureValue(CommonUsages.triggerButton, out discard))
            {
                controllers.Add(device);
                Debug.Log(device.name);
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
            GameObject navigator = GameObject.Find("PlayerControllers");
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
                navigator.transform.LookAt(nextDiamondPosition);
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

            // Update facingDiamondIndex to the next building index
            facingDiamondIndex = (pointingDiamondIndex + 1) % buildingNames.Count;

            // show instructions
            pointingPromptObject.GetComponentInChildren<UnityEngine.UI.Text>().text = "Point to " + buildingNames[targetBuildingIndex];
        }
        else
        {
            startPointingSet(pointingDiamondIndex + 1);
        }
    }


    void Update()
    {
             
        if (grabPinch.GetStateDown(inputSource))
        {

            targetBuildingIndicesRemaining.RemoveAt(0);

            // Get the controller's orientation using your existing SteamVR_Action_Pose
            //Quaternion controllerOrientation = controllerPoseAction.GetLocalRotation(inputSource);

            // We only need the Y rotation for the controller's pointing angle ECE on 9/11/2023
            float pointingAngle = controller.transform.eulerAngles.y;

            // Print the pointing angle
            Debug.Log("Pointing Angle: " + pointingAngle);


            // send pointing angle to the file
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string pathString = System.IO.Path.Combine(path, "vr_scale_raw_data");
            System.IO.Directory.CreateDirectory(pathString);
            string fullFileName = System.IO.Path.Combine(pathString, fileName);

            using (System.IO.StreamWriter sw = System.IO.File.AppendText(fullFileName))
            {
                sw.WriteLine(buildingNames[pointingDiamondIndex] + "," + buildingNames[facingDiamondIndex] + "," + buildingNames[targetBuildingIndex] + "," + pointingAngle + "\n");
            }

            Input.ResetInputAxes(); // we don't want this repeated multiple times
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