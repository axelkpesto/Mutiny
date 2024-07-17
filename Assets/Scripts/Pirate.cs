using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*8Bit Pirate Tileset from https://0x72.itch.io/16x16-pirates-tileset */
public class Pirate : MonoBehaviour
{
	//INSTANTIATION
	//Sprite Array
	[SerializeField] private Sprite[] spriteArray;

	//Armory of Items
	private Armory armory;

	//Movement and Selection
	private int selectionState;
	private bool movementLocked;
	private bool directionLocked;

	//Component References
	private Rigidbody2D rb;
	private SpriteRenderer spriteRenderer;
	private Outline outline;
	private AudioSource[] pirateSounds;

	//Item
	private Item currentItem;

	//Pirate Object Variables
	private float xScale;
	private float yScale;

	private int pirateType;
	private float health;
	private float maxHealth;

	private bool alive;
	private string team;
	private Dictionary<string, int> items;


	//On Instantiation, Call
	void Awake()
	{
		BuildPirate();
	}

	//SETUP
	//Set up pirate
	private void BuildPirate()
	{

		//Set up references to components
		rb = GetComponent<Rigidbody2D>();
		spriteRenderer = rb.GetComponent<SpriteRenderer>();
		armory =(Armory) (GameObject.FindGameObjectWithTag("Armory")).GetComponent(typeof(Armory));
		pirateSounds = gameObject.GetComponents<AudioSource>();
		SetupOutline();

		//Movement conditions
		movementLocked = false;
		directionLocked = false;
		selectionState = 0;
		rb.freezeRotation = true;

		alive = true;
		maxHealth = 100;
		health = maxHealth;

		xScale = -transform.localScale.x;
		yScale = transform.localScale.y;

		team = (gameObject.name.Contains("Blue") ? "Blue" : "Red");
		pirateType = (gameObject.name.Contains("A") ? 0 : 1);
		items = CreateItems();

		rb.AddForce(Vector2.up, ForceMode2D.Impulse);
	}

	//Setup outline
	private void SetupOutline()
	{
		outline = gameObject.AddComponent<Outline>();
		outline.OutlineMode = Outline.Mode.OutlineAll;
		outline.OutlineColor = Color.yellow;
		outline.OutlineWidth = 5f;
	}


	//UPDATES
	//Update
	public void Update()
	{
		SetOutline();
	}

	//Fixed Update
	public void FixedUpdate()
	{
		if (!rb.isKinematic && rb.velocity.magnitude == 0)
		{
			rb.isKinematic = true;
		}
	}



	//STATE CONTROL
	//Change Selection State
	public void SetState(int i)
	{
		selectionState = i;
	}

	//Reset Selection State
	public void ResetState()
	{
		selectionState = 0;
		if (currentItem != null)
		{
			//If Item has been thrown, destroy it after it would expire
			//Else Destroy immidiately
			if (currentItem.IsThrown())
			{
				Destroy(currentItem.gameObject, currentItem.GetTimeline());
			}
			else
			{
				Destroy(currentItem.gameObject);
			}
		}
	}



	//MOVEMENT
	//Move Pirate Depending on Direction and Power
	public void Move(Vector2 force, float power)
	{
		if (alive && !movementLocked && (rb.velocity == Vector2.zero) && selectionState == 2)
		{
			rb.isKinematic = false;
			rb.AddForce(force * power, ForceMode2D.Impulse);
		}
	}

	//Throw Item Depending on Direction and Power
	public void Throw(Vector2 force, float power)
	{
		if(currentItem!=null && selectionState == 3 && (rb.velocity == Vector2.zero))
		{
			currentItem.Throw(force, power);
			if(items[currentItem.GetName()]!=0)
			{
				items[currentItem.GetName()] = items[currentItem.GetName()] - 1;
			}
		}
	}

	//Knockback Pirate depending on Direction and Power
	public void KnockBack(Vector2 force, float power)
	{
		if(alive)
		{
			rb.isKinematic = false;
			rb.AddForce(-force * power, ForceMode2D.Impulse); //Inverted to simulate 'knockBACK'
		}
	}

	//Flips pirate by current direction
	public void SetDirection(bool rightFacing)
	{
		if (!directionLocked)
		{
			transform.localScale = GetDirection(rightFacing);
		}
	}

	//Gets direction depending on rightFacing variable
	public Vector3 GetDirection(bool rightFacing)
	{
		return ((rightFacing ? new Vector3(xScale, yScale, 1) : new Vector3(-xScale, yScale, 1)));
	}


	//Deal Damage to Pirate. Contains kill conditions.
	public void Damage(float f)
	{
		if(alive)
		{
			health = health - f;
			
			if(health <= 0)
			{
				KillPirate();
			}
		}
	}






	//OUTLINES
	//Sets outline by selection state
	private void SetOutline()
	{
		outline.enabled = ((selectionState == 0) ? false : true);
	}



	//ITEMS
	//Adds items to dictionary
	public void AddItems(string s, int quantity)
	{
		if (items.ContainsKey(s))
		{
			items[s] = items[s] + quantity;
		} else
		{
			items.Add(s, quantity);
		}
	}

	//Creates items by the type
	private Dictionary<string, int> CreateItems()
	{
		return CreateItemsFromPirateType(pirateType);
	}

	//CreateItem with conditional
	private Dictionary<string, int> CreateItemsFromPirateType(int pirateType)
	{
		return ((pirateType % 2 == 0) ? CreateItemsPirateType0() : CreateItemsPirateType1());
	}

	//Create Items by Type
	private Dictionary<string, int> CreateItemsPirateType0()
	{
		Dictionary<string, int> itemDict = new Dictionary<string, int>();
		itemDict.Add("Bomb", 10);
		return itemDict;
	}

	private Dictionary<string, int> CreateItemsPirateType1()
	{
		Dictionary<string, int> itemDict = new Dictionary<string, int>();
		itemDict.Add("Bomb", 10);
		itemDict.Add("Large Bomb", 2);
		return itemDict;
	}

	
	private string DictionaryToString(Dictionary<string, int> dictionary)
	{
		string dictionaryString = "{\n";
		foreach (KeyValuePair<string, int> keyValues in dictionary)
		{
			dictionaryString += keyValues.Key + " : " + keyValues.Value + "\n";
		}
		return dictionaryString + " }";
	}

	//Set CurrentItem from String
	public void GetItem(string s)
	{
		if (items[s] != 0 && (rb.velocity == Vector2.zero))
		{
			Vector3 itemPosition = new Vector3(transform.position.x, transform.position.y + .8f, transform.position.z);
			currentItem = (Item)Instantiate(armory.GetPrefab(s), itemPosition, transform.localRotation).GetComponent(typeof(Item));
			currentItem.SetTeam(team);
		}
	}

	//FALLKILLER + SOUNDS
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.CompareTag("FallKiller"))
		{
			PlaySplash();
			KillPirate();
		}
	}
	
	//Play Splash sound effect
	private void PlaySplash()
	{
		if (pirateSounds != null && pirateSounds.Length >= 0 && alive)
		{
			pirateSounds[0].Play();
		}
	}

	//KILLING PIRATE
	public void KillPirate()
	{
		health = 0;
		alive = false;
		movementLocked = true;
		directionLocked = true;
		rb.isKinematic = true;
		spriteRenderer.sprite = spriteArray[1];
	}

	
	//GETTERS AND TOSTRINGS
	override
	public string ToString()
	{
		return ($"Team: {team}, pirateType: {pirateType}, X: {transform.position.x}, Y: {transform.position.y}, Health: {health}, Alive: {alive}, \nItems: {ItemString()}");
	}

	private string ItemString() { return DictionaryToString(items); }
	public int GetPirateType() { return pirateType; }
	public bool IsAlive() { return alive; }
	public Dictionary<string, int> GetItems() { return items; }
	public float GetHealth() { return health; }
	public string GetTeam() { return team; }
	public int GetTeamInt() { return ((team == "Blue") ? 0 : 1); }
	public float GetWorth() { return health * (Mathf.Abs(pirateType) + 1); }
	public float GetX() { return transform.position.x; }
	public float GetY() { return transform.position.y; }
	public int GetState() { return selectionState; }
	public Rigidbody2D GetRigidbody() { return rb; }

	public Item GetItem() { return currentItem; }
}
