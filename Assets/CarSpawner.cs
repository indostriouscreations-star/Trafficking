using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarSpawner : MonoBehaviour
{
    [Header("Prefaby aut")]
    public GameObject[] carPrefabs;

    [Header("Czas spawnu")]
    public float minSpawnTime = 1f;
    public float maxSpawnTime = 5f;

    [Header("Punkt spawnu")]
    public Transform spawnPoint;

    [Header("Trasy")]
    public Route[] routes;

    void Start()
    {
        StartCoroutine(SpawnCars());
    }

    IEnumerator SpawnCars()
    {
        while (true)
        {
            // Losowy czas oczekiwania
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            // Losowy prefab auta
            int randomCarIndex = Random.Range(0, carPrefabs.Length);

            // Losowa trasa
            int randomRouteIndex = Random.Range(0, routes.Length);

            // Spawn auta
            GameObject car = Instantiate(
                carPrefabs[randomCarIndex],
                spawnPoint.position,
                spawnPoint.rotation
            );

            // Pobranie CarDrive
            CarDrive drive = car.GetComponent<CarDrive>();

            // Ustawienie trasy
            drive.SetRoute(routes[randomRouteIndex].points);
        }
    }
}

[System.Serializable]
public class Route
{
    public Transform[] points;
}