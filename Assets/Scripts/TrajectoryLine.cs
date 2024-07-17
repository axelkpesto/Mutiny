using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryLine : MonoBehaviour
{
	private LineRenderer lr;
	private int maxLength;
	private void Awake()
	{
		lr = GetComponent<LineRenderer>();
		maxLength = 3;
	}

	public void RenderLine(Vector3 start, Vector3 end)
	{
		lr.positionCount = 2;
		Vector3[] points = new Vector3[2] { start, end };

		Vector3 direction = (end - start).normalized;
		float distance = Mathf.Clamp((Vector3.Distance(start, end)), -maxLength, maxLength);

		lr.SetPositions(new Vector3[2] {start, start+(direction*distance)});
	}

	public void EndLine()
	{
		lr.positionCount = 0;
	}

}
