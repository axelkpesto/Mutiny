using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    private string item;
    private int quantity;
	private Rigidbody2D rb;
	private AudioSource coinSound;
    private void Start()
    {	
		rb = GetComponent<Rigidbody2D>();
        item = (Random.Range(0, 100) < 80) ? "Bomb" : "Large Bomb";
        quantity = (Random.Range(0, 100) < 90) ? 1 : 2;
		coinSound = gameObject.GetComponent<AudioSource>();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag.Contains("Pirate"))
		{
			Pirate current = CollisionToPirate(collision);
			if (current != null)
			{
				current.AddItems(item, quantity);
				coinSound.Play();
				Destroy(gameObject);
			}
		} else if(collision.gameObject.CompareTag("FallKiller"))
		{
			Destroy(gameObject);
		} else if (collision.gameObject.CompareTag("Ground"))
		{
			rb.gravityScale = 0f;
			rb.velocity = Vector3.zero;
		}
	}
	private Pirate CollisionToPirate(Collider2D collision)
	{
		return (Pirate)collision.gameObject.GetComponent(typeof(Pirate));
	}
	
	private void PlayCoinSound()
	{
		if (coinSound != null)
		{
			coinSound.Play();
		}

	}

}
