using System;
using System.Collections;
using UnityEngine;

public class PedestrianTrafficLight : MonoBehaviour
{
    private GameObject RedLight;
    private GameObject GereenLight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private IEnumerator RedLightCoroutine()
    {
        yield return new WaitForSeconds(3);
    }
}
