using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField] private Slider slider;

    // Start is called before the first frame update
    void Start()
    {
        ChangeVolume();
    }

    // Update is called once per frame
    void Update()
    {
        slider.onValueChanged.AddListener(delegate { ChangeVolume(); });
    }

    private void ChangeVolume()
    {
        AudioListener.volume = slider.value;
    }
}
