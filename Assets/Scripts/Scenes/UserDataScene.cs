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
    private double[] eyeTestScores = { 0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0 };
    private int[] eyeTestLR = { 0, 0 };
    // LEFT = 0, RIGHT = 1, GLASSES = 2
    private int currSelection = 0;
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
        WriteEyeText();
    }

    public override void RegisterControls()
    {
        base.RegisterControls();
        // Swap eyes
        controllerButtons[(int)Constants.CONTROLS.BUTTON].action.performed += ToggleDestroyFlag;
        controllerButtons[(int)Constants.CONTROLS.TRIGGER].action.performed += SwapEyeIndex;
        // Change prescription numbers with joysticks
        controllerButtons[(int)Constants.CONTROLS.UP].action.performed += EyeValueUp;
        controllerButtons[(int)Constants.CONTROLS.DOWN].action.performed += EyeValueDown;
    }
    private void WriteEyeText()
    {
        // Change formatting based on which field is edited
        // Just highlight the selected field yellow
        switch (currSelection)
        {
            case 0:
                // Left eye
                eyeAcuityText.text = "<color=yellow>Left Eye (LogMAR): " + eyeVal[0].ToString("F1") + "\n</color>" +
                    "Right Eye (LogMAR): " + eyeVal[1].ToString("F1") + "\n";
                eyeDataText.text = "Glasses: " + (eyeVal[2] == 0 ? "No" : "Yes");
                break;
            case 1:
                // Right eye
                eyeAcuityText.text = "Left Eye (LogMAR): " + eyeVal[0].ToString("F1") + "\n" +
                    "<color=yellow>Right Eye (LogMAR): " + eyeVal[1].ToString("F1") + "\n</color>";
                eyeDataText.text = "Glasses: " + (eyeVal[2] == 0 ? "No" : "Yes");
                break;
            case 2:
                // Glasses
                eyeAcuityText.text = "Left Eye (LogMAR): " + eyeVal[0].ToString("F1") + "\n" +
                    "Right Eye (LogMAR): " + eyeVal[1].ToString("F1") + "\n";
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
    private void EyeValueUp(InputAction.CallbackContext context)
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
            if (eyeTestLR[currSelection] == eyeTestScores.Length)
            {
                eyeTestLR[currSelection] = eyeTestScores.Length - 1;
            }
            eyeVal[currSelection] = eyeTestScores[eyeTestLR[currSelection]];
        }
        // Rewrite the text to screen
        WriteEyeText();
    }

    private void EyeValueDown(InputAction.CallbackContext context)
    {
        if (currSelection == 2)
        {
            // Glasses edit, 1 or 0
            eyeVal[currSelection] = (eyeVal[currSelection] + 1) % 2;
        }
        else
        {
            eyeTestLR[currSelection] -= 1;
            if (eyeTestLR[currSelection] < 0)
            {
                eyeTestLR[currSelection] = 0;
            }
            eyeVal[currSelection] = eyeTestScores[eyeTestLR[currSelection]];
        }
        // Rewrite the text to screen
        WriteEyeText();
    }

    public override void DeregisterControls()
    {
        base.DeregisterControls();
        // REMOVE EVERYTHING THAT WAS SET ABOVE
        controllerButtons[(int)Constants.CONTROLS.BUTTON].action.performed -= ToggleDestroyFlag;
        controllerButtons[(int)Constants.CONTROLS.TRIGGER].action.performed -= SwapEyeIndex;
        // Change prescription numbers with joysticks
        controllerButtons[(int)Constants.CONTROLS.UP].action.performed -= EyeValueUp;
        controllerButtons[(int)Constants.CONTROLS.DOWN].action.performed -= EyeValueDown;
    }

    public override void Destroy()
    {
        base.Destroy();
        // Write eye data to logs
        log.LogUserData(eyeVal[0], eyeVal[1], eyeVal[2]);
    }
}