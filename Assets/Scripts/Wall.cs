using UnityEngine;
using Random = UnityEngine.Random;

public class Wall : MonoBehaviour
{
	public Sprite dmgSprite;                    //Alternate sprite to display after Wall has been attacked by player.
	public int hp = 4;                          //hit points for the wall.

	// Prefab references
	public GameObject[] foodTiles;

	private SpriteRenderer spriteRenderer;      //Store a component reference to the attached SpriteRenderer.


	void Awake()
	{
		//Get a component reference to the SpriteRenderer.
		spriteRenderer = GetComponent<SpriteRenderer>();
	}


	//DamageWall is called when the player attacks a wall.
	public void DamageWall(int loss)
	{
		//Set spriteRenderer to the damaged wall sprite.
		spriteRenderer.sprite = dmgSprite;

		//Subtract loss from hit point total.
		hp -= loss;

		//If hit points are less than or equal to zero:
		if(hp <= 0)
		{
			// 1 in 5 chance that there is food in this wall.
			if (Random.Range(0, 2) == 1)
			{
				//GameObject toInstantiate = foodTiles[Random.Range(0, foodTiles.Length)];
				//GameObject instance = Instantiate(toInstantiate, new Vector3(transform.position.x, transform.position.y, 0f), Quaternion.identity) as GameObject;
				//instance.transform.SetParent(transform.parent);
				PlaceRandomPrefab(foodTiles, transform.position);
			}
		
			//Disable the gameObject.
			gameObject.SetActive(false);
		}
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