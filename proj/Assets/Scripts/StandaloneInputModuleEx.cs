using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class StandaloneInputModuleEx : StandaloneInputModule
{
    private Vector2 previousMouse = Vector2.zero;
    private GameObject lastAnalogSelected;
    private bool useMouse = false;

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

            // Event system reference
            EventSystem evSys = UIManager.instance.eventSystem;

            // Switch to mouse & keyboard
            if (input.mousePosition != previousMouse || input.GetMouseButton(0) || input.GetMouseButton(1) || input.GetMouseButton(2))
            {
                GameManager.controllerType = ControllerType.Keyboard;

                useMouse = true;
                if (evSys.currentSelectedGameObject != null)
                    lastAnalogSelected = evSys.currentSelectedGameObject;
                evSys.SetSelectedGameObject(null);
            }
            else
            {
                foreach (string verb in GameManager.inputVerbs)
                {
                    if (Input.GetAxis("Keyboard " + verb) != 0f)
                    {
                        GameManager.controllerType = ControllerType.Keyboard;
                        break;
                    }
                }

                if (Input.anyKey && evSys.currentSelectedGameObject == null)
                {
                    ClearSelection();
                    evSys.SetSelectedGameObject(lastAnalogSelected);
                    useMouse = false;
                }
            }

            // Switch to gamepad
            foreach (string verb in GameManager.inputVerbs)
            {
                if (Input.GetAxis("Gamepad " + verb) != 0f || Input.GetButton("Gamepad " + verb))
                {
                    ClearSelection();
                    GameManager.controllerType = ControllerType.Gamepad;
                    evSys.SetSelectedGameObject(lastAnalogSelected);
                    break;
                }
            }

            // Check for change
            if (GameManager.controllerType != prevControlType)
            {
                UIManager.inputDeviceFadeCounter = 0f;
                evSys.SetSelectedGameObject(lastAnalogSelected);
            }
        }


        // Only process the mouse if in m&k mode
        if (GameManager.controllerType == ControllerType.Keyboard && useMouse)
            ProcessMouseEvent();

        previousMouse = input.mousePosition;
    }
}
