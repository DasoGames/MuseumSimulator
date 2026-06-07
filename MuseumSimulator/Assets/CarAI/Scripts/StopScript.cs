using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopScript : MonoBehaviour
{
    [Tooltip("If true the car that touches the trigger will stop.")]
    public bool stop = true;
    public int priority = 0;

    [Header("?? 물리 정지선 장벽 오브젝트")]
    public GameObject StopperCollider;

    public void CanGo(bool isRedLight)
    {
        if (StopperCollider != null)
        {
            StopperCollider.SetActive(isRedLight);
        }
    }
}