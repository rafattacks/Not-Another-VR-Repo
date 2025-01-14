﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    [SerializeField]
    private Vector3 spawnArea = new Vector3(3, 3, 4);

    [SerializeField]
    private int targetsToSpawn = 3;

    public static int totalTargets;

    [SerializeField]
    private GameObject targetPrefab;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(totalTargets == 0)
        {
            SpawnTargets();
        }
    }
    
    void SpawnTargets()
    {
        if(totalTargets == 0)
        {
            float xMin = transform.position.x - spawnArea.x / 2;
            float yMin = transform.position.y - spawnArea.y / 2;
            float zMin = transform.position.z - spawnArea.z / 2;
            float xMax = transform.position.x + spawnArea.x / 2;
            float yMax = transform.position.y + spawnArea.y / 2;
            float zMax = transform.position.z + spawnArea.y / 2;
            
            for(int i = 0; i < targetsToSpawn; i++)
            {
               float xRandom = Random.Range(xMin, xMax);
               float yRandom = Random.Range(yMin, yMax);
               float zRandom = Random.Range(zMin, zMax);

            Vector3 randomPosition = new Vector3(xRandom, yRandom, zRandom);

                Instantiate(targetPrefab, randomPosition, targetPrefab.transform.rotation);

                totalTargets++;
            }
            
        }
        
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, spawnArea);
    }
}
