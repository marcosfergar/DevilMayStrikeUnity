using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;

    [Header("Configuracion de Oleadas")]
    public int currentWave = 0;
    public int enemiesPerWaveBase = 3;
    public float timeBetweenWaves = 3f;

    // Volvemos a una lista controlada internamente
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isWaveActive = false;

    void Start()
    {
        StartCoroutine(StartNextWaveAfterDelay());
    }

    void Update()
    {
        if (isWaveActive)
        {
            // El propio Manager revisa cada fotograma si los clones de la lista se han borrado de la jerarquía
            activeEnemies.RemoveAll(enemy => enemy == null);

            // Si la lista se queda completamente vacía, pasamos de ronda automáticamente
            if (activeEnemies.Count == 0)
            {
                EndWave();
            }
        }
    }

    IEnumerator StartNextWaveAfterDelay()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        
        currentWave++;
        Debug.Log("¡PREPÁRATE! COMIENZA LA OLEADA: " + currentWave);

        int enemiesToSpawn = enemiesPerWaveBase + (currentWave * 2);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(0.3f);
        }

        isWaveActive = true;
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length == 0) return;

        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform selectedPoint = spawnPoints[randomIndex];

        // Guardamos el clon recién creado en una variable
        GameObject newEnemy = Instantiate(enemyPrefab, selectedPoint.position, selectedPoint.rotation);
        
        // Lo metemos directamente en la lista interna
        activeEnemies.Add(newEnemy);
    }

    void EndWave()
    {
        isWaveActive = false;
        Debug.Log("Oleada completada. Un momento de respiro...");
        StartCoroutine(StartNextWaveAfterDelay());
    }
}