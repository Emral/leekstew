using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class StandaloneInputModuleEx : StandaloneInputModule
{
    private Vector2 previousMouse = Vector2.zero;

    public override void Process()
    {
        bool usedEvent = SendUpdateEventToSelectedObject();

        if (eventSystem.sendNavigationEvents)
        {
            if (!usedEvent)
                usedEvent |= SendMoveEventToSelectedObject();

            if (!usedEvent)
                SendSubmitEventToSelectedObject();
        }

        if (GameManager.isGamePaused)
        {
            // Store previous control type
            ControllerType prevControlType = GameManager.controllerType;

            // Switch to mouse & keyboard
            if (input.mousePosition != previousMouse || input.GetMouseButton(0) || input.GetMouseButton(1) || input.GetMouseButton(2) || Input.anyKey)
            {
                GameManager.controllerType = ControllerType.Keyboard;
            }

            // Switch to gamepad
            foreach (string verb in GameManager.inputVerbs)
            {
                if (Input.GetAxis("Gamepad "+verb) != 0f || Input.GetButton ("Gamepad " + verb))
                {
                    GameManager.controllerType = ControllerType.Gamepad;
                    break;
                }
            }

            // Check for change
            if (GameManager.controllerType != prevControlType)
                UIManager.inputDeviceFadeCounter = 0f;
        }


        // Only process the mouse if in m&k mode
        if (GameManager.controllerType == ControllerType.Keyboard)
            ProcessMouseEvent();

        previousMouse = input.mousePosition;
    }
}
