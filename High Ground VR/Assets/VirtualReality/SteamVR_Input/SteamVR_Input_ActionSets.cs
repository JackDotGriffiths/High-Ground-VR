//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Valve.VR
{
    using System;
    using UnityEngine;
    
    
    public partial class SteamVR_Actions
    {
        
        private static SteamVR_Input_ActionSet_High_GroundControlSet p_High_GroundControlSet;
        
        public static SteamVR_Input_ActionSet_High_GroundControlSet High_GroundControlSet
        {
            get
            {
                return SteamVR_Actions.p_High_GroundControlSet.GetCopy<SteamVR_Input_ActionSet_High_GroundControlSet>();
            }
        }
        
        private static void StartPreInitActionSets()
        {
            SteamVR_Actions.p_High_GroundControlSet = ((SteamVR_Input_ActionSet_High_GroundControlSet)(SteamVR_ActionSet.Create<SteamVR_Input_ActionSet_High_GroundControlSet>("/actions/High_GroundControlSet")));
            Valve.VR.SteamVR_Input.actionSets = new Valve.VR.SteamVR_ActionSet[] {
                    SteamVR_Actions.High_GroundControlSet};
        }
    }
}
