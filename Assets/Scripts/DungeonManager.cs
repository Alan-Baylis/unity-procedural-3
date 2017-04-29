using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// The Dungeon Manager manages the dungeon map. Each dungeon is created when the Player enters and is destroyed 
/// when the player leaves. As well the door is destroyed in the world so the player cannot enter a dungeon a second
/// time. 
/// </summary>
public class DungeonManager : MonoBehaviour {

	//
	// This is a lame implementation of a single dungeon prefab. Trying to decide what to do with this because I
	// don't like the two prcedural implementations that I have seen. Will spend more time on this later.
	//

	// Reference to prefabs, technically I could build a bunch of predefined dungeon prefabs which would be easy
	// but a little lame.
	public GameObject[] dungeonTiles;

	// Variables that represent the dungeon game board
	[HideInInspector]
	public Transform dungeonBoard;
	private Dictionary<Vector2, Vector2> dungeonGrid = new Dictionary<Vector2, Vector2>();

	// Represents where the player begins the dungeon at.
	public static Vector2 startPos;

	//Clears our list gridPositions and prepares it to generate a new board.
	private void InitializeList()
	{
		dungeonGrid.Clear();
	}

	// Sets up the dungeon board.
	private void BoardSetup()
	{
		dungeonBoard = new GameObject("Dungeon").transform;
	}

	// Helper method that will select a random prefab and place it in the dungeon at position
	private void PlaceRandomPrefab(GameObject[] prefabs, Vector2 pos)
	{
		// Select a random tile from the prefab array.
		GameObject randomTile = prefabs[Random.Range(0, prefabs.Length)];
		// Create an instance of the prefab
		GameObject instance = Instantiate(randomTile, new Vector3(pos.x, pos.y, 0f), Quaternion.identity) as GameObject;
		instance.transform.SetParent(dungeonBoard);
	}

	// BuildDungeon sets up the dungeon 
	public void BuildDungeon()
	{
		// Initialize the dungeon board
		BoardSetup();

		// Clear out the grid
		InitializeList();

		// Place the dungeon prefab and set the start position.
		startPos = new Vector2(0f, 0f);
		PlaceRandomPrefab(dungeonTiles, startPos);			
	}
	
}
