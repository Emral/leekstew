using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class StandaloneInputModuleEx : StandaloneInputModule
{
    private Vector2 previousMouse = Vector2.zero;
    private GameObject lastSelectedObj;
    private GameObject currentSelection;
    private GameObject currentHover;
    private bool useMouse = false;
    private bool previousUseMouse = false;
    private ControllerType prevControlType;



    public void RefreshSelectionToLast()
    {
        if (GameManager.controllerType != prevControlType || useMouse != previousUseMouse)
        {
            ClearSelection();
            eventSystem.SetSelectedGameObject(currentSelection);
        }
    }


    public void SwitchToMouse()
    {
        GameManager.controllerType = ControllerType.Keyboard;
        useMouse = true;
    }


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

        // Only process the mouse if in m&k mode
        if (GameManager.controllerType == ControllerType.Keyboard && useMouse)
            ProcessMouseEvent();



        if (GameManager.isGamePaused)
        {
            // Store previous control type
            prevControlType = GameManager.controllerType;

            // If mouse input detected, switch to mouse & keyboard controls using the mouse
            if (input.mousePosition != previousMouse || input.GetMouseButton(0) || input.GetMouseButton(1) || input.GetMouseButton(2))
            {
                SwitchToMouse();
            }

            // Otherwise, use
            else if (Input.anyKey)
            {
                GameManager.controllerType = ControllerType.Keyboard;
                useMouse = false;
            }


            // Switch to gamepad
            foreach (string verb in GameManager.inputVerbs)
            {
                if (Input.GetAxis("Gamepad " + verb) != 0f || Input.GetButton("Gamepad " + verb))
                {
                    GameManager.controllerType = ControllerType.Gamepad;
                    break;
                }
            }


            // Popup when controller type changes
            if (GameManager.controllerType != prevControlType)
            {
                UIManager.inputDeviceFadeCounter = 0f;
            }



            // Manage current hover vs current selection
            currentSelection = eventSystem.currentSelectedGameObject;
            if (useMouse && GameManager.controllerType == ControllerType.Keyboard)
            {
                GameObject obj = GetCurrentFocusedGameObject();
                if (obj != null && obj.layer == 15)
                    currentSelection = obj;
            }

            if (currentSelection != null)
                lastSelectedObj = currentSelection;

            if (eventSystem.currentSelectedGameObject != currentSelection)
            {
                ClearSelection();
                eventSystem.SetSelectedGameObject (currentSelection);
            }


            // Refresh the selection if need be
            RefreshSelectionToLast();
        }

        previousMouse = input.mousePosition;
        previousUseMouse = useMouse;
    }
}
