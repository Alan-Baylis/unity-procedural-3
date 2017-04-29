using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The Game Manager handles the operations of the game and acts as a controller for the other game components.
/// </summary>
public class GameManager : MonoBehaviour
{
	#region Public Properties

	//Time to wait before starting level, in seconds.
	public float levelStartDelay = 2f;
	//Delay between each Player turn.
	public float turnDelay = 0.1f;
	//Static instance of GameManager which allows it to be accessed by any other script.
	public static GameManager instance = null;
	//Boolean to check if it's players turn
	[HideInInspector]
	public bool playersTurn = true;

	#endregion

	#region Local Properties

	// Game initialization variables
	//Boolean to check if we're setting up board, prevent Player from moving during setup.
	private bool doingSetup = true;

	// Splash screen variables
	//Image to block out level as levels are being set up, background for levelText.
	private GameObject splashImage;
	//Text to display current level number.
	private Text splashText;

	// References to scripts that are attached to this Game Manager
	private WorldManager worldScript;
	private DungeonManager dungeonScript;

	// Enemy variables
	//List of all Enemy units, used to issue them move commands.
	private List<Enemy> enemies;
	//Boolean to check if enemies are moving.
	private bool enemiesMoving;

	#endregion

	#region Game Management

	void Awake ()
	{
		// Implement this object as a Singleton
		if (instance == null)

			instance = this;

		else if (instance != this)

			Destroy (gameObject);

		//Sets this to not be destroyed when reloading scene
		DontDestroyOnLoad (gameObject);

		//Assign enemies to a new List of Enemy objects.
		enemies = new List<Enemy> ();

		//Get a component reference to the attached BoardManager script
		worldScript = GetComponent<WorldManager> ();
		dungeonScript = GetComponent<DungeonManager> ();

		//Call the InitGame function to build the world
		InitGame ();
	}

	//Update is called every frame.
	void Update()
	{
		//Check that playersTurn or enemiesMoving or doingSetup are not currently true.
		if (playersTurn || enemiesMoving || doingSetup)

			//If any of these are true, return and do not start MoveEnemies.
			return;

		//Start moving enemies.
		StartCoroutine(MoveEnemies());
	}
	
	//Initializes the game
	void InitGame()
	{
		//While doingSetup is true the player can't move, prevent player from moving while title card is up.
		doingSetup = true;

		//Get a reference to our image LevelImage by finding it by name.
		splashImage = GameObject.Find("SplashImage");

		//Get a reference to our text LevelText's text component by finding it by name and calling GetComponent.
		splashText = GameObject.Find("SplashText").GetComponent<Text>();

		//Set the text of levelText to the string "Day" and append the current level number.
		splashText.text = "Tales of Wonder";

		//Set levelImage to active blocking player's view of the game board during setup.
		splashImage.SetActive(true);

		//Call the HideLevelImage function with a delay in seconds of levelStartDelay.
		Invoke("HideLevelImage", levelStartDelay);

		//Clear any Enemy objects in our List to prepare for next level.
		enemies.Clear();

		//Call the SetupScene function of the BoardManager script.
		worldScript.BuildWorld();
	}
	
	//Hides black image used between levels
	void HideLevelImage()
	{
		//Disable the levelImage gameObject.
		splashImage.SetActive(false);

		//Set doingSetup to false allowing player to move again.
		doingSetup = false;
	}

	//GameOver is called when the player reaches 0 food points
	public void GameOver()
	{
		//Set levelText to display number of levels passed and game over message
		splashText.text = "The End";

		//Enable black background image gameObject.
		splashImage.SetActive(true);

		//Disable this GameManager.
		enabled = false;
	}

	#endregion

	#region Enemy Management

	//Call this to add the passed in Enemy to the List of Enemy objects.
	public void AddEnemyToList (Enemy script)
	{
		//Add Enemy to List enemies.
		enemies.Add (script);
	}

	// Call this to remove the passed Enemy from the list of enemy objects.
	public void RemoveEnemyFromList(Enemy script)
	{
		enemies.Remove(script);
	}

	//Coroutine to move enemies in sequence.
	IEnumerator MoveEnemies ()
	{
		//While enemiesMoving is true player is unable to move.
		enemiesMoving = true;

		//Wait for turnDelay seconds, defaults to .1 (100 ms).
		yield return new WaitForSeconds (turnDelay);

		//If there are no enemies spawned (IE in first level):
		if (enemies.Count == 0) {
			//Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
			yield return new WaitForSeconds (turnDelay);
		}

		//Loop through List of Enemy objects.
		for (int i = 0; i < enemies.Count; i++) {
			//Call the MoveEnemy function of Enemy at index i in the enemies List.
			enemies [i].MoveEnemy ();

			//Wait for Enemy's moveTime before moving next Enemy, 
			yield return new WaitForSeconds (enemies [i].moveTime);
		}
		//Once Enemies are done moving, set playersTurn to true so player can move.
		playersTurn = true;

		//Enemies are done moving, set enemiesMoving to false.
		enemiesMoving = false;
	}

	#endregion

	#region Player Management
	// These methods management the interaction between the Player and the game environment.


	// When the Player object moves in the world space, it calls this method to move the player in space and update
	// the line of sight. 
	public void MovePlayer ()
	{
		worldScript.DrawLineOfSight ();
	}

	// When the Player object enters a door in the world space this method will handle the transition.
	public void EnterDungeon ()
	{
		// Make the world space invisible
		worldScript.worldBoard.gameObject.SetActive (false);

		ClearEnemies();
		
		// Build a dungeon map (the Player object will place the Player in the new dungeon).
		dungeonScript.BuildDungeon ();
	}

	// When the Player object exits a dungeon, this method will handle the transition.
	public void ExitDungeon ()
	{
		// Destory the dungeon
		Destroy (dungeonScript.dungeonBoard.gameObject);

		ClearEnemies();

		// Show the world space, the player object will place the Player back in the world.
		worldScript.worldBoard.gameObject.SetActive (true);
	}

	private void ClearEnemies()
	{
		// Clear out all enemies
		for (int i = 0; i < enemies.Count; i++)
		{
			Destroy(enemies[i].gameObject);
		}
		enemies.Clear();
	}
	
	#endregion
}
