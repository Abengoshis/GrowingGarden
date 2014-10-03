using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrFlowerManager : MonoBehaviour {

	const float FLOWER_SPACING = 1.0f;
	private WWW w;
	
	public GameObject flowerPrefab;

	bool downloading;
	List<GameObject> flowerObjects = new List<GameObject>();	// Array of 3 lists of flower objects (one for each plane).

	//FLOWER MATERIALS:
	public Material redFlowerHead;
	public Material blueFlowerHead;
	public Material pinkFlowerHead;
	public Material purpleFlowerHead;
	public Material yellowFlowerHead;
	public Material orangeFlowerHead;
	public Material whiteFlowerHead;

	public Material stemNumber0;
	public Material stemNumber1;
	public Material stemNumber2;
	public Material stemNumber3;
	public Material stemNumber4;
	public Material stemNumber5;
	public Material stemNumber6;
	public Material stemNumber7;
	public Material stemNumber8;
	public Material stemNumber9;


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
					GameObject myFlower = GameObject.Instantiate(flowerPrefab, flowerPosition, Quaternion.identity) as GameObject;
					flowerObjects.Add (myFlower);
					switch (flower[2])
					{
					case 0: //RED
						myFlower.GetComponent<scrFlowerScript>().ChooseFlowerType(redFlowerHead,0);
						break;
					case 1: //BLUE
						myFlower.GetComponent<scrFlowerScript>().ChooseFlowerType(blueFlowerHead,1);
						break;
					case 2: //YELLOW
						myFlower.GetComponent<scrFlowerScript>().ChooseFlowerType(yellowFlowerHead,2);
						break;
					case 3: //WHITE
						myFlower.GetComponent<scrFlowerScript>().ChooseFlowerType(whiteFlowerHead,3);
						break;
					case 4: //PINK
						myFlower.GetComponent<scrFlowerScript>().ChooseFlowerType(pinkFlowerHead,4);
						break;
					case 5: //PURPLE
						myFlower.GetComponent<scrFlowerScript>().ChooseFlowerType(purpleFlowerHead,5);
						break;
					case 6: //ORANGE
						myFlower.GetComponent<scrFlowerScript>().ChooseFlowerType(orangeFlowerHead,6);
						break;
					}

					Random.seed = flower[1] + flower[0];
					switch (Random.Range(0,9))
					{
					case 0: 
						myFlower.GetComponent<scrFlowerScript>().ChooseStemType(stemNumber0);
						break;
					case 1: 
						myFlower.GetComponent<scrFlowerScript>().ChooseStemType(stemNumber1);
						break;
					case 2: 
						myFlower.GetComponent<scrFlowerScript>().ChooseStemType(stemNumber2);
						break;
					case 3: 
						myFlower.GetComponent<scrFlowerScript>().ChooseStemType(stemNumber3);
						break;
					case 4: 
						myFlower.GetComponent<scrFlowerScript>().ChooseStemType(stemNumber4);
						break;
					case 5: 
						myFlower.GetComponent<scrFlowerScript>().ChooseStemType(stemNumber5);
						break;
					case 6: 
						myFlower.GetComponent<scrFlowerScript>().ChooseStemType(stemNumber6);
						break;
					case 7: 
						myFlower.GetComponent<scrFlowerScript>().ChooseStemType(stemNumber7);
						break;
					case 8: 
						myFlower.GetComponent<scrFlowerScript>().ChooseStemType(stemNumber8);
						break;
					case 9: 
						myFlower.GetComponent<scrFlowerScript>().ChooseStemType(stemNumber9);
						break;
					}
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
