using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomHandlerImageChild : DefaultObserverEventHandler
{
    public GameObject trackedContent;  // The content you want to track

    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();
        if (trackedContent != null)
        {
            // Reposition the tracked content manually on tracking found
            trackedContent.transform.localPosition = Vector3.zero;
            trackedContent.transform.localRotation = Quaternion.Euler(90, 0, 0);
            trackedContent.SetActive(true);
        }
    }

    protected override void OnTrackingLost()
    {
        base.OnTrackingLost();
        if (trackedContent != null)
        {
            // Optionally, disable the content on tracking lost
            trackedContent.SetActive(false);
        }
    }
}
