using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnsureIsChild : MonoBehaviour
{
    public Transform imageTarget;
    public Transform childObject;

    void Start()
    {
        if (imageTarget == null || childObject == null)
        {
            Debug.LogError("ImageTarget or ChildObject is not assigned.");
            return;
        }

        // Ensure the child is a child of the image target
        if (!childObject.IsChildOf(imageTarget))
        {
            childObject.SetParent(imageTarget);
            Debug.Log("ChildObject has been reparented to ImageTarget.");
        }

        // Optional: Reset local position and rotation if needed
        childObject.localPosition = Vector3.zero + new Vector3(0, (float)0.2, 0);
        childObject.localRotation = Quaternion.identity * Quaternion.Euler(90, 0, 0);
    }

    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, 0.1f);
    //}
}
