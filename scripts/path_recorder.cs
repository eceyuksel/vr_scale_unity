using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class path_recorder : MonoBehaviour
{
    public float recordingInterval = 1f;

    private List<Vector3> playerPositions = new List<Vector3>();
    private List<Vector3> playerRotations = new List<Vector3>();
    private float timer = 0f;

    private string fileName;
    private string sceneNameHeader = "Scene Name is ";
    private string participantIdHeader = "Participant Number is ";

    private bool headerWritten = false; // Flag to track if the header is written

    void Start()
    {
        // Set the file name based on the scene name and participant ID
        string participant = experimentFlow.participant; // Assuming you have the participant ID set in the "experimentFlow" script
        fileName = "path_" + SceneManager.GetActiveScene().name + "_" + participant + DateTime.Now.ToString("_yyyy-MM-dd_hhmm") + ".txt";
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= recordingInterval)
        {
            timer = 0f;
            RecordPlayerPositionAndRotation();
        }
    }


    private void RecordPlayerPositionAndRotation()
    {
        GameObject KeyboardMouseController = GameObject.Find("KeyboardMouseController");
        if (KeyboardMouseController != null)
        {
           
            Vector3 currentPosition = KeyboardMouseController.transform.position;
            Vector3 currentRotation = KeyboardMouseController.transform.rotation.eulerAngles;

            playerPositions.Add(new Vector3(currentPosition.x, currentPosition.y, currentPosition.z));
            playerRotations.Add(new Vector3(currentRotation.x, currentRotation.y, currentRotation.z));

            Debug.Log("Rotation added: " + currentRotation);

            if (!headerWritten)
            {
                WriteHeaderToFile();
                headerWritten = true;
            }
          
        }
    }



    private void WriteHeaderToFile()
    {
        string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        string pathString = Path.Combine(path, "vr_scale_raw_data");
        Directory.CreateDirectory(pathString);
        string fullFileName = Path.Combine(pathString, fileName);

        string header = sceneNameHeader  + SceneManager.GetActiveScene().name + "\n";
        header += participantIdHeader + experimentFlow.participant + "\n";
        header += "x,y,z\n"; // Include "x", "y", and "z" as headers
        header += "x,y,z,x_rotation,y_rotation,z_rotation\n";

        using (StreamWriter sw = File.CreateText(fullFileName))
        {
            sw.WriteLine(header);
        }
    }

    private void SaveDataToFile()
    {
        string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        string pathString = Path.Combine(path, "vr_scale_raw_data");
        Directory.CreateDirectory(pathString);
        string fullFileName = Path.Combine(pathString, fileName);

        using (StreamWriter sw = File.AppendText(fullFileName))
        {
            for (int i = 0; i < playerPositions.Count; i++)
            {
                sw.WriteLine(playerPositions[i].x + "," + playerPositions[i].y + "," + playerPositions[i].z + ","
                             + playerRotations[i].x + "," + playerRotations[i].y + "," + playerRotations[i].z);
            }
        }
    }

    private void FinishRecordingAndSaveData()
    {
        SaveDataToFile();
    }

    private void OnApplicationQuit()
    {
        FinishRecordingAndSaveData();
    }

    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        FinishRecordingAndSaveData();
    }

    private void OnSceneUnloaded(Scene scene)
    {
        FinishRecordingAndSaveData();
    }
}
