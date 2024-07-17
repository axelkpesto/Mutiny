using Cinemachine;
using UnityEngine;


//https://www.youtube.com/watch?v=pJQndtJ2rk0&ab_channel=CodeMonkey
public class CameraController : MonoBehaviour
{
	[SerializeField] private CinemachineVirtualCamera cmv;
	[SerializeField] private GameObject fallKiller;
	
	private bool movementEnabled;
	private bool zoomEnabled;
	private bool edgeScrollingEnabled;
	private bool camTracking;

	private float cameraSpeed;
	private float zoomSpeed;

	private int edgeScrollSize;
	private Vector2 minPos;
	private Vector2 maxPos;

	private float targetOrthoSize;
	private float orthoSizeMin;
	private float orthoSizeMax;

	private Pirate currentPirate;

	public void Awake()
	{
		movementEnabled = true;
		zoomEnabled = true;
		edgeScrollingEnabled = true;
		camTracking = false;

		cameraSpeed = 10f;
		zoomSpeed = 5f;

		edgeScrollSize = 20;

		targetOrthoSize = 10f;
		orthoSizeMin = 5f;
		orthoSizeMax = 15f;

		//Tweak this
		float xBound = (fallKiller.transform.localScale.x - (Screen.width/100)) / 2;
		//Debug.Log("SCREEN WIDTH: " + Screen.width); //screen is 967 width
		//Debug.Log("FALLKILLER SIZE: " + fallKiller.transform.localScale.x); //goes to -60, +60 unity units, unity unit = 100px
		minPos = new Vector2(-xBound, fallKiller.transform.position.y-fallKiller.transform.localScale.y/2);
		maxPos = new Vector2(xBound, Screen.height);
	}

	public void Update()
	{
		HandleKeyMovement();
		HandleEdgeScrolling();
		HandleZoom();
		HandleCamLock();
		HandleSpaceTracking();
		HandleCamTracking();
	}

	private void HandleKeyMovement()
	{
		if (movementEnabled) 
		{
			Vector3 inputDirection = new Vector3(0, 0, transform.position.z);
			if (Input.GetKey(KeyCode.UpArrow))
			{
				inputDirection.y += 1f;
			}

			if (Input.GetKey(KeyCode.DownArrow))
			{
				inputDirection.y += -1f;
			}

			if (Input.GetKey(KeyCode.LeftArrow))
			{
				inputDirection.x += -1f;
			}

			if (Input.GetKey(KeyCode.RightArrow))
			{
				inputDirection.x += 1f;
			}

			Vector3 moveDirection = transform.forward * inputDirection.y + transform.right * inputDirection.x;
			transform.position = ClampPosition(moveDirection);
		}
	}

	private void HandleEdgeScrolling() 
	{
		if (edgeScrollingEnabled)
		{
			Vector3 inputDirection = new Vector3(0, 0, transform.position.z);
			if (Input.mousePosition.x < edgeScrollSize)
			{
				inputDirection.x += -1f;
			}

			if (Input.mousePosition.y < edgeScrollSize)
			{
				inputDirection.y += -1f;
			}

			if (Input.mousePosition.x > Screen.width - edgeScrollSize)
			{
				inputDirection.x += 1f;
			}


			if (Input.mousePosition.y > Screen.height - edgeScrollSize)
			{
				inputDirection.y += 1f;
			}

			Vector3 moveDirection = transform.forward * inputDirection.y + transform.right * inputDirection.x;
			transform.position = ClampPosition(moveDirection);
		}
	}

	private void HandleZoom()
	{
		if(zoomEnabled)
		{
			if (Input.mouseScrollDelta.y < 0f)
			{
				targetOrthoSize += 5f;
			}

			if (Input.mouseScrollDelta.y > 0f)
			{
				targetOrthoSize += -5f;
			}

			targetOrthoSize = Mathf.Clamp(targetOrthoSize, orthoSizeMin, orthoSizeMax);
			cmv.m_Lens.OrthographicSize = Mathf.Lerp(cmv.m_Lens.OrthographicSize, targetOrthoSize, Time.deltaTime * zoomSpeed);
		}
	}

	private void HandleCamLock()
	{
		if(Input.GetKey(KeyCode.Space) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftControl)))
		{
			camTracking = !camTracking;
		}
	}

	private void HandleSpaceTracking()
	{
		if(Input.GetKey(KeyCode.Space) && !camTracking)
		{
			SetPosition(currentPirate);
		}
	}

	private void HandleCamTracking()
	{
		if(camTracking)
		{
			SetPosition(currentPirate);
		}
	}

	private Vector3 ClampPosition(Vector3 dir) 
	{
		Vector3 newPos = new Vector3(0, 0, transform.position.z);
		newPos.x = Mathf.Clamp(transform.position.x + dir.x * cameraSpeed * Time.deltaTime, minPos.x, maxPos.x);
		newPos.y = Mathf.Clamp(transform.position.y + dir.y * cameraSpeed * Time.deltaTime, minPos.y, maxPos.y);
		return newPos;
	}

	public void SetPosition(Pirate p)
	{
		currentPirate = p;
		if(currentPirate!= null)
		{
			transform.position = Vector3.MoveTowards(transform.position, currentPirate.transform.position, Time.deltaTime * cameraSpeed);
		}
	}
}
