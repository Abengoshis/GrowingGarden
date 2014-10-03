using UnityEngine;
using System.Collections;

public class scrFlowerScript : MonoBehaviour {
	
	public int teamNumber;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ChooseFlowerType(Material flowerMat, int teamNum)
	{
		teamNumber = teamNum;
		this.transform.FindChild("FlowerHead").renderer.material = flowerMat;
	}

	public void ChooseStemType(Material stemMat)
	{
		this.transform.FindChild("FlowerStem").renderer.material = stemMat;
	}
}
