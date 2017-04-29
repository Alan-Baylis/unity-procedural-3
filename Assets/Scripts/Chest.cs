using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Chest : MonoBehaviour {

	public Sprite openSprite;

	// Prefabs
	public GameObject[] itemPrefabs;

	private SpriteRenderer spriteRenderer;

	void Awake () {
		spriteRenderer = GetComponent<SpriteRenderer>();	
	}
	
	public void Open()
	{
		spriteRenderer.sprite = openSprite;

		// Spawn things in the chest here
		PlaceRandomPrefab(itemPrefabs, transform.position);				

		gameObject.layer = 10;
		spriteRenderer.sortingLayerName = "Items";
	}

	// Helper method that will select a random prefab and place it in the dungeon at position
	private void PlaceRandomPrefab(GameObject[] prefabs, Vector2 pos)
	{
		// Select a random tile from the prefab array.
		GameObject randomTile = prefabs[Random.Range(0, prefabs.Length)];
		// Create an instance of the prefab
		GameObject instance = Instantiate(randomTile, new Vector3(pos.x, pos.y, 0f), Quaternion.identity) as GameObject;
		instance.transform.SetParent(transform.parent);
	}
}
