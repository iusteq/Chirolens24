using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextVisibility : MonoBehaviour
{
    [SerializeField] GameObject canvas;

    void Start()
    {
        canvas.SetActive(false);
    }

    public void SetTextVisibilityTrue()
    {
        canvas.SetActive(true);
    }

    public void SetTextVisibilityFalse()
    {
        canvas.SetActive(false);
    }
}
