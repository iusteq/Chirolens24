using MagicLeap.Examples;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class GetImageName : MonoBehaviour
{
    public SaveEyeTrackingData eyeTrackingData;
    public GameObject picture;

    private void Awake()
    {
        eyeTrackingData.currentImageName = picture.name;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        eyeTrackingData.currentImageName = picture.name;
    }
}
