﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrLandscapeGenerator : MonoBehaviour
{
	public GameObject debugPrefab;
	
	const int xSpacing = 10;	// The regular spacing of points along the x axis. An integer because integers are pretty and simple.
	const float yHighest = 10.0f;	// The highest value on the y axis that a point can take. The lowest ground value is 0. Below this level, lakes will form..

	const int numIntermediateVertices = 50;	// The number of intermediate vertices between each point. Higher = smoother terrain.
	const float vertexGap = (float)xSpacing / numIntermediateVertices;	// The x gap between intermediate vertices.

	LinkedList<Vector2> visiblePoints = new LinkedList<Vector2>();	// The currently visible points. This should include the one point before the left of the camera, the one point after the right of the camera, and all points in between.
	LinkedList<Vector2>.Enumerator firstPoint;	// The first point after illegal points are removed.
	LinkedList<Vector2>.Enumerator lastPoint;	// The last point after illegal points are removed.
	bool allPointsWiped;	// Whether or not the list of points has no recurring members after illegal points are removed.

	LinkedList<Vector2> visibleVertices = new LinkedList<Vector2>();	// All vertices visible on screen.

	float camLeft;	// The left x of the camera.
	float camRight;	// The right x of the camera.


	/*
	 * ORDER OF EXECUTION
	 * 
	 * 1) Remove points that were previously in the camera view but aren't any more.
	 * 2) Remove vertices that were previously in the camera view but aren't any more.
	 * 3) Get the x values at the start and end of the resulting list of points.
	 * 4) Add new points that have entered the camera view.
	 * 5) Set the firstPoint and lastPoint enumerators to the positions of the first and last element before points were added.
	 * 6) If there are points to the left of the firstPoint, generateVerticesLeft. If there are points to the right of the lastPoint, generateVerticesRight.
	 *
	 */
	
	/// <summary>
	/// Generates the vertex data for the landscape by generating points between the left and right of the screen, adding and/or deleting points and vertices depending on changes in the camera's position.
	/// </summary>
	void generateLandscapeData()
	{
		// TODO make points based on camera position
		// TODO make y value based on x value and z value so always generated the same when degenerated and regenerated when the view comes back
		// TODO as said above, make it so all visiblePoints.Add[First/Last/etc.](...) have a Vector2(x, calcHeight(x, z)) as the parameter. calcY(x) should return the height calculated from the given x and z values 

		// STEP 1 & 2 ======================================================================================================================================================

		if (visiblePoints.Count > 1)
		{
			// Whether a change to the landscape is required.
			bool updateLandscape = false;

			// Check whether the second from leftmost visible point is out of the screen. (One point should be out of the screen).
			if (visiblePoints.First.Next.Value.x < camLeft)
			{
				// Delete all but one point before the left of the viewport.
				while (visiblePoints.First.Next.Value.x < camLeft)
					visiblePoints.RemoveFirst();

				// Delete all vertices up to the new leftmost point.
				while (visibleVertices.First.Value.x < visiblePoints.First.Value.x)
					visibleVertices.RemoveFirst();

				updateLandscape = true;
			}

			// Check whether the second from rightmost visible point is out of the screen. (One point should be out of the screen).
			if (visiblePoints.Last.Previous.Value.x > camRight)
			{
				// Delete all but one point after the right of the screen.
				while (visiblePoints.Last.Previous.Value.x > camRight)
					visiblePoints.RemoveLast();

				// Delete all vertices from the new rightmost point onwards.
				while (visibleVertices.Last.Value.x > visiblePoints.Last.Value.x)
					visibleVertices.RemoveLast();

				updateLandscape = true;
			}

			// If no update to the landscape is needed, exit the function early.
			if (!updateLandscape)
				return;
		}

		Debug.Log ("Updating landscape.");

		// STEP 3 ======================================================================================================================================================

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

			Debug.Log ("All points have been wiped.");
		}

		// STEP 4 ======================================================================================================================================================

		// If there are no points left, generate a single point before the left of the screen then generate further points rightwards. I'm right handed so I have a rightwards bias, muahaha.
		if (allPointsWiped)
		{
			// Get the first multiple of xSpacing before the left of the camera. This includes the left of the camera, but is unlikely since, being a floating point number, it will probably have a different fractional part.
			float nearestMultiple;

			// Round down to the nearest multiple.
			if (camLeft >= 0)
				nearestMultiple = (int)camLeft / xSpacing * xSpacing;
			else
				nearestMultiple = ((int)camLeft - xSpacing) / xSpacing * xSpacing;

			Debug.Log (nearestMultiple + " is the nearest multiple of " + xSpacing + " behind " + camLeft);

			// Add the point to the visible points list.
			visiblePoints.AddFirst(new Vector2(nearestMultiple, calcHeight(nearestMultiple, 0.0f)));

			// Generate all new points rightwards.
			generatePointsRight();
		}
		else
		{
			// If the left x of the camera is further to the left than the leftmost visible point, generate points to the left.
			if (camLeft < visiblePoints.First.Value.x)
				generatePointsLeft();

			// If the right x of the camera is further to the right than the rightmost visible point, generate points to the right.
			if (camRight > visiblePoints.Last.Value.x)
				generatePointsRight();
		}

		// STEP 5 & 6 ==================================================================================================================================================

		// Get the list's enumerators.
		firstPoint = visiblePoints.GetEnumerator();
		lastPoint = visiblePoints.GetEnumerator();

		// Move the enumerators to the start of the list.
		firstPoint.MoveNext();
		lastPoint.MoveNext ();

		if (allPointsWiped)
		{
			// There are no vertices in the list, so add the first one.
			visibleVertices.AddLast(lastPoint.Current);

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
	/// Generates points leftwards until there is a single point outside of the camera view.
	/// </summary>
	void generatePointsLeft()
	{
		Debug.Log ("Generating points leftwards.");

		// Initialise x as the leftmost point.
		float x = visiblePoints.First.Value.x;

		// Generate x values leftwards until there is a single point outside of the camera view, giving the impression that terrain continues off the screen.
		while (x > camLeft)
		{
			// Shift x backwards by the regular spacing amount.
			x -= xSpacing;

			// Add the point before the start of the visible points list.
			visiblePoints.AddFirst(new Vector2(x, calcHeight(x, 0.0f)));
		}
	}

	/// <summary>
	/// Generates points rightwards until there is a single point outside of the camera view.
	/// </summary>
	void generatePointsRight()
	{
		Debug.Log ("Generating points rightwards.");
		
		// Initialise x as the leftmost point.
		float x = visiblePoints.Last.Value.x;
		
		// Generate x values rightwards until there is a single point outside of the camera view, giving the impression that terrain continues off the screen.
		while (x < camRight)
		{
			// Shift x forwards by the regular spacing amount.
			x += xSpacing;
			
			// Add the point after the end of the visible points list.
			visiblePoints.AddLast(new Vector2(x, calcHeight(x, 0.0f)));
		}
	}

	/// <summary>
	/// Generates the vertices between the old first and new first points in the points list.
	/// </summary>
	void generateVerticesLeft()
	{
		Debug.Log ("Generating vertices leftwards.");

		// Get an enumerator at the start of the visible points.
		LinkedList<Vector2>.Enumerator leftPoint = visiblePoints.GetEnumerator();
		leftPoint.MoveNext();

		Vector2 left = leftPoint.Current;
		Vector2 right = firstPoint.Current;
		
		// Keep moving the enumerator right until there are no new points.
		while (leftPoint.Current.x != right.x)
		{
			// Advance the enumerator.
			leftPoint.MoveNext();

			// Set the right value to the current point.
			right = leftPoint.Current;

			// Add the right point.
			visibleVertices.AddFirst(right);

			// Create the intermediate vertices from left to right, with their y calculated from the smoothstep function between the left and right point.
			for (int i = numIntermediateVertices - 1; i >= 1; --i)
				visibleVertices.AddFirst (new Vector2(left.x + i * vertexGap, Mathf.SmoothStep (left.y, right.y, (float)i / numIntermediateVertices)));
			
			// Assign the right point to the left variable for the next point.
			left = right;
		}
	}

	/// <summary>
	/// Generates the vertices between the old last and new last points in the points list.
	/// </summary>
	void generateVerticesRight()
	{
		Debug.Log ("Generating vertices rightwards.");

		Vector2 left = lastPoint.Current;
		Vector2 right;	// Doesn't need to be defined, as the enumerator can travel to the end until null.

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

	/// <summary>
	/// Calculates the y height at the given x and z coordinates using some kind of noise generator or something thats constant.
	/// </summary>
	/// <returns>The height of the point.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="z">The z coordinate.</param> 
	float calcHeight(float x, float z)
	{
		// TODO actually do this properly, and change the summary above.
		return Random.Range (0.0f, yHighest);
	}

	// Use this for initialization
	void Start ()
	{
		camLeft = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.5f, -Camera.main.transform.position.z)).x;
		camRight = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.5f, -Camera.main.transform.position.z)).x;

		generateLandscapeData();

	}
	
	// Update is called once per frame
	void Update ()
	{
		// Update the left and right x values of the camera viewport at the right z value, in the vertical centre.
		camLeft = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.5f, -Camera.main.transform.position.z)).x;
		camRight = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.5f, -Camera.main.transform.position.z)).x;

		generateLandscapeData();

		// DEBUG
		foreach (GameObject g in GameObject.FindGameObjectsWithTag("Respawn"))
			Destroy (g);

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


}