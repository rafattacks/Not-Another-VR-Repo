﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    [SerializeField]
    private float amplitude = 1f;

    [SerializeField]
    private float timePeriod = 1f;

    private Vector3 startPosition;

    [SerializeField]
    private float chanceOfMovement = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        //Save the starting position
        startPosition = transform.localPosition;

        //Determine whether or not this target should be moving 
        if(Random.Range(0f, 1f) >= chanceOfMovement)
        {
            this.enabled = false;

        }

    }

    // Update is called once per frame
    void Update()
    {
        float theta = Time.timeSinceLevelLoad / timePeriod;

        float distance = Mathf.Sin(theta) * amplitude;

        Vector3 deltaPosition = new Vector3(distance, 0, 0);

        transform.position = startPosition + deltaPosition;
    }
}
