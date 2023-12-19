using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps; // Don't forget to add this namespace

public class LevelGenerator : MonoBehaviour {
    [Header("Level Sections")]
    public List<GameObject> levelSections;

    [Header("Generation Settings")]
    public int numberOfSections;

    private int sectionsSpawned = 0;
    private int lastSpawnedSectionIndex = -1;

    void Start() {
        for (int i = 0; i < numberOfSections; i++) {
            GenerateSection();
        }
    }

    private void GenerateSection() {
        int randomIndex;
        do {
            randomIndex = Random.Range(0, levelSections.Count);
        } while (randomIndex == lastSpawnedSectionIndex);

        lastSpawnedSectionIndex = randomIndex;
        GameObject sectionPrefab = levelSections[randomIndex];

        // Calculate the section width using the prefab's tilemap component's cellBounds property
        Tilemap tilemap = sectionPrefab.GetComponentInChildren<Tilemap>();
        float sectionWidth = tilemap.cellBounds.size.x * tilemap.cellSize.x;

        Vector3 spawnPosition = new Vector3(transform.position.x + sectionWidth * sectionsSpawned, transform.position.y, 0);
        Instantiate(sectionPrefab, spawnPosition, Quaternion.identity, transform);

        sectionsSpawned++;
    }
}
