using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UserDataScene : SceneBasis
{
    private string UUID;
    private TextMeshPro eyeAcuityText;
    private TextMeshPro eyeDataText;
    // Stores visual acuity for left eye, right eye, and glasses state, respectively
    private double[] eyeVal = { 0, 0, 0};
    // Possible values of the LogMAR chart used
    private List<double> eyeTestScores = new List<double>();
    private int[] eyeTestLR = { 0, 0 };
    // LEFT = 0, RIGHT = 1, GLASSES = 2
    private int currSelection = 0;
    // Holding for easier eye test scores
    private float[] UpDownTime = { 0, 0 };
    private bool[] UpDownHeld = { false, false };
    // Logger reference
    LogController log;
    
    public UserDataScene(InputActionReference[] controls, LogController logger, string userUUID) :
        base(Resources.Load("Scenes/User Data Screen"), controls)
    {
        UUID = userUUID;
        log = logger;
    }

    public override void Start()
    {
        base.Start();
        // Get UUID and active text
        var uuidText = GameObject.Find("UUID").GetComponent<TextMeshPro>().text = UUID;
        eyeAcuityText = GameObject.Find("Eye Acuity").GetComponent<TextMeshPro>();
        eyeDataText = GameObject.Find("Eye Data").GetComponent<TextMeshPro>();
        // Populate eye test scores
        // First row difference
        eyeTestScores.AddRange(new List<double>{2.00, 1.75, 1.50, 1.25});
        // All following rows
        for (double x = 1.00; x > -0.01; x -= 0.02)
        {
            eyeTestScores.Add(x);
        }
        // Set default eye values
        eyeVal[0] = eyeTestScores[eyeTestLR[0]];
        eyeVal[1] = eyeTestScores[eyeTestLR[1]];
        // Draw default lines
        WriteEyeText();
    }

    public override void RegisterControls()
    {
        base.RegisterControls();
        // Swap eyes
        controllerButtons[(int)Constants.CONTROLS.BUTTON].action.performed += ToggleDestroyFlag;
        controllerButtons[(int)Constants.CONTROLS.TRIGGER].action.performed += SwapEyeIndex;
        // Change prescription numbers with joysticks
        controllerButtons[(int)Constants.CONTROLS.UP].action.performed += ToggleUp;
        controllerButtons[(int)Constants.CONTROLS.DOWN].action.performed += ToggleDown;
        controllerButtons[(int)Constants.CONTROLS.UP].action.canceled += ToggleUp;
        controllerButtons[(int)Constants.CONTROLS.DOWN].action.canceled += ToggleDown;
    }
    private void WriteEyeText()
    {
        // Change formatting based on which field is edited
        // Just highlight the selected field yellow
        switch (currSelection)
        {
            case 0:
                // Left eye
                eyeAcuityText.text = "<color=yellow>Left Eye (LogMAR): " + eyeVal[0].ToString("F2") + "\n</color>" +
                    "Right Eye (LogMAR): " + eyeVal[1].ToString("F2") + "\n";
                eyeDataText.text = "Glasses: " + (eyeVal[2] == 0 ? "No" : "Yes");
                break;
            case 1:
                // Right eye
                eyeAcuityText.text = "Left Eye (LogMAR): " + eyeVal[0].ToString("F2") + "\n" +
                    "<color=yellow>Right Eye (LogMAR): " + eyeVal[1].ToString("F2") + "\n</color>";
                eyeDataText.text = "Glasses: " + (eyeVal[2] == 0 ? "No" : "Yes");
                break;
            case 2:
                // Glasses
                eyeAcuityText.text = "Left Eye (LogMAR): " + eyeVal[0].ToString("F2") + "\n" +
                    "Right Eye (LogMAR): " + eyeVal[1].ToString("F2") + "\n";
                eyeDataText.text = "<color=yellow>Glasses: " + (eyeVal[2] == 0 ? "No" : "Yes") + "</color>";
                break;

        }
    }

    private void SwapEyeIndex(InputAction.CallbackContext context)
    {
        currSelection++;
        currSelection %= eyeVal.Length;
        WriteEyeText();
    }
    private void EyeValueDown()
    {
        if (currSelection == 2)
        {
            // Glasses edit, 1 or 0
            eyeVal[currSelection] = (eyeVal[currSelection] + 1) % 2;
        }
        else
        {
            // Eye test, update depending on left or right!
            eyeTestLR[currSelection] += 1;
            // Prevent going over limits
            if (eyeTestLR[currSelection] == eyeTestScores.Count)
            {
                eyeTestLR[currSelection] = eyeTestScores.Count - 1;
            }
            eyeVal[currSelection] = eyeTestScores[eyeTestLR[currSelection]];
        }
        // Rewrite the text to screen
        WriteEyeText();
    }

    private void EyeValueUp()
    {
        if (currSelection == 2)
        {
            // Glasses edit, 1 or 0
            eyeVal[currSelection] = (eyeVal[currSelection] + 1) % 2;
        }
        else
        {
            eyeTestLR[currSelection] -= 1;
            // Prevent going over limits
            if (eyeTestLR[currSelection] < 0)
            {
                eyeTestLR[currSelection] = 0;
            }
            eyeVal[currSelection] = eyeTestScores[eyeTestLR[currSelection]];
        }
        // Rewrite the text to screen
        WriteEyeText();
    }


    private void ToggleUp(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Perform the action
            EyeValueUp();
            // Toggle hold
            UpDownHeld[0] = true;
        }
        else if (context.canceled)
        {
            // Reset counters
            UpDownHeld[0] = false;
            UpDownTime[0] = 0;
        }
    }

    private void ToggleDown(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            EyeValueDown();
            UpDownHeld[1] = true;
        }
        else if (context.canceled)
        {
            // Reset counters
            UpDownHeld[1] = false;
            UpDownTime[1] = 0;
        }        
    }

    public override void Update()
    {
        // Check for held values
        // UP
        if (UpDownHeld[0])
        {
            // Add delta time (done in seconds)
            UpDownTime[0] += Time.deltaTime;
            if (UpDownTime[0] > 0.5)
            {
                // Repeatedly increase size
                EyeValueUp();
            }
        }
        // DOWN
        else if (UpDownHeld[1])
        {
            // Add delta time (done in seconds)
            UpDownTime[1] += Time.deltaTime;
            if (UpDownTime[1] > 0.5)
            {
                // Repeatedly increase size
                EyeValueDown();
            }
        }
    }

    public override void DeregisterControls()
    {
        base.DeregisterControls();
        // REMOVE EVERYTHING THAT WAS SET ABOVE
        controllerButtons[(int)Constants.CONTROLS.BUTTON].action.performed -= ToggleDestroyFlag;
        controllerButtons[(int)Constants.CONTROLS.TRIGGER].action.performed -= SwapEyeIndex;
        // Change prescription numbers with joysticks
        controllerButtons[(int)Constants.CONTROLS.UP].action.performed -= ToggleUp;
        controllerButtons[(int)Constants.CONTROLS.DOWN].action.performed -= ToggleDown;
        controllerButtons[(int)Constants.CONTROLS.UP].action.canceled -= ToggleUp;
        controllerButtons[(int)Constants.CONTROLS.DOWN].action.canceled -= ToggleDown;
    }

    public override void Destroy()
    {
        base.Destroy();
        // Write eye data to logs
        log.LogUserData(eyeVal[0], eyeVal[1], eyeVal[2]);
    }
}