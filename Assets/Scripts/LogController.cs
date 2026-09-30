using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

/*
    Data Logging Controller
*/
public class LogController
{
    // UUID for user
    private string UUID;
    // Debug flag
    private bool debug;
    private double eyeLeft, eyeRight, glasses;
    // Store all final line sizes
    // Horizontal Inf, Vertical Inf, Diagonal Inf, Horizontal E, Vertical E, Diagonal E
    private float[] staticLines;
    private float[] dynamicLines;

    public void Init(string userUUID, bool printDebug)
    {
        // Save the test numbers for future use
        string[] ltList = Enum.GetNames(typeof(Constants.LINE_TYPE));
        string[] loList = Enum.GetNames(typeof(Constants.LINE_ORIENTATION));
        
        // Create a new log file if it doesn't already exists
        // Look in .config/Unity3D/TMU MDSL, will save directly to headset running program
        if (!File.Exists(Application.persistentDataPath + "/" + Constants.LOGFILE))
        {
            // Set up CSV headers using tests chosen in Constants.cs
            string testHeaders = "";
            foreach (string testType in new List<string> {"STATIC", "DYNAMIC"})
            {
                foreach (string lineType in ltList)
                {
                    foreach (string lineOri in loList)
                    {
                        testHeaders += "," + string.Join(" ", testType, lineType, lineOri);
                    }
                    
                }
            }

            using (StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/" + Constants.LOGFILE))
            {
                // User data prefix
                sw.WriteLine("UUID,EYE LEFT,EYE RIGHT,GLASSES" + testHeaders);
            }
        }
        // Set the size of the static and dynamic line size lists
        staticLines = new float[ltList.Length * loList.Length];
        dynamicLines = new float[ltList.Length * loList.Length];

        // Save the UUID for later writing
        UUID = userUUID;
        debug = printDebug;
        // Note whether debug should be printed
        Debug.Log("Data being saved to: " + Application.persistentDataPath + "/" + Constants.LOGFILE);
    }

    public void LogUserData(double leftEye, double rightEye, double hasGlasses)
    {
        // Store passed data into the logger
        eyeLeft = leftEye;
        eyeRight = rightEye;
        glasses = hasGlasses;

        if (debug) { Debug.Log(string.Join(" ", "ELeft:", eyeLeft.ToString("F1"), "ERight:", eyeRight.ToString("F1"), "Glasses:", glasses.ToString("F0"))); }
    }

    public void LogLineData(float lineScale, Constants.LINE_TYPE LT, Constants.LINE_ORIENTATION LO, bool isDynamic = false)
    {
        // Store the tests in the order they were taken
        // Order is line type > line orientation
        if (isDynamic)
        {
            dynamicLines[(int)LT * Enum.GetNames(typeof(Constants.LINE_ORIENTATION)).Length + (int)LO] = lineScale;
        }
        else
        {
            staticLines[(int)LT * Enum.GetNames(typeof(Constants.LINE_ORIENTATION)).Length + (int)LO] = lineScale;
        }

        if (debug) { Debug.Log(string.Join(" ", isDynamic ? "DYNAMIC" : "STATIC", LT.ToString(), LO.ToString(), ":", lineScale)); }
    }

    public void WriteToFile()
    {
        // Write to the stored log file path
        using (StreamWriter sw = File.AppendText(Application.persistentDataPath + "/" + Constants.LOGFILE))
        {
            // Write all formatted data to the line
            // EYE DATA
            sw.Write(UUID + ",20/" + eyeLeft +
                ",20/" + eyeRight +
                "," + glasses);
            // STATIC TESTING
            for (int i = 0; i < staticLines.Length; i++)
            {
                sw.Write("," + staticLines[i].ToString("F3") + "mm");
            }
            // DYNAMIC TESTING
            for (int i = 0; i < dynamicLines.Length; i++)
            {
                sw.Write("," + dynamicLines[i].ToString("F3") + "mm");
            }
            sw.Write("\n");
        }    
    }
}