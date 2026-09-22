using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    private float _spawnTimer;
    private float _spawnInterval = 2f;
    public GameObject enemyPrefab;

    void Update()
    {
        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0 )
        {
            _spawnTimer = _spawnInterval;
        SpawnEnemy();
        }
    }
    private void SpawnEnemy()
    {
        GameObject spawnObject = GameObject.Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        spawnObject.transform.position = transform.position;
    }
}
