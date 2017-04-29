using System;
using System.Collections.Generic;       //Allows us to use Lists.
using UnityEngine;
using Random = UnityEngine.Random;      //Tells Random to use the Unity Engine random number generator.


/// <summary>
/// The World Manager manages the world map. This implementation is a procedural world generator. The world is 
/// generated as the Player's line of sight reveals the content of the game world.
/// </summary>
public class WorldManager : MonoBehaviour
{
	// References to prefabs
	public GameObject[] floorTiles;                                 //Array of floor prefabs.
	public GameObject[] wallTiles;                                  //Array of wall prefabs.
	public GameObject[] foodTiles;                                  //Array of food prefabs.
	public GameObject[] enemyTiles;                                 //Array of enemy prefabs.
	public GameObject[] doorTiles;
	public GameObject[] chestTiles;

	// Variables that represent the world game board.
	[HideInInspector]public Transform worldBoard;                                  
	private Dictionary<Vector2, Vector2> worldGrid = new Dictionary<Vector2, Vector2>();

	//Clears our list gridPositions and prepares it to generate a new board.
	protected void InitialiseList()
	{
		//Clear our list gridPositions.
		worldGrid.Clear();		
	}


	//Sets up the world board.
	protected void BoardSetup()
	{
		//Instantiate Board and set boardHolder to its transform.
		worldBoard = new GameObject("World").transform;		
	}

	// This method builds the Player's line of sight by building repeating circles from the Player
	// out until the Player's line of sight distance.
	public void DrawLineOfSight()
	{
		// This is a cheap hack to fill in Bresenham's circle but it is much easier than the 
		// alternatives that are out there.
		for (int r = 0; r < Player.instance.lineOfSight; r++)
		{
			DrawCirlce(Convert.ToInt32(Player.position.x), Convert.ToInt32(Player.position.y), r);
		}
	}

	// Implementation of Bresenham's circle algorithm to build the Player's line of sight. 
	// Look it up on Wikipedia if you want to know the details.
	private void DrawCirlce(int x0, int y0, int radius)
	{
		int x = radius;
		int y = 0;
		int err = 0;

		while (x >= y)
		{
			PlaceGroundTile(x0 + x, y0 + y);
			PlaceGroundTile(x0 + y, y0 + x);
			PlaceGroundTile(x0 - y, y0 + x);
			PlaceGroundTile(x0 - x, y0 + y);
			PlaceGroundTile(x0 - x, y0 - y);
			PlaceGroundTile(x0 - y, y0 - x);
			PlaceGroundTile(x0 + y, y0 - x);
			PlaceGroundTile(x0 + x, y0 - y);

			if (err <= 0)
			{
				y += 1;
				err += 2 * y + 1;
			}
			if (err > 0)
			{
				x -= 1;
				err -= 2 * x + 1;
			}
		}
	}

	//TODO: Need a better name, this method does more than just placing ground tiles
	private void PlaceGroundTile(int x, int y)
	{
		// Could have taken in a Vector2 but it is easier to read the circle algorothm by
		// putting this here.
		Vector2 pos = new Vector2 (x, y);

		if (!worldGrid.ContainsKey(pos))
		{
			//Select a random piece of ground to place at this location in world space
			PlaceRandomPrefab(floorTiles, pos);
			worldGrid.Add(pos, pos);

			// Randomly place other objects in world space
			PlaceRandomObjects(pos);
		}
	}


	// This method is a helper that will randomly place other objects in world space.
	private void PlaceRandomObjects(Vector2 pos)
	{
		//TODO: Change all of these magic numbers and make them configurable in the inspector

		// Don't put an object in the same space that the player occupies
		if (!(pos.x == Convert.ToInt32(Player.position.x) && pos.y == Convert.ToInt32(Player.position.y)))
		{
			// Walls
			if (Random.Range(0, 3) == 1)
			{
				PlaceRandomPrefab (wallTiles, pos);
			}
			// Doors
			else if (Random.Range(0, 20) == 1)
			{
				PlaceRandomPrefab (doorTiles, pos);
			}
			// Chest
			else if(Random.Range(0, 20) == 1)
			{
				PlaceRandomPrefab(chestTiles, pos);
			}
			// Enemies
			else if (Random.Range(0, 50) == 1)
			{
				PlaceRandomPrefab(enemyTiles, pos);
			}
		}
	}
		
	// Helper method that will select a prefab from an array and then place that tile at the provided location
	// in the world space.
	private void PlaceRandomPrefab(GameObject[] prefabs, Vector2 pos)
	{
		// Select a random tile from the prefab array.
		GameObject randomTile = prefabs[Random.Range(0, prefabs.Length)];
		// Create an instance of the prefab
		GameObject instance = Instantiate(randomTile, new Vector3(pos.x, pos.y, 0f), Quaternion.identity) as GameObject;
		instance.transform.SetParent(worldBoard);
	}

	//SetupScene initializes our level and calls the previous functions to lay out the game board
	public void BuildWorld()
	{
		//Creates the outer walls and floor.
		BoardSetup();

		//Reset our list of gridpositions.
		InitialiseList();

		// This is a cheap hack for filing the circle, but it works.
		DrawLineOfSight();		
	}
}
