using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class TouchpadInput : MonoBehaviour {

	//public Text rotations;
	private SteamVR_TrackedObject trackedObject;
	private SteamVR_Controller.Device device;
	private SteamVR_TrackedController controller;
	public GameObject plane;

	public Renderer customTouchpad;
	public Material[] letters = new Material[4];
	public Material[] arrows4 = new Material[5];
	public Material[] arrows2 = new Material[3];


	// Use this for initialization
	void Start () {
		trackedObject = GetComponent<SteamVR_TrackedObject>();
		controller = GetComponent<SteamVR_TrackedController>();
		controller.PadClicked += Controller_PadClicked;
		controller.MenuButtonClicked += Controller_MenuButtonClicked;
		controller.TriggerClicked += Controller_TriggerClicked;
		controller.PadTouched += Controller_PadTouched;
		controller.PadUntouched += Controller_PadUntouched;
		controller.Gripped += Controller_Gripped;

	}

	private void Controller_TriggerClicked(object sender, ClickedEventArgs e){
		if (this.tag == "RightController"){
			plane.GetComponent<gameLogic>().matchMiniCube();
		}
		else if (this.tag == "LeftController"){
			// plane.GetComponent<gameLogic>().moveBlockWithArrowKeys("trigger");
			plane.GetComponent<gameLogic>().changeGridSizeViaController();
		}

	}

	private void Controller_Gripped(object sender, ClickedEventArgs e){
		plane.GetComponent<gameLogic>().moveBlockWithArrowKeys("grip");
	}

	private void Controller_PadClicked(object sender, ClickedEventArgs e){
		if (this.tag == "RightController"){
			plane.GetComponent<gameLogic>().moveBlockWithArrowKeys(getClosestButton4());
		}
		else if(this.tag == "LeftController"){
			//plane.GetComponent<gameLogic>().rotateBlockWithArrowKeys(getClosestButton3());
			//plane.GetComponent<gameLogic>().moveBlockWithArrowKeys("padClick");
			plane.GetComponent<gameLogic>().moveUser(getClosestButton2());
		}
	}

	private void Controller_PadTouched(object sender, ClickedEventArgs e){
		updateControllerMaterial();

		if (this.tag == "RightController"){
			customTouchpad.material = arrows4[0];
		}
		else if(this.tag == "LeftController"){
			customTouchpad.material = arrows2[0];
		}
	}

	private void Controller_PadUntouched(object sender, ClickedEventArgs e){
		if (this.tag == "RightController"){
			customTouchpad.material = arrows4[0];
		}
		else if(this.tag == "LeftController"){
			customTouchpad.material = arrows2[0];
		}
	}

	private void Controller_MenuButtonClicked(object sender, ClickedEventArgs e){
		//rotations.text = "Moves:\n";

		if (this.tag == "RightController"){
			plane.GetComponent<gameLogic>().moveBlockWithArrowKeys("menuButton");
		}
		else if(this.tag == "LeftController"){
			plane.GetComponent<gameLogic>().resetGame();
		}
	}

	private void updateControllerMaterial(){
		if (this.tag == "RightController"){
			if(getClosestButton4() == "Up"){
				customTouchpad.material = arrows4[1];
			}
			else if(getClosestButton4() == "Right"){
				customTouchpad.material = arrows4[2];
			}
			else if(getClosestButton4() == "Down"){
				customTouchpad.material = arrows4[3];
			}
			else if(getClosestButton4() == "Left"){
				customTouchpad.material = arrows4[4];
			}
		}
		else if(this.tag == "LeftController"){
			if(getClosestButton2() == "Up"){
				customTouchpad.material = arrows2[1];
			}
			else if(getClosestButton2() == "Down"){
				customTouchpad.material = arrows2[2];
			}
		}
	}

	// Update is called once per frame
	void Update () {
		device = SteamVR_Controller.Input((int)trackedObject.index);
		if(device.GetAxis().x != 0 && device.GetAxis().y != 0){
			updateControllerMaterial();
		}
	}
	float findSquareDistance(float x1, float y1, float x2, float y2){
		return ((x1-x2) * (x1-x2)) + ((y1-y2) * (y1-y2));
	}
	public string getClosestButton3(){
		float x = device.GetAxis().x;
		float y = device.GetAxis().y;
		string closestPoint = "X";
		float smallestSqaureDistance = findSquareDistance(x, y, Mathf.Cos(Mathf.PI/6f), Mathf.Sin(Mathf.PI/6f));

		if (findSquareDistance(x,y,Mathf.Cos(5f*Mathf.PI/6f), Mathf.Sin(5f*Mathf.PI/6f)) < smallestSqaureDistance){
			closestPoint = "Y";
			smallestSqaureDistance = findSquareDistance(x,y,Mathf.Cos(5*Mathf.PI/6f), Mathf.Sin(5*Mathf.PI/6f));
		}
		if (findSquareDistance(x,y,Mathf.Cos(9f*Mathf.PI/6f), Mathf.Sin(9*Mathf.PI/6f)) < smallestSqaureDistance){
			closestPoint = "Z";
			smallestSqaureDistance = findSquareDistance(x,y,Mathf.Cos(9f*Mathf.PI/6f), Mathf.Sin(9f*Mathf.PI/6f));
		}
		return closestPoint;
	}
	public string getClosestButton4(){
		float x = device.GetAxis().x;
		float y = device.GetAxis().y;
		string closestPoint = "Up";
		float smallestSqaureDistance = findSquareDistance(x, y, 0f, 1f);

		if (findSquareDistance(x,y,1f,0f) < smallestSqaureDistance){
			closestPoint = "Right";
			smallestSqaureDistance = findSquareDistance(x,y,1f,0f);
		}
		if (findSquareDistance(x,y,0f,-1f) < smallestSqaureDistance){
			closestPoint = "Down";
			smallestSqaureDistance = findSquareDistance(x,y,0f,-1f);
		}
		if (findSquareDistance(x,y,-1f,0f) < smallestSqaureDistance){
			closestPoint = "Left";
			smallestSqaureDistance = findSquareDistance(x,y,-1f,0f);
		}
		return closestPoint;
	}
	public string getClosestButton2(){
		float x = device.GetAxis().x;
		float y = device.GetAxis().y;
		string closestPoint = "Up";
		float smallestSqaureDistance = findSquareDistance(x, y, 0f, 1f);

		if (findSquareDistance(x,y,0f,-1f) < smallestSqaureDistance){
			closestPoint = "Down";
			smallestSqaureDistance = findSquareDistance(x,y,0f,-1f);
		}

		return closestPoint;
	}
}
