using MagicLeap.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartEyeTrackingWhenImageDetected : MonoBehaviour
{
    public SaveEyeTrackingData eyeTrackingData;
    //public GetImageName getName;

    private void Start()
    {
        //eyeTrackingData.currentImageName = getName.picture.name;
        eyeTrackingData.enabled = false;
    }
    public void StartEyeTrackingScript()
    {
        if (eyeTrackingData != null)
        {
            eyeTrackingData.enabled = true;
        }
    }

    public void StopEyeTracking()
    {
        // Disable ScriptB
        if (eyeTrackingData != null)
        {
            eyeTrackingData.enabled = false;
        }
    }
}
