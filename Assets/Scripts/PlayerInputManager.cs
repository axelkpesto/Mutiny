using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInputManager : MonoBehaviour
{
	[SerializeField] private GameObject pointsPrefab;
	[SerializeField] private CameraController controller;

	private TeamManager teamManager0;
	private TeamManager teamManager1;


	private AudioSource[] gameSoundsArray;
	private Pirate currentPirate;

	private int currentPlayerIndex = 0;
	private int numPoints = 15;
	private int maxForce = 3;
	private float rayLength = 100f;
	private float power = 4f;

	private Vector2 direction;
	private Vector2 force;
	private Vector2 minPower;
	private Vector2 maxPower;
	private LayerMask layerMask;

	private Vector3? startPoint;
	private Vector3 endPoint;

	private Camera cam;
	private GameObject[] points;
	private TrajectoryLine line;

	public bool moveUsed;
	public bool itemUsed;

	private bool online = false;
	public void Startup()
	{
		online = true;
	}
	public void BuildInputManager()
	{
		startPoint = null;
		minPower = new Vector2(-maxForce, -maxForce);
		maxPower = new Vector2(maxForce, maxForce);

		cam = Camera.main;
		line = GetComponent<TrajectoryLine>();
		gameSoundsArray = gameObject.GetComponents<AudioSource>();

		teamManager0 = new TeamManager(0);
		teamManager1 = new TeamManager(1);

		layerMask = LayerMask.GetMask("Pirate");

		online = true;
	}

	public void Update()
	{
		if(online)
		{
			KeyInputs();
			RaycastInputs();
			MouseInputs();
			UpdatePirateLists();
		}

	}

	private bool MovementConditionsMet()
	{
		return (currentPirate != null && currentPirate.GetTeamInt() == GetCurrentPlayer().GetPlayerNum() && GetCurrentPlayer().PirateIsCurrentPirate(currentPirate) && currentPirate.GetState()==2 && !moveUsed);
	}

	private bool ItemConditionsMet()
	{
		return (currentPirate != null && currentPirate.GetItem() != null && currentPirate.GetTeamInt() == GetCurrentPlayer().GetPlayerNum() && GetCurrentPlayer().PirateIsCurrentPirate(currentPirate) && currentPirate.GetState() == 3 && !itemUsed);
	}

	public bool IsCurrentPirateOnCurrentTeam()
	{
		return (currentPirate != null && currentPirate.GetTeamInt() == GetCurrentPlayer().GetPlayerNum());
	}

	public void ResetState()
	{
		if (currentPirate != null) { currentPirate.ResetState(); }
	}

	public Pirate SetSelectionState(Pirate p, int state)
	{
		ResetState();
		KillPoints();
		startPoint = null;
		if (currentPirate != null) { p.SetState(state); }
		return p;
	}

	public void SetSelectionState(int state)
	{
		SetSelectionState(currentPirate, state);
	}

	public void MoveState() { SetSelectionState(2); }

	public void ItemState() { SetSelectionState(3); }

	private void NextPirate()
	{
		GetCurrentPlayer().NextPirate();
		SetCurrentPirate(GetCurrentPlayer().GetCurrentPirate());
	}

	private void PrevPirate()
	{
		GetCurrentPlayer().PrevPirate();
		SetCurrentPirate(GetCurrentPlayer().GetCurrentPirate());
	}

	private void SetCurrentPirate(Pirate p)
	{
		if(currentPirate != p)
		{
			ResetState();
			currentPirate = p;

			if (IsCurrentPirateOnCurrentTeam())
			{
				GetCurrentPlayer().SetCurrentPirate(currentPirate);
			}

			controller.SetPosition(currentPirate);
			SetSelectionState(1);
		}

		if(p != null)
		{
			PlaySelectionSound();
		}
	}

	public void NextTurn()
	{
		currentPlayerIndex = (currentPlayerIndex + 1) % 2;
		SetCurrentPirate(GetCurrentPlayer().GetCurrentPirate());
		ResetBooleans();
		ResetStartVector();
		line.EndLine();
		KillPoints();
	}

	private void ResetBooleans()
	{
		moveUsed = false;
		itemUsed = false;
	}

	private void UpdatePirateLists()
	{
		teamManager0.UpdatePirateStatus();
		teamManager1.UpdatePirateStatus();
	}

	public void RaycastInputs()
	{
		if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
		{
			RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, rayLength, layerMask);
			if (hit.collider != null)
			{
				SetCurrentPirate((Pirate)hit.collider.gameObject.GetComponent(typeof(Pirate)));
			}
		}
	}

	public void MouseInputs()
	{
		if(MovementConditionsMet())
		{
			if (Input.GetMouseButtonDown(0))
			{
				startPoint = currentPirate.transform.position;
				InitPoints();
			}

			if (Input.GetMouseButton(0))
			{
				if (CheckStartVector())
				{
					Vector3 currentPoint = cam.ScreenToWorldPoint(Input.mousePosition);
					GetCurrentPlayer().GetCurrentPirate().SetDirection((currentPoint.x - startPoint.Value.x) < 0);
					line.RenderLine(startPoint.Value, currentPoint);

					direction = (new Vector2(currentPoint.x, currentPoint.y) - (Vector2)GetCurrentPlayer().GetCurrentPirate().transform.position);

					for (int i = 0; i < numPoints; i++)
					{
						points[i].transform.position = PointPosition(i * .1f, new Vector2(currentPoint.x, currentPoint.y));
					}
				}
			}

			if (Input.GetMouseButtonUp(0))
			{
				if (CheckStartVector())
				{
					endPoint = cam.ScreenToWorldPoint(Input.mousePosition);

					direction = (new Vector2(endPoint.x, endPoint.y) - (Vector2)GetCurrentPlayer().GetCurrentPirate().transform.position);
					force = direction.normalized * -PointForce(new Vector2(endPoint.x, endPoint.y));
					GetCurrentPlayer().GetCurrentPirate().Move(force, power);
					line.EndLine();
					KillPoints();
					ResetStartVector();
					moveUsed = true;
				}
			}


		} else if(ItemConditionsMet())
		{
			if (Input.GetMouseButtonDown(0))
			{
				startPoint = currentPirate.transform.position;
				InitPoints();
			}

			if (Input.GetMouseButton(0))
			{
				if (CheckStartVector())
				{
					Vector3 currentPoint = cam.ScreenToWorldPoint(Input.mousePosition);
					GetCurrentPlayer().GetCurrentPirate().SetDirection((currentPoint.x - startPoint.Value.x) < 0);
					line.RenderLine(startPoint.Value, currentPoint);

					direction = (new Vector2(currentPoint.x, currentPoint.y) - (Vector2)GetCurrentPlayer().GetCurrentPirate().transform.position);

					for (int i = 0; i < numPoints; i++)
					{
						points[i].transform.position = PointPosition(i * .1f, new Vector2(currentPoint.x, currentPoint.y));
					}
				}
			}

			if (Input.GetMouseButtonUp(0))
			{
				if (CheckStartVector())
				{
					endPoint = cam.ScreenToWorldPoint(Input.mousePosition);

					direction = (new Vector2(endPoint.x, endPoint.y) - (Vector2)GetCurrentPlayer().GetCurrentPirate().transform.position);
					force = direction.normalized * -PointForce(new Vector2(endPoint.x, endPoint.y));
					GetCurrentPlayer().GetCurrentPirate().Throw(force, power);
					line.EndLine();
					KillPoints();
					ResetStartVector();
					itemUsed = true;
				}
			}
		}
	}

	public void SetItem(string s)
	{
		currentPirate.GetItem(s);
	}

	private bool CheckStartVector()
	{
		return (startPoint.HasValue);
	}

	private void ResetStartVector()
	{
		startPoint = null;
	}

	public void KeyInputs()
	{
		if (Input.GetKeyDown(KeyCode.W))
		{
			NextPirate();
		}

		if (Input.GetKeyDown(KeyCode.A))
		{
			PrevPirate();
		}

		if (Input.GetKeyDown(KeyCode.S))
		{
			PrevPirate();
		}

		if (Input.GetKeyDown(KeyCode.D))
		{
			NextPirate();
		}

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (currentPirate.GetState()>1) 
			{
				SetSelectionState(1);
			} else if(currentPirate.GetState()==1)
			{
				SetCurrentPirate(null);
			}
		}
	}

	public float GetTotalValue(TeamManager tm) { return tm.GetTotalWorth(); }

	public TeamManager GetCurrentPlayer() { return ((currentPlayerIndex % 2 == 0) ? teamManager0 : teamManager1); }

	private TeamManager GetOppositePlayer() { return ((currentPlayerIndex % 2 == 0) ? teamManager1 : teamManager0); }

	public TeamManager[] GetTeamManagers() { return (new TeamManager[] { teamManager0, teamManager1 }); }

	public TeamManager GetTeamManager(int playerNum) { return ((playerNum == 0) ? teamManager0 : teamManager1); }

	private void PlaySelectionSound()
	{
		if (gameSoundsArray != null && gameSoundsArray.Length > 0)
		{
			gameSoundsArray[Random.Range(0,gameSoundsArray.Length-1)].Play();
		}
	}

	public Pirate GetCurrentPirateGlobal()
	{
		return currentPirate;
	}

	private void InitPoints()
	{
		points = new GameObject[numPoints];
		for (int i = 0; i < numPoints; i++)
		{
			Instantiate(pointsPrefab, GetCurrentPlayer().GetCurrentPirate().transform.position, Quaternion.identity);
		}

		points = GameObject.FindGameObjectsWithTag("Point");
	}

	private void KillPoints()
	{
		if(points != null)
		{
			for (int i = 0; i < points.Length; i++)
			{
				if (points[i] != null)
				{
					Destroy(points[i]);
				}
			}
			points = new GameObject[numPoints];
		}

		if(line != null)
		{
			line.EndLine();
		}
	}

	private Vector2 PointPosition(float t, Vector2 point)
	{
		Vector2 currentPointPos = ((Vector2)GetCurrentPlayer().GetCurrentPirate().transform.position) + (direction.normalized * -PointForce(point) * power * t) + (0.5f * Physics2D.gravity * t * t);
		return currentPointPos;
	}

	private float PointForce(Vector2 point)
	{
		float pointForceX = Mathf.Clamp(startPoint.Value.x - point.x, minPower.x, maxPower.x);
		float pointForceY = Mathf.Clamp(startPoint.Value.y - point.y, minPower.y, maxPower.y);
		float pointForce = Mathf.Sqrt(Mathf.Pow(pointForceX, 2) + Mathf.Pow(pointForceY, 2));
		return pointForce;
	}
}
