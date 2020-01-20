using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ControllerInputs : MonoBehaviour
{
    public SteamVR_Input_Sources handType; // 1
    public SteamVR_Action_Boolean teleportAction; // 2
    public SteamVR_Action_Boolean grabAction; // 3

    // Update is called once per frame
    void Update()
    {
        InputManager.Instance.RightGripped = false;
        InputManager.Instance.RightGripped = false;
        if (GetTeleportDown())
        {
            //print("Teleport " + handType);
        }

        if (GetGrab())
        {
            print("Grab " + handType);
            if (handType == SteamVR_Input_Sources.LeftHand)
            {
                InputManager.Instance.LeftGripped = true;
            }
            if (handType == SteamVR_Input_Sources.RightHand)
            {
                InputManager.Instance.RightGripped = true;
            }
        }
    }

    public bool GetTeleportDown() // 1
    {
        return teleportAction.GetStateDown(handType);
    }

    public bool GetGrab() // 2
    {
        return grabAction.GetState(handType);
    }

}
