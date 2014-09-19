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
	LinkedList<Vector2> visibleVertices = new LinkedList<Vector2>();	// All vertices visible on screen.

	/// <summary>
	/// Populates the points list with the points between (and including) the point to the left of the camera frustum and the point to the right of the camera frustum.
	/// </summary>
	void populatePointsList()
	{
		// TODO make points based on camera position
		// TODO make y value based on x value and z value so always generated the same when degenerated and regenerated when the view comes back
		// TODO make bool list? of new points to generate vertices between?
		// TODO populate points list by adding only to the front and back? initial population would be generated left to right.
		// TODO if the camera moves right and, for example, 4 new points appear, the left points that are gone will be removed from the start of the list, leaving the previous rightmost point at the end of the list. Loop, generating vertices between each point in sequence after the end of the list, and making each point in sequence become the new end of the list.

		// Add the first point.
		visiblePoints.AddFirst(new Vector2(0.0f, 0.0f));

		// Add further points after the first point, generating vertices between each.
		for (float x = 0.0f + xSpacing; x < 100.0f; x += xSpacing)
		{
			// Get the old last node.
			Vector2 oldLast = visiblePoints.Last.Value;

			// Add the new last node.
			visiblePoints.AddLast (new Vector2(x, Random.Range (0.0f, yHighest)));

			// Generate vertices from the old last node to the new last node.
			generateVerticesRight(oldLast);
		}
	}


	/// <summary>
	/// Generates the vertices between the old last and new last points in the points list.
	/// </summary>
	void generateVerticesRight(Vector2 left)
	{
		Vector2 right = visiblePoints.Last.Value;

		// Add the left point if it doesn't already exist.
		if (visibleVertices.Count == 0 || visibleVertices.Last.Value != left)
			 visibleVertices.AddLast(left);

		// Create the intermediate vertices from left to right, with their y calculated from the smoothstep function between the left and right point.
		for (int i = 1; i < numIntermediateVertices; ++i)
		{
			visibleVertices.AddLast (new Vector2(left.x + i * vertexGap, Mathf.SmoothStep (left.y, right.y, (float)i / numIntermediateVertices)));
		}

		// Add the right point.
		visibleVertices.AddLast(right);
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
