using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
public class RumbleManager : MonoBehaviour
{
    private static RumbleManager _instance;

    public SteamVR_Action_Vibration Vibration;

    public static RumbleManager Instance { get => _instance; set => _instance = value; }


    private void Awake()
    {
        if (_instance)
        {
            Destroy(_instance.gameObject);
            _instance = this;
        }

        _instance = this;
    }


    /// <summary>
    /// Light vibration - Used for button confirmations 
    /// </summary>
    /// <param name="_hand"></param>
    public void lightVibration(HandTypes _hand)
    {
        if(_hand == HandTypes.left)
        {
            Vibration.Execute(0, 0.1f, 3, 1, SteamVR_Input_Sources.LeftHand);
        }
        else
        {
            Vibration.Execute(0, 0.1f, 3, 1, SteamVR_Input_Sources.RightHand);
        }
    }


    public void heavyVibration(HandTypes _hand)
    {
        if (_hand == HandTypes.left)
        {
            Vibration.Execute(0, 0.5f, 30, 10, SteamVR_Input_Sources.LeftHand);
        }
        else
        {
            Vibration.Execute(0, 0.5f, 30, 10, SteamVR_Input_Sources.RightHand);
        }
    }

}
