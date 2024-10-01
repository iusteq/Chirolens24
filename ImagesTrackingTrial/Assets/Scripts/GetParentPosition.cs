using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetParentPosition : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.GetComponentInParent<Transform>().localPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.GetComponentInParent<Transform>().localPosition = this.gameObject.transform.parent.localPosition;
        this.gameObject.GetComponentInParent<Transform>().localRotation = this.gameObject.transform.parent.localRotation * Quaternion.Euler(90, 0, 0);
    }
}
