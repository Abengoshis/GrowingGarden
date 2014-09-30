using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrLandscapeGenerator : MonoBehaviour
{
	#region DEBUG

	bool debug = true;
	public GameObject debugPrefab;
	public GameObject flowerManager;

	#endregion

	#region 2D Vertex Generation Variables

	const float eccentricity = 1.0f;	// The bumpiness of the landscape between 0.0 and 1.0 where 0.0 is completely flat.
	const int xSpacing = 10;	// The regular spacing of points along the x axis. An integer because integers are pretty and simple.
	const float yHighest = 10.0f;	// The highest value on the y axis that a point can take. The lowest ground value is 0. Below this level, lakes will form..

	const int numIntermediateVertices = 5;	// The number of intermediate vertices between each point. Higher = smoother terrain.
	const float vertexGap = (float)xSpacing / numIntermediateVertices;	// The x gap between intermediate vertices.

	LinkedList<Vector2> visiblePoints = new LinkedList<Vector2>();	// The currently visible points. This should include the one point before the left of the camera, the one point after the right of the camera, and all points in between.
	LinkedList<Vector2>.Enumerator firstPoint;	// The first point after illegal points are removed.
	LinkedList<Vector2>.Enumerator lastPoint;	// The last point after illegal points are removed.
	bool allPointsWiped;	// Whether or not the list of points has no recurring members after illegal points are removed.

	LinkedList<Vector2> visibleVertices = new LinkedList<Vector2>();	// All vertices visible on screen.

	#endregion

	#region 3D Mesh Generation Variables
	
	const float zDepth = 5.0f;	// The depth of a landscape plane.
	MeshFilter meshFilter;	// The mesh filter component of the generator's gameobject.

	#endregion

	bool landscapeChanged = false;	// Whether or not the landscape points have changed in this update.

	float camLeft;	// The left x of the camera.
	float camRight;	// The right x of the camera.
	


	#region Vertex Generation

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
			// Reset the landscape changed flag.
			landscapeChanged = false;

			// Check whether the second from leftmost visible point is out of the screen. (One point should be out of the screen).
			if (visiblePoints.First.Next.Value.x < camLeft)
			{
				// Delete all but one point before the left of the viewport.
				while (visiblePoints.First.Next.Value.x < camLeft)
					visiblePoints.RemoveFirst();

				// Delete all vertices up to the new leftmost point.
				while (visibleVertices.First.Value.x < visiblePoints.First.Value.x)
					visibleVertices.RemoveFirst();

				landscapeChanged = true;
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

				landscapeChanged = true;
			}

			// An update to the landscape is needed if the first and last points aren't outside the camera view. I could combine this with the next if statement but CLARITY YO.
			if (visiblePoints.First.Value.x > camLeft || visiblePoints.Last.Value.x < camRight)
				landscapeChanged = true;

			// If no update to the landscape is needed, exit the function early.
			if (!landscapeChanged)
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
			if (visiblePoints.First.Value.x > camLeft)
				generatePointsLeft();

			// If the right x of the camera is further to the right than the rightmost visible point, generate points to the right.
			if (visiblePoints.Last.Value.x < camRight)
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
		flowerManager.GetComponent<scrFlowerManager>().StartDownloading(camLeft,camRight);
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

		landscapeChanged = true;
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

		landscapeChanged = true;
	}

	/// <summary>
	/// Generates the vertices between the old first and new first points in the points list.
	/// </summary>
	void generateVerticesLeft()
	{
		Debug.Log ("Generating vertices leftwards.");

		// Get linked list node to firstPoint of visible points
		// addfirst to vertices, going backwards from firstPoint.previous until there are no more nodes.


		LinkedListNode<Vector2> firstNode = visiblePoints.Find (firstPoint.Current);

		while (firstNode.Previous != null)
		{
			// Create the intermediate vertices from right to left, with their y calculated from the smoothstep function between the left and right point. Place these vertices at the start of the vertex list.
			for (int i = numIntermediateVertices - 1; i >= 0; --i)
				visibleVertices.AddFirst(new Vector2(firstNode.Previous.Value.x + i * vertexGap, Mathf.SmoothStep (firstNode.Previous.Value.y, firstNode.Value.y, (float)i / numIntermediateVertices)));

			firstNode = firstNode.Previous;
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
	/// Calculates the y height at the given x and z coordinates using perlin noise.
	/// </summary>
	/// <returns>The height of the point.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="z">The z coordinate.</param> 
	float calcHeight(float x, float z)
	{
		// Perlin noise is always 0 on integers, so add a fractional part between 0.0 and 0.5 to give less or more eccentricity.
		return Mathf.PerlinNoise(x + eccentricity * 0.5f, z + eccentricity * 0.5f) * yHighest;
	}

	#endregion

	#region Mesh Generation

	void generateMeshData()
	{
		Debug.Log ("Updating mesh data.");

		// Create a new mesh.
		Mesh mesh = new Mesh();

		/* Create the plane's true vertices by looping through the generated vertex list and duplicating each element,
		 * then pushing the duplicate through the z by the desired depth of the plane. */


		// TODO will need 2 submeshes. meshVertices will contain all visible vertices * 3, and the vertices will be generated so that it first generates the visible vertex, then the visible vertex with its z value set to the z depth, then the visible vertex with its y value set off the screen.
		// TODO looping through the vertices in the visible vertex list, and keeping track of i such that for each vertex i is at the visible vertex in the vertices array, the top (grass) indices will be i, i + 1; the side (dirt) will be i + 2, i

		Vector3[] meshVertices = new Vector3[visibleVertices.Count * 3];

		int[] grassMeshIndices = new int[visibleVertices.Count * 2];
		int[] dirtMeshIndices = new int[grassMeshIndices.Length];

		Vector2[] meshUV = new Vector2[meshVertices.Length];

		int i = 0;
		int j = 0;
		int u = (Mathf.Abs (visiblePoints.First.Value.x) % (xSpacing * 2)) > 0 ? 1 : 0;
		LinkedList<Vector2>.Enumerator vertex = visibleVertices.GetEnumerator();
		while (vertex.MoveNext())
		{
			// Write the near vertex.
			meshVertices[i].x = vertex.Current.x;
			meshVertices[i].y = vertex.Current.y;
			meshVertices[i].z = 0.0f;	// TODO make this based on current plane. (0.0f)

			// Write the near index. (shared)
			grassMeshIndices[j] = i;
			dirtMeshIndices[j] = i + 2;

			// Write the near (bottom) UV.
			meshUV[i].x = u;
			meshUV[i].y = 0.0f;

			++i;
			++j;

			// Write the far vertex.
			meshVertices[i].x = vertex.Current.x;
			meshVertices[i].y = vertex.Current.y;
			meshVertices[i].z = 0.0f + zDepth;	// TODO make this based on current plane. (0.0f)

			// Write the far index.
			grassMeshIndices[j] = i;

			// Write the far (top) UV.
			meshUV[i].x = u;
			meshUV[i].y = 1.0f;

			++i;
			++j;

			// Write the low vertex.
			meshVertices[i].x = vertex.Current.x;
			meshVertices[i].y = -10.0f;	// TODO make this based on....something?
			meshVertices[i].z = 0.0f;	// TODO make this based on the current plane. (0.0f)

			// Write the low index.
			dirtMeshIndices[j - 1] = i - 2;

			// Write the low (top) UV. (the texture will be upside down!)
			meshUV[i].x = u;
			meshUV[i].y = 1.0f;

			++i;

			// Move the u to the opposite side.
			u = u == 0 ? 1 : 0;
		}

		// Set the vertices.
		mesh.vertices = meshVertices;

		// Set the number of submeshes.
		mesh.subMeshCount = 2;

		// Set the grass and dirt indices.
		mesh.SetTriangleStrip(grassMeshIndices, 0);	// Obsolete function is actually useful!
		mesh.SetTriangleStrip(dirtMeshIndices, 1);

		// Set the UVs.
		mesh.uv = meshUV;

		// Calculate the normals.
		mesh.RecalculateNormals();

		meshFilter.mesh = mesh;

		
	}

	#endregion

	// Use this for initialization
	void Start ()
	{
		meshFilter = GetComponent<MeshFilter>();

		camLeft = Camera.main.ViewportToWorldPoint(new Vector3(-0.5f, 0.5f,  -Camera.main.transform.position.z)).x;
		camRight = Camera.main.ViewportToWorldPoint(new Vector3(1.5f, 0.5f, -Camera.main.transform.position.z)).x;

		generateLandscapeData();
		generateMeshData();

	}
	
	// Update is called once per frame
	void Update ()
	{
		// Update the left and right x values of the camera viewport at the right z value, in the vertical centre.
		camLeft = Camera.main.ViewportToWorldPoint(new Vector3(-0.5f, 0.5f, -Camera.main.transform.position.z)).x;
		camRight = Camera.main.ViewportToWorldPoint(new Vector3(1.5f, 0.5f, -Camera.main.transform.position.z)).x;

		// Generate the landscape's points.
		generateLandscapeData();

		// If the landscape data has changed, update the mesh data.
		if (landscapeChanged)
			generateMeshData();

		if (!debug)
		{
			foreach (GameObject g in GameObject.FindGameObjectsWithTag("Respawn"))
				Destroy (g);

			debugPrefab.transform.localScale = 0.25f * Vector3.one;
//			LinkedList<Vector2>.Enumerator e = visibleVertices.GetEnumerator();
//			while (e.MoveNext())
//			{
//				Instantiate(debugPrefab, new Vector3(e.Current.x, e.Current.y), Quaternion.identity);
//			}

			foreach (Vector3 v in meshFilter.mesh.vertices)
			{
				Instantiate(debugPrefab, v, Quaternion.identity);
			}
			
			debugPrefab.transform.localScale = 0.5f * Vector3.one;
			
			LinkedList<Vector2>.Enumerator e2 = visiblePoints.GetEnumerator();
			while (e2.MoveNext())
			{
				Instantiate(debugPrefab, new Vector3(e2.Current.x, e2.Current.y), Quaternion.identity);
			}
		}
	}


}
