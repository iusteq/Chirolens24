using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedPosition : MonoBehaviour
{
    public Transform referenceObject;  // Object to keep the canvas relative to (e.g., Image Target)

    void Update()
    {
        if (referenceObject != null)
        {
            // Keep the canvas fixed relative to the reference object
            transform.localPosition = referenceObject.localPosition;
            transform.localRotation = referenceObject.localRotation;
        }
    }
}
