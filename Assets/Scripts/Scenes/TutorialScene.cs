using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialScene : SceneBasis
{
    // Line pair variables
    private LinePair testLinePair;
    private float[] UpDownTime = { 0, 0 };
    private bool[] UpDownHeld = { false, false };


    public TutorialScene(InputActionReference[] controls) :
        base(Resources.Load("Scenes/Tutorial Screen"), controls)
    { }

    public override void Start()
    {
        base.Start();
        // Add a line pair to the active scene
        testLinePair = activeScene.AddComponent<LinePair>();
        // Use the default line size
        testLinePair.MakeLines("DEFAULT", 0.5f);
        testLinePair.lines.transform.position = new Vector3(14, 0, 4);
        // Colour the controller
    }
    public override void RegisterControls()
    {
        base.RegisterControls();
        // Joystick scaling for the example line pair
        controllerButtons[(int)Constants.CONTROLS.BUTTON].action.performed += ToggleDestroyFlag;
        controllerButtons[(int)Constants.CONTROLS.UP].action.canceled += StopJUp;
        controllerButtons[(int)Constants.CONTROLS.UP].action.performed += StartJUp;
        controllerButtons[(int)Constants.CONTROLS.DOWN].action.canceled += StopJDown;
        controllerButtons[(int)Constants.CONTROLS.DOWN].action.performed += StartJDown;
    }

    private void StopJUp(InputAction.CallbackContext context)
    {
        UpDownHeld[0] = false;
        UpDownTime[0] = 0;
    }

    private void StartJUp(InputAction.CallbackContext context)
    {
        // Perform base action
        testLinePair.IncreaseSize(controllerButtons[(int)Constants.CONTROLS.TRIGGER].action.inProgress);
        // Start adding to time
        UpDownHeld[0] = true;
    }

    private void StopJDown(InputAction.CallbackContext context)
    {
        UpDownHeld[1] = false;
        UpDownTime[1] = 0;
    }

    private void StartJDown(InputAction.CallbackContext context)
    {
        // Perform base action
        testLinePair.DecreaseSize(controllerButtons[(int)Constants.CONTROLS.TRIGGER].action.inProgress);
        // Start adding to time
        UpDownHeld[1] = true;
    }
    public override void DeregisterControls()
    {
        base.DeregisterControls();
        // REMOVE EVERYTHING THAT WAS SET ABOVE
        controllerButtons[(int)Constants.CONTROLS.BUTTON].action.performed -= ToggleDestroyFlag;
        controllerButtons[(int)Constants.CONTROLS.UP].action.canceled -= StopJUp;
        controllerButtons[(int)Constants.CONTROLS.UP].action.performed -= StartJUp;
        controllerButtons[(int)Constants.CONTROLS.DOWN].action.canceled -= StopJDown;
        controllerButtons[(int)Constants.CONTROLS.DOWN].action.performed -= StartJDown;
    }

    public override void Destroy()
    {
        base.Destroy();
        // Also destroy the line pair
        testLinePair.Remove();
    }

    public override void Update()
    {
        if (UpDownHeld[0])
        {
            // Add delta time (done in seconds)
            UpDownTime[0] += Time.deltaTime;
            if (UpDownTime[0] > 0.5)
            {
                // Repeatedly increase size
                testLinePair.IncreaseSize(true);
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
                testLinePair.DecreaseSize(true);
            }
        }
    }
}