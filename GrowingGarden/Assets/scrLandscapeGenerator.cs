using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrLandscapeGenerator : MonoBehaviour
{
	public GameObject debugPrefab;
	
	const float xSpacing = 20.0f;	// The regular spacing of points along the x axis.
	const float yHighest = 10.0f;	// The highest value on the y axis that a point can take. The lowest ground value is 0. Below this level, lakes will form..

	const int numIntermediateVertices = 50;	// The number of intermediate vertices between each point. Higher = smoother terrain.
	const float vertexGap = xSpacing / numIntermediateVertices;	// The x gap between intermediate vertices.

	LinkedList<Vector2> visiblePoints = new LinkedList<Vector2>();	// The currently visible points. This should include the one point before the left of the camera, the one point after the right of the camera, and all points in between.
	LinkedList<Vector2>.Enumerator firstPoint;	// The first point after illegal points are removed.
	LinkedList<Vector2>.Enumerator lastPoint;	// The last point after illegal points are removed.
	bool allPointsWiped;	// Whether or not the list of points has no recurring members after illegal points are removed.

	LinkedList<Vector2> visibleVertices = new LinkedList<Vector2>();	// All vertices visible on screen.


	/*
	 * ORDER OF EXECUTION
	 * 
	 * 1) Remove points that were previously in the camera view but aren't any more.
	 * 2) Remove vertices that were previously in the camera view but aren't any more.
	 * 3) Get the x values at the start and end of the resulting list of points.
	 * 3) Add new points that have entered the camera view.
	 * 4) Set the firstPoint and lastPoint enumerators to the positions of the first and last element before points were added.
	 * 5) If there are points to the left of the firstPoint, generateVerticesLeft.
	 * 6) If there are points to the right of the lastPoint, generateVerticesRight.
	 *
	 */

	
	/// <summary>
	/// Populates the points list with the points between (and including) the point to the left of the camera frustum and the point to the right of the camera frustum.
	/// </summary>
	void populatePointsList()
	{
		// TODO make points based on camera position
		// TODO make y value based on x value and z value so always generated the same when degenerated and regenerated when the view comes back

		// STEP 1:
		// TODO

		// STEP 2:
		// TODO

		// STEP 3:
		float oldFirstX = 0.0f;
		float oldLastX = 0.0f;

		if (visiblePoints.Count != 0)
		{
			oldFirstX = visiblePoints.First.Value.x;
			oldLastX = visiblePoints.Last.Value.x;

			allPointsWiped = false;
		}
		else
		{
			allPointsWiped = true;
		}

		// STEP 4:

		// THIS PART IS DEBUG  =================================================================================
		visiblePoints.AddFirst(new Vector2(0.0f, 0.0f));

		for (float x = 0.0f + xSpacing; x < 100.0f; x += xSpacing)
		{
			// Add the new last node.
			visiblePoints.AddLast (new Vector2(x, Random.Range (0.0f, yHighest)));
		}
		// THIS PART IS DEBUG	=================================================================================

		// STEP 5:

		// Get the list's enumerators.
		firstPoint = visiblePoints.GetEnumerator();
		lastPoint = visiblePoints.GetEnumerator();

		// Move the enumerators to the start of the list.
		firstPoint.MoveNext();
		lastPoint.MoveNext ();

		if (allPointsWiped)
		{
			// If all points are wiped, create all vertices from left to right with the lastPoint starting at the firstPoint.
			generateVerticesRight();
		}
		else
		{
			// Move the first and last enumerators to the old first and last x positions.
			while (firstPoint.Current.x != oldFirstX)
				firstPoint.MoveNext();

			while (lastPoint.Current.x != oldLastX)
				lastPoint.MoveNext(); 

			// If the old first point's x is not the same as the new first point's x, generate vertices from the old first point to the left. 
			if (firstPoint.Current.x != visiblePoints.First.Value.x)
				generateVerticesLeft();

			// If the old last point's x is not the same as the new last point's x, generate vertices from the old last point to the right. 
			if (oldLastX != visiblePoints.Last.Value.x)
				generateVerticesRight();
		}
	}

	/// <summary>
	/// Generates the vertices between the old first and new first points in the points list.
	/// </summary>
	void generateVerticesLeft()
	{
		Debug.Log ("Generating vertices leftwards.");
	}

	/// <summary>
	/// Generates the vertices between the old last and new last points in the points list.
	/// </summary>
	void generateVerticesRight()
	{
		Debug.Log ("Generating vertices rightwards.");

		Vector2 left = lastPoint.Current;
		Vector2 right;

		// If the camera has moved so far there are no old points, set the last point to the first point of the new points.
		if (allPointsWiped)
		{
			// There are no vertices in the list, so add the first one.
			visibleVertices.AddLast(left);
		}

		// Keep moving right until there are no new points.
		while (lastPoint.MoveNext())
		{
			// Set the right value to the current point.
			right = lastPoint.Current;

			// Create the intermediate vertices from left to right, with their y calculated from the smoothstep function between the left and right point.
			for (int i = 1; i < numIntermediateVertices; ++i)
				visibleVertices.AddLast (new Vector2(left.x + i * vertexGap, Mathf.SmoothStep (left.y, right.y, (float)i / numIntermediateVertices)));

			// Add the right point.
			visibleVertices.AddLast(right);

			// Assign the right point to the left variable for the next last point.
			left = right;
		}
	}

	// Use this for initialization
	void Start ()
	{
		populatePointsList();

		// DEBUG
		debugPrefab.transform.localScale = 0.25f * Vector3.one;
		LinkedList<Vector2>.Enumerator e = visibleVertices.GetEnumerator();
		while (e.MoveNext())
		{
			Instantiate(debugPrefab, new Vector3(e.Current.x, e.Current.y), Quaternion.identity);
		}

		debugPrefab.transform.localScale = 0.5f * Vector3.one;

		LinkedList<Vector2>.Enumerator e2 = visiblePoints.GetEnumerator();
		while (e2.MoveNext())
		{
			Instantiate(debugPrefab, new Vector3(e2.Current.x, e2.Current.y), Quaternion.identity);
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}


}
