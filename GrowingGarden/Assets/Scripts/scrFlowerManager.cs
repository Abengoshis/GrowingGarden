using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrFlowerManager : MonoBehaviour {

	const float FLOWER_SPACING = 0.5f;
	private WWW w;
	List<GameObject> flowerObjects;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	


	}

	void DownloadFlowerData(float left, float right)
	{
		//List for storing processed flowers from the server:
		List<Vector3> processedFlowerData;
		//To get data http://www.hitpointgames.com/AddFlower.php?GetFlower=4;7
		//Where 4 and 7 are the left and right of the screen, respectively...
		w = new WWW("http://www.hitpointgames.com/AddFlower.php?GetFlower="
		            +(int)(left/FLOWER_SPACING)+";"
		            +Mathf.CeilToInt(right/FLOWER_SPACING));
		yield w;
		if(w.error != null)
		{
			Debug.Log("Failed to download data to Hitpoint Server!");
		}
		else
		{
			Debug.Log("Successfully downloaded data to Hitpoint Server!");
			//Comes in the format: x,plane,team; x,plane,team; etc.
			string rawFlowerData = w.text;
			string[] rawFlowersSeperated = rawFlowerData.Split(';');
			for (int i = 0; i < rawFlowersSeperated.Length();i++)
			{
				//Take apart the current flower:
				string[] myFlower = rawFlowersSeperated[i].Split(',');
				processedFlowerData.Add(new Vector3(myFlower[0] * FLOWER_SPACING,myFlower[1],myFlower[2]));
			}

			flowerObjects.Clear();
		}

	}

	bool UploadFlowerData(float worldLocation,int plane,int team)
	{
		int location = worldLocation / FLOWER_SPACING;
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
