using UnityEngine;
using UnityEngine.UI;   //Allows us to use UI.


/// <summary>
/// Player.
/// </summary>
public class Player : MovingObject
{

	public static Player instance = null;       // Singleton pattern


	// Character attributes
	public float health = 100;
	public int lineOfSight = 3;

	// Inventory
	public int gold = 0;

	// Survival attributes
	public float starvationRate = 0.5f;

	// Combat attributes
	public int enemyDamage;
	public int wallDamage = 1;                  //How much damage a player does to a wall when chopping it.

	// Treasure attributes
	public int pointsPerFood = 10;              //Number of points to add to player food points when picking up a food object.
	public int pointsPerSoda = 20;              //Number of points to add to player food points when picking up a soda object.

	// UI
	public Text healthText;                       //UI Text to display current player food total.
	public Text goldText;
	private Animator animator;                  //Used to store a reference to the Player's animator component.

	// Player location attributes
	public static Vector2 position;
	private bool isInDungeon;
	private bool isInTransit;


	#region Player Management

	private void Awake ()
	{
		// Implement the Player as a singleton
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		// Set this to not be destroyed
		DontDestroyOnLoad (gameObject);
	}


	//Start overrides the Start function of MovingObject
	protected override void Start ()
	{
		//Get a component reference to the Player's animator component
		animator = GetComponent<Animator> ();
				
		//Set the foodText to reflect the current player food total.
		healthText.text = "Health: " + health;

		goldText.text = "Gold: " + gold;

		// Get the player's current position in space.
		position.x = position.y = 0;

		isInDungeon = false;
		isInTransit = false;

		//Call the Start function of the MovingObject base class.
		base.Start ();
	}

	private void Update ()
	{
		//If it's not the player's turn, exit the function.
		if (!GameManager.instance.playersTurn)
			return;

		int horizontal = 0;     //Used to store the horizontal move direction.
		int vertical = 0;       //Used to store the vertical move direction.

		//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
		horizontal = (int)(Input.GetAxisRaw ("Horizontal"));

		//Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
		vertical = (int)(Input.GetAxisRaw ("Vertical"));

		//Check if moving horizontally, if so set vertical to zero.
		if (horizontal != 0) {
			vertical = 0;
		}

		//Check if we have a non-zero value for horizontal or vertical
		if (horizontal != 0 || vertical != 0) {
			if (!isInTransit) {
				//Call AttemptMove passing in the generic parameter Wall, since that is what Player may interact with if they encounter one (by attacking it)
				//Pass in horizontal and vertical as parameters to specify the direction to move Player in.
				AttemptMove (horizontal, vertical);
			}
		}
	}

	#endregion

	#region Player movement

	//AttemptMove overrides the AttemptMove function in the base class MovingObject
	//AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
	protected override bool AttemptMove (int xDir, int yDir)
	{
		//Every time player moves, subtract from food points total.
		health -= starvationRate;

		//Update food text display to reflect current score.
		healthText.text = "Health: " + health;

		//Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
		bool canMove = base.AttemptMove (xDir, yDir);

		if (!isInDungeon)
		{
			if (canMove)
			{
				position.x += xDir;
				position.y += yDir;
				GameManager.instance.MovePlayer();
			}
		}
		
		//Since the player has moved and lost food points, check if the game has ended.
		CheckIfGameOver ();

		//Set the playersTurn boolean of GameManager to false now that players turn is over.
		GameManager.instance.playersTurn = false;

		return true;
	}


	//OnCantMove overrides the abstract function OnCantMove in MovingObject.
	//It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
	protected override void OnCantMove<T> (T component)
	{
		if (typeof(T) == typeof(Wall)) {
			//Set hitWall to equal the component passed in as a parameter.
			Wall hitWall = component as Wall;

			//Call the DamageWall function of the Wall we are hitting.
			hitWall.DamageWall (wallDamage);

			//Set the attack trigger of the player's animation controller in order to play the player's attack animation.
			animator.SetTrigger ("playerChop");			
		} else if (typeof(T) == typeof(Enemy)) {
			Enemy hitEnemy = component as Enemy;

			hitEnemy.LoseFood (enemyDamage);

			animator.SetTrigger ("playerChop");
		} else if (typeof(T) == typeof(Chest)) {
			Chest hitChest = component as Chest;

			hitChest.Open();
		}

	}

	private void EnterExitDungeon ()
	{
		if (isInDungeon) {
			// Exit
			isInDungeon = false;
			GameManager.instance.ExitDungeon ();
			transform.position = position;
		} else {
			// Enter
			isInDungeon = true;
			GameManager.instance.EnterDungeon ();
			transform.position = DungeonManager.startPos;
		}
	}

	#endregion

	#region Player interaction

	//OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
	private void OnTriggerEnter2D (Collider2D other)
	{
		//Check if the tag of the trigger collided with is Exit.
		if (other.tag == "Portal") {
			isInTransit = true;

			Invoke ("EnterExitDungeon", 0.5f);
			
			Destroy (other.gameObject);

			isInTransit = false;
		}

		//Check if the tag of the trigger collided with is Food.
		else if (other.tag == "Food") {
			//Add pointsPerFood to the players current food total.
			health += pointsPerFood;

			//Update foodText to represent current total and notify player that they gained points
			healthText.text = "+" + pointsPerFood + " Food: " + health;

			//Disable the food object the player collided with.
			other.gameObject.SetActive (false);
		}

		//Check if the tag of the trigger collided with is Soda.
		else if (other.tag == "Soda") {
			//Add pointsPerSoda to players food points total
			health += pointsPerSoda;

			//Update foodText to represent current total and notify player that they gained points
			healthText.text = "+" + pointsPerSoda + " Food: " + health;

			//Disable the soda object the player collided with.
			other.gameObject.SetActive (false);
		}

		else if(other.tag == "Gold")
		{
			gold += 10;

			goldText.text = "Gold: " + gold;

			other.gameObject.SetActive(false);
		}

	}

	#endregion
		
	#region Player combat and survival

	//LoseFood is called when an enemy attacks the player.
	//It takes a parameter loss which specifies how many points to lose.
	public void LoseHealth (float loss)
	{
		//Set the trigger for the player animator to transition to the playerHit animation.
		animator.SetTrigger ("playerHit");

		//Subtract lost food points from the players total.
		health -= loss;

		//Update the food display with the new total.
		healthText.text = "-" + loss + " Health: " + health;

		//Check to see if game has ended.
		CheckIfGameOver ();
	}


	//CheckIfGameOver checks if the player is out of food points and if so, ends the game.
	private void CheckIfGameOver ()
	{
		//Check if food point total is less than or equal to zero.
		if (health <= 0) {
			//Call the GameOver function of GameManager.
			GameManager.instance.GameOver ();
		}
	}

	#endregion
}
