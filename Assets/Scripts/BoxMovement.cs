using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoxMovement : MonoBehaviour
{
    public int forkliftLayer = 3;
    public BoxCollider attachCollider;

    private GameObject _fork;
    private Rigidbody _rb;
    
    void Start()
    {
        _fork = GameObject.Find("Fork");
        _rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == forkliftLayer)
        {
            transform.SetParent(_fork.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == forkliftLayer)
        {
            transform.SetParent(null);
        }
    }
}
