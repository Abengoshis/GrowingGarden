using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrFlowerManager : MonoBehaviour {

	const float FLOWER_SPACING = 1.0f;
	private WWW w;
	
	public GameObject flowerPrefab;

	bool downloading;
	List<GameObject> flowerObjects = new List<GameObject>();	// Array of 3 lists of flower objects (one for each plane).

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

		if (downloading)
		{
			if(w.error != null)
			{
				Debug.Log("Failed to download data to Hitpoint Server!");
			}
			else if (w.isDone)
			{
				Debug.Log("Successfully downloaded data to Hitpoint Server!");

				// Clear the current flowers.
				for (int i = flowerObjects.Count - 1; i >= 0; --i)
				{
					Destroy (flowerObjects[i]);
					flowerObjects.RemoveAt (i);
				}

				//Comes in the format: x,plane,team; x,plane,team; etc.
				string rawFlowerData = w.text;
				string[] rawFlowersSeperated = rawFlowerData.Split(';');
				for (int i = 0; i < rawFlowersSeperated.Length - 1;i++)
				{
					//Take apart the current flower:
					string[] flowerString = rawFlowersSeperated[i].Split(',');
					
					int[] flower = new int[flowerString.Length];
					
					for(int j = 0; j < 3; j++)
					{
						Debug.Log(flowerString[j].ToString());
						flower[j] = int.Parse(flowerString[j]);
					}

					// TODO remove this 
					flower[1] = 2;

					// Raycast to get the y value. Since in future there will be more planes which depend on previous planes, I can't simply request the y position from an algorithm.. 
					RaycastHit hit;
					Vector3 flowerPosition = new Vector3(flower[0] * FLOWER_SPACING, 20.0f, flower[1]);
					if (Physics.Raycast (flowerPosition, Vector3.down, out hit, 100.0f, 1 << LayerMask.NameToLayer("Landscape")))
						flowerPosition.y = hit.point.y + flowerPrefab.transform.localScale.y * 0.5f;
					else
						Debug.Log ("Something went wrong and the flower raycast didn't hit the landscape.");

					// Add a flower object.
					flowerObjects.Add (GameObject.Instantiate(flowerPrefab, flowerPosition, Quaternion.identity) as GameObject);
				}

				
				downloading = false;
			}
		}

	}

	public void DownloadFlowerData(float left, float right)
	{
		Debug.Log ("Sending download request to Hitpoint Server!");

		//To get data http://www.hitpointgames.com/AddFlower.php?GetFlower=4;7
		//Where 4 and 7 are the left and right of the screen, respectively...
		w = new WWW("http://www.hitpointgames.com/AddFlower.php?GetFlower="
		            +(int)(left/FLOWER_SPACING)+";"
		            +Mathf.CeilToInt(right/FLOWER_SPACING));

		downloading = true;
	}

	bool UploadFlowerData(float worldLocation,int plane,int team)
	{
		int location = (int)worldLocation / (int)FLOWER_SPACING;
		w = new WWW("http://www.hitpointgames.com/AddFlower.php?AddFlower="+location+";"+plane+";"+team);
		if(w.error != null)
		{
			Debug.Log("Failed to upload data to Hitpoint Server!");
			return false;
		}
		else
		{
			Debug.Log("Successfully uploaded data to Hitpoint Server!");
			return true;
		}
	}

}
