using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class Item : MonoBehaviour
{
	[SerializeField] private string type;
	[SerializeField] private float max_damage;
	[SerializeField] private float size;
	[SerializeField] private float exp_radius;
	[SerializeField] private GameObject explosion_effect;

	private string team;
	private bool thrown;
	private LayerMask layerMask;
	private int timeline = 3;

	private float min_damage = 10.0f;
	private Rigidbody2D rb;
	private AudioSource splashSound;
	public void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		rb.gravityScale = 0f;
		layerMask = LayerMask.GetMask("Pirate");
		splashSound = gameObject.GetComponent<AudioSource>();
	}

	public Rigidbody2D GetRigidbody() { return rb; }

	public void Throw(Vector2 force, float power)
	{
		thrown = true;
		rb.gravityScale = 1f;
		rb.AddForce(force * power, ForceMode2D.Impulse);
	}

	public void Explode()
	{
		GameObject _exp = Instantiate(explosion_effect, transform.position, transform.rotation);
		_exp.transform.localScale = Vector3.one * size;
		Knockback();
		Destroy(_exp, timeline);
		Destroy(gameObject);
	}

	private void Knockback()
	{
		RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, exp_radius, Vector2.zero, 0, layerMask);
		foreach (RaycastHit2D hit in hits)
		{
			Pirate current = RaycastHit2DToPirate(hit);
			current.KnockBack((this.transform.position - current.transform.position).normalized, CalculateMagnitude(this.transform.position, current.transform.position)*1/2);
			current.Damage(CalculateMagnitude(this.transform.position, current.transform.position));
		}
	}

	private float CalculateMagnitude(Vector3 originPosition, Vector3 objectPosition)
	{
		float distance = Vector3.Distance(originPosition, objectPosition);
		float magnitude = Mathf.Abs((max_damage / (4 * Mathf.PI * Mathf.Pow((distance - .7f), 3))));
		return (Mathf.Clamp(magnitude, min_damage, max_damage));
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (thrown) 
		{
			if (collision.gameObject.tag.Contains("Pirate"))
			{
				if (CollisionToPirate(collision).GetTeam() != team && CollisionToPirate(collision).IsAlive())
				{
					Explode();
				}
			}
			else if (collision.gameObject.CompareTag("Ground"))
			{
				Explode();
			}
			else if (collision.gameObject.CompareTag("FallKiller"))
			{
				Explode();
				PlaySplashSound();
			}
		}

	}

	private int GetMultiplyer()
	{
		return (type == "Bomb") ? 1 : 2;
	}

	private void PlaySplashSound()
	{
		if(splashSound != null)
		{
			splashSound.Play();
		}
	}

	private Pirate CollisionToPirate(Collider2D collision)
	{
		return (Pirate)collision.gameObject.GetComponent(typeof(Pirate));
	}

	private Pirate RaycastHit2DToPirate(RaycastHit2D collision)
	{
		return (Pirate)collision.collider.gameObject.GetComponent(typeof(Pirate));
	}

	public string GetName() { return type; }

	public void SetTeam(string s) { team = s; }

	public bool IsThrown() { return thrown; }

	public int GetTimeline() { return timeline; }
}
