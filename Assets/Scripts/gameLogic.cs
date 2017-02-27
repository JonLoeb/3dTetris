using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UI;


public class gameLogic : MonoBehaviour {

	//magic numbers
	public float cubeScale;
	int boardSize = 5;
	int counter = 0;
	int startHeight = 10;
	float epsilon = 0.01f;

	//Unity Public variables
	public Text score;
	public Renderer gridView;
	public Material[] gridMaterial = new Material[3];
	public Transform user;

	public Transform skyBlock;
	public Transform groundBlock;
	public Transform tempBlock;
	public Transform nextBlock;
	public Transform miniSkyCube;
	public Transform controller;
	public Transform headset;

	public GameObject blueCubie;
	public GameObject greenCubie;
	public GameObject orangeCubie;
	public GameObject pinkCubie;
	public GameObject purpCubie;
	public GameObject yellowCubie;

	//CONVENTION: direction defined by new rotation
private static readonly Quaternion[] cubeRotationTable = {
	Quaternion.LookRotation(Vector3.forward, Vector3.up),//This is equal to Quaternion.identity
	Quaternion.LookRotation(Vector3.left, Vector3.up), //y'
	Quaternion.LookRotation(Vector3.back, Vector3.up),//y2
	Quaternion.LookRotation(Vector3.right, Vector3.up),//y
	Quaternion.LookRotation(Vector3.forward, Vector3.left),//z'
	Quaternion.LookRotation(Vector3.forward, Vector3.down),//z2
	Quaternion.LookRotation(Vector3.forward, Vector3.right),//z
	Quaternion.LookRotation(Vector3.up, Vector3.back),//x'
	Quaternion.LookRotation(Vector3.back, Vector3.down),//x2
	Quaternion.LookRotation(Vector3.down, Vector3.forward),//x
	Quaternion.LookRotation(Vector3.up, Vector3.left),//z' x'
	Quaternion.LookRotation(Vector3.back, Vector3.left),//z y2
	Quaternion.LookRotation(Vector3.down, Vector3.left),//z' x'
	Quaternion.LookRotation(Vector3.left, Vector3.back),//z' y'
	Quaternion.LookRotation(Vector3.right, Vector3.back),//z y
	Quaternion.LookRotation(Vector3.down, Vector3.back),//z2 x
	Quaternion.LookRotation(Vector3.up, Vector3.right),//z x'
	Quaternion.LookRotation(Vector3.back, Vector3.right),//z' y2
	Quaternion.LookRotation(Vector3.down, Vector3.right),//z x
	Quaternion.LookRotation(Vector3.up, Vector3.forward),//z2 x'
	Quaternion.LookRotation(Vector3.left, Vector3.forward),//z y'
	Quaternion.LookRotation(Vector3.right, Vector3.forward),//z' y
	Quaternion.LookRotation(Vector3.left, Vector3.down),//z2 y'
	Quaternion.LookRotation(Vector3.right, Vector3.down)//z2 y
};



	//Values to keep between FixedUpdate
	Quaternion skyBlockRotation = Quaternion.identity;
	bool clearLayersNow = false;
	bool blockStillDropping = true;
	bool gameOver = false;
	int currentScore = 0;
	int nextBlockSize = 0;
	int bestScore = 0;

	void Start () {
		// positionButton = GetComponent<TouchpadInput>();
		createNewSkyBlock();
		createNewSkyBlock();

	}


	// Update is called once per frame
	void FixedUpdate () {
		if(!gameOver){
			changeGridSizeWithKeys();
			counter++;
			if (counter % 100 == 0){
				dropSkyBlock();
				if (!blockStillDropping){
					blockStillDropping = true;
					//if(skyBlock.position.y >= (cubeScale * startHeight) + (cubeScale/2f)){
					if(skyBlock.position.y >= cubeScale * startHeight){
						gameOver = true;
						score.text = "Cubies: " + currentScore + "\nBest: " + bestScore + "\nGame Over";

					}
					else{
						createNewSkyBlock();
					}
				}
			}
			if (clearLayersNow){
				clearFullLayers();
			}
		}
	}


	public void resetGame(){
		destroyAllChilds(skyBlock);
		destroyAllChilds(groundBlock);
		destroyAllChilds(tempBlock);
		destroyAllChilds(nextBlock);
		counter = 0;
		gameOver = false;
		currentScore = 0;
		nextBlockSize = 0;
		createNewSkyBlock();
		createNewSkyBlock();
		user.position = Vector3.zero;


	}

	public void changeGridSizeViaController(){
		if (boardSize == 3){
			changeGridSize(4);
		}
		else if(boardSize == 4){
			changeGridSize(5);
		}
		else if (boardSize == 5){
			changeGridSize(3);
		}
	}

	void changeGridSize(int size){
		boardSize = size;

		if(size == 3){
			gridView.material = gridMaterial[0];
		}
		else if(size == 4){
			gridView.material = gridMaterial[1];
		}
		else if(size == 5){
			gridView.material = gridMaterial[2];
		}

		//( .15f, 1f, .15f) = 5x5 grid
		transform.localScale = (size / 5f) * new Vector3(0.15f, 1f, 0.15f);
		resetGame();

	}

	void clearFullLayers(){
			int fullLayerSize = boardSize * boardSize;
			//fullLayerSize = 16;//for testing because filling the board is hard

			int[] cubiesOnLayer = new int[1];
			cubiesOnLayer[0] = 0;

			//creates array with number of cubies on each layer
			foreach(Transform cubie in groundBlock){
				int cubieLayer = getLayerNumber(cubie);
				if(cubieLayer > cubiesOnLayer.Length - 1){
					int n = cubieLayer - (cubiesOnLayer.Length - 1);
					Array.Resize(ref cubiesOnLayer, cubiesOnLayer.Length + n);

					//make new indexs not null
					for (int i = 0; i < n; i++){
						cubiesOnLayer[cubieLayer - i] = 0;
					}
				}
				cubiesOnLayer[cubieLayer]++;
			}


			//creates array with number of full layers (amount to move down) below each layer
			int[] fullLayersBelowCubie = new int[cubiesOnLayer.Length];
			for(int i = 0; i < fullLayersBelowCubie.Length; i++){
				fullLayersBelowCubie[i] = 0;
			}
			for (int i = 1; i < fullLayersBelowCubie.Length; i++){
				if(cubiesOnLayer[i-1] >= fullLayerSize){
					for(int j = i; j < fullLayersBelowCubie.Length; j++){
						fullLayersBelowCubie[j]++;
					}
				}
			}

			//delete or move cubie

			foreach (Transform cubie in groundBlock) {
				int cubieLayer = getLayerNumber(cubie);
				if(cubiesOnLayer[cubieLayer] >= fullLayerSize){
					GameObject.Destroy(cubie.gameObject);
				}
				else{
					float dropAmount = cubeScale * fullLayersBelowCubie[cubieLayer];
					cubie.position = cubie.position + (dropAmount * Vector3.down);
				}
			}

			clearLayersNow = false;
		}

		int getLayerNumber(Transform cubie){
			//float layerHeight = (cubie.position.y - (cubeScale / 2f)) / cubeScale;
			float layerHeight = (cubie.position.y / cubeScale) - 0.5f;

			return (int) (layerHeight + 0.5f); //rounds to nearest int
		}

		public void moveUser(string direction){
			if(direction.Equals("Up") && user.position.y < cubeScale * (startHeight -1)){
				user.position = user.position + (cubeScale * Vector3.up);
			}
			if(direction.Equals("Down") && user.position.y >= cubeScale){
				user.position = user.position + (cubeScale * Vector3.down);
			}
		}

		Vector3 nearestOrthoVector(Vector3 v){
			Vector3 returnVector = Vector3.forward;

			if(Vector3.Angle(v, Vector3.right) < Vector3.Angle(v, returnVector)){
				returnVector = Vector3.right;
			}
			if(Vector3.Angle(v, Vector3.back) < Vector3.Angle(v, returnVector)){
				returnVector = Vector3.back;
			}
			if(Vector3.Angle(v, Vector3.left) < Vector3.Angle(v, returnVector)){
				returnVector = Vector3.left;
			}

			return returnVector;
		}

	//returns index of array (to aproximate the rotation) that is assosiated with a given rotation
Quaternion nearestCubeRotation(Quaternion rotation){
	int index = 0;
	float closestDistance = Quaternion.Angle(rotation, cubeRotationTable[0]);

	for (int i = 1; i < cubeRotationTable.Length; i++){
		float distance = Quaternion.Angle(rotation, cubeRotationTable[i]);
		if (distance < closestDistance){
			closestDistance = distance;
			index = i;
		}
	}
	return cubeRotationTable[index];
}

	public void matchMiniCube(){
		Quaternion q = nearestCubeRotation(miniSkyCube.rotation) * Quaternion.Inverse(skyBlockRotation);
		rotateSkyBlock(q);
	}

	void destroyAllChilds(Transform parent){
		while (parent.childCount > 0){
								 Transform child = parent.GetChild(0);
								 child.parent = null;
								 Destroy(child.gameObject);
						 }
	}

	void changeParent(Transform oldParent, Transform newParent){

		while (oldParent.childCount > 0){
								 Transform child = oldParent.GetChild(0);
								 child.parent = newParent;
						 }
	}

	void dropSkyBlock(){
		Vector3 dropAmount = (1f * cubeScale) * Vector3.down;
		moveSkyBlock(dropAmount, true);
	}

	void moveSkyBlock(Vector3 displacmentVector, bool moveIsDrop){
		if (isValidBlockPosition(displacmentVector)){
			skyBlock.position = skyBlock.position + displacmentVector;
		}
		else if (moveIsDrop){
			blockStillDropping = false;
		}
	}

	void rotateSkyBlock(Quaternion q){
		skyBlock.rotation = q * skyBlock.rotation;
		skyBlockRotation = q * skyBlockRotation;
		//miniSkyCube.GetChild(0).rotation = q * miniSkyCube.GetChild(0).rotation;

		//check if floor or bounds will be hit
		foreach(Transform cubie in skyBlock){
			if(Mathf.Abs(cubie.position.x) > (cubeScale * ((boardSize - 1f)/2f) + epsilon) ||
			Mathf.Abs(cubie.position.z) > (cubeScale * ((boardSize - 1f)/2f) + epsilon) ||
			cubie.position.y < (cubeScale / 2f) - epsilon){
				skyBlock.rotation = Quaternion.Inverse(q) * skyBlock.rotation;
				skyBlockRotation = Quaternion.Inverse(q) * skyBlockRotation;
				//miniSkyCube.GetChild(0).rotation = Quaternion.Inverse(q) * miniSkyCube.GetChild(0).rotation;

				return;
			}
		}

		//check if other cube will be hit
		foreach(Transform skyCubie in skyBlock){
			foreach(Transform groundCubie in groundBlock){
				if (almostEqual(skyCubie.position, groundCubie.position, epsilon)){
					skyBlock.rotation = Quaternion.Inverse(q) * skyBlock.rotation;
					skyBlockRotation = Quaternion.Inverse(q) * skyBlockRotation;
					//miniSkyCube.GetChild(0).rotation = Quaternion.Inverse(q) * miniSkyCube.GetChild(0).rotation;

				}
			}
		}
		clearRotationValues();
	}

	void clearRotationValues(){
		changeParent(skyBlock, tempBlock);
		skyBlock.rotation = Quaternion.identity;
		//skyBlock.position = Vector3.zero;

		foreach(Transform cubie in tempBlock){
			cubie.rotation = Quaternion.identity;
		}

		changeParent(tempBlock, skyBlock);
	}

	bool isValidBlockPosition(Vector3 displacmentVector){
		//check floor or bounds will be hit
		foreach(Transform cubie in skyBlock){
			if(Mathf.Abs((cubie.position + displacmentVector).x) > (cubeScale * ((boardSize - 1f)/2f) + epsilon) ||
			Mathf.Abs((cubie.position + displacmentVector).z) > (cubeScale * ((boardSize - 1f)/2f) + epsilon) ||
			(cubie.position + displacmentVector).y < (cubeScale / 2f) - epsilon){

				return false;
			}
		}


		//check if other cube will be hit
		foreach(Transform skyCubie in skyBlock){
			foreach(Transform groundCubie in groundBlock){
				if (almostEqual(skyCubie.position + displacmentVector, groundCubie.position, epsilon)){
					return false;
				}
			}
		}

		return true;
	}

	bool almostEqual(Vector3 v1, Vector3 v2, float precision){
		bool equal = true;

		if (Mathf.Abs (v1.x - v2.x) > precision) equal = false;
		if (Mathf.Abs (v1.y - v2.y) > precision) equal = false;
		if (Mathf.Abs (v1.z - v2.z) > precision) equal = false;

		return equal;
	}

	void changeGridSizeWithKeys(){
		//With Arrow Keys
		if(Input.GetKeyDown(KeyCode.Keypad3)){
			changeGridSize(3);
		}
		if(Input.GetKeyDown(KeyCode.Keypad4)){
			changeGridSize(4);
		}
		if(Input.GetKeyDown(KeyCode.Keypad5)){
			changeGridSize(5);
		}
	}

	public void moveBlockWithArrowKeys(string touchButtonDirection){
		//With Arrow Keys
		if(Input.GetKeyDown(KeyCode.UpArrow)){
			moveSkyBlock((1f * cubeScale) * Vector3.forward, false);
		}
		if(Input.GetKeyDown(KeyCode.DownArrow)){
			moveSkyBlock((1f * cubeScale) * Vector3.back, false);
		}
		if(Input.GetKeyDown(KeyCode.LeftArrow)){
			moveSkyBlock((1f * cubeScale) * Vector3.left, false);
		}
		if(Input.GetKeyDown(KeyCode.RightArrow)){
			moveSkyBlock((1f * cubeScale) * Vector3.right, false);
		}
		//With Controller_PadClicked
		if(touchButtonDirection.Equals("Up") ){
			//moveSkyBlock((1f * cubeScale) * Vector3.forward, false);
			moveSkyBlock((1f * cubeScale) * nearestOrthoVector(headset.rotation * Vector3.forward), false);
		}
		if(touchButtonDirection.Equals("Down") ){
			//moveSkyBlock((1f * cubeScale) * Vector3.back, false);
			moveSkyBlock((1f * cubeScale) * nearestOrthoVector(headset.rotation * Vector3.back), false);
		}
		if(touchButtonDirection.Equals("Left") ){
		//	moveSkyBlock((1f * cubeScale) * Vector3.left, false);
			moveSkyBlock((1f * cubeScale) * nearestOrthoVector(headset.rotation * Vector3.left), false);

		}
		if(touchButtonDirection.Equals("Right") ){
//			moveSkyBlock((1f * cubeScale) * Vector3.right, false);
			moveSkyBlock((1f * cubeScale) * nearestOrthoVector(headset.rotation * Vector3.right), false);

		}
		if(touchButtonDirection.Equals("grip")){
			dropSkyBlock();
			counter += 100 - (counter % 100);
		}
		if(touchButtonDirection.Equals("trigger")){
			dropSkyBlock();
			counter += 100 - (counter % 100);
		}
		if(touchButtonDirection.Equals("padClick")){
			dropSkyBlock();
			counter += 100 - (counter % 100);
		}
		if(touchButtonDirection.Equals("menuButton")){
			dropSkyBlock();
			counter += 100 - (counter % 100);
		}
	}

	public void rotateBlockWithArrowKeys(string axisOfRotation){
		if(axisOfRotation.Equals("Y")){
			rotateSkyBlock(Quaternion.LookRotation(Vector3.right, Vector3.up));
		}
		if(axisOfRotation.Equals("Y'")){
			rotateSkyBlock(Quaternion.LookRotation(Vector3.left, Vector3.up));
		}
		if(axisOfRotation.Equals("X")){
			rotateSkyBlock(Quaternion.LookRotation(Vector3.down, Vector3.forward));
		}
		if(axisOfRotation.Equals("X'")){
			rotateSkyBlock(Quaternion.LookRotation(Vector3.up, Vector3.back));
		}
		if(axisOfRotation.Equals("Z")){
			rotateSkyBlock(Quaternion.LookRotation(Vector3.forward, Vector3.right));
		}
	}

	void updateMiniSkyCube(){
		destroyAllChilds(miniSkyCube);
		miniSkyCube.localRotation = Quaternion.identity;
		//miniSkyCube.rotation = Quaternion.identity;

		//miniSkyCube.localScale = new Vector3(1f,1f,1f);

		Vector3 localPosition = miniSkyCube.position;
		miniSkyCube.position = Vector3.zero -  new Vector3(0f,0f,0f);

//		Instantiate(skyBlock, new Vector3(0f, 0f, 0f), Quaternion.identity, miniSkyCube);
		Instantiate(skyBlock, new Vector3(0f, 0f, 0f), controller.rotation, miniSkyCube);


		//miniSkyCube.position = localPosition;
		miniSkyCube.position = controller.position + controller.rotation * new Vector3(0f,0.1f,0f);


		miniSkyCube.localScale = new Vector3(.1f,.1f,.1f);

	}
	void printScore(){
		if(currentScore > bestScore){
			bestScore = currentScore;
		}
		score.text = "Cubies: " + currentScore + "\nBest: " + bestScore;
	}

	void createNewSkyBlock(){

		//empties the skyBlock and resets its position
		resetSkyBlock();

		clearLayersNow = true;

		skyBlockRotation = Quaternion.identity;
		nextBlock.position = Vector3.zero;
		changeParent(nextBlock, skyBlock);
		currentScore += nextBlockSize;
		printScore();

		destroyAllChilds(miniSkyCube);

		updateMiniSkyCube();

		//creates the cubies for the skyBlock
		int foo = Random.Range(0,6);
		//foo = -1;
		if(foo == -1){
			Instantiate(blueCubie, cubeScale * new Vector3(0f, 0f, 0f), Quaternion.identity, nextBlock);
			Instantiate(blueCubie, cubeScale * new Vector3(1f, 0f, 0f), Quaternion.identity, nextBlock);
		}

		if (foo == 0){//blue
			Instantiate(blueCubie, cubeScale * new Vector3(0f, 0f, 0f), Quaternion.identity, nextBlock);
			Instantiate(blueCubie, cubeScale * new Vector3(1f, 0f, 0f), Quaternion.identity, nextBlock);
			Instantiate(blueCubie, cubeScale * new Vector3(1f, 0f, 1f), Quaternion.identity, nextBlock);
			Instantiate(blueCubie, cubeScale * new Vector3(0f, 0f, 1f), Quaternion.identity, nextBlock);
		//	Instantiate(blueCubie, cubeScale * new Vector3(0f, 0f, -1f), Quaternion.identity, nextBlock);
			nextBlockSize = 5;
		}
		else if(foo == 1){//green
			Instantiate(greenCubie, cubeScale * new Vector3(0f, 0f, 0f), Quaternion.identity, nextBlock);
			Instantiate(greenCubie, cubeScale * new Vector3(1f, 0f, 0f), Quaternion.identity, nextBlock);
			Instantiate(greenCubie, cubeScale * new Vector3(0f, 1f, 0f), Quaternion.identity, nextBlock);
			Instantiate(greenCubie, cubeScale * new Vector3(0f, 0f, 1f), Quaternion.identity, nextBlock);
			Instantiate(greenCubie, cubeScale * new Vector3(0f, 0f, -1f), Quaternion.identity, nextBlock);
			nextBlockSize = 5;
		}
		else if(foo == 2){//purple
			Instantiate(orangeCubie, cubeScale * new Vector3(0f, 0f, 0f), Quaternion.identity, nextBlock);
			Instantiate(orangeCubie, cubeScale * new Vector3(1f, 0f, 0f), Quaternion.identity, nextBlock);
			//Instantiate(orangeCubie, cubeScale * new Vector3(2f, 0f, 0f), Quaternion.identity, nextBlock);
			Instantiate(orangeCubie, cubeScale * new Vector3(-1f, 0f, 0f), Quaternion.identity, nextBlock);
			nextBlockSize = 3;

		}
		else if(foo == 3){//pink
			Instantiate(pinkCubie, cubeScale * new Vector3(0f, 0f, 0f), Quaternion.identity, nextBlock);
			Instantiate(pinkCubie, cubeScale * new Vector3(1f, 0f, 0f), Quaternion.identity, nextBlock);
			Instantiate(pinkCubie, cubeScale * new Vector3(0f, 0f, 1f), Quaternion.identity, nextBlock);
			Instantiate(pinkCubie, cubeScale * new Vector3(0f, 1f, 0f), Quaternion.identity, nextBlock);
			nextBlockSize = 4;
		}
		else if(foo == 4){//orange
			Instantiate(purpCubie, cubeScale * new Vector3(0f, 0f, 0f), Quaternion.identity, nextBlock);
			Instantiate(purpCubie, cubeScale * new Vector3(1f, 0f, 0f), Quaternion.identity, nextBlock);
			Instantiate(purpCubie, cubeScale * new Vector3(0f, 1f, 0f), Quaternion.identity, nextBlock);
			Instantiate(purpCubie, cubeScale * new Vector3(1f, 0f, 1f), Quaternion.identity, nextBlock);
			nextBlockSize = 4;
		}
		else if(foo == 5){//yellow
			Instantiate(yellowCubie, cubeScale * new Vector3(0f, 0f, 0f), Quaternion.identity, nextBlock);
			Instantiate(yellowCubie, cubeScale * new Vector3(1f, 1f, 0f), Quaternion.identity, nextBlock);
			Instantiate(yellowCubie, cubeScale * new Vector3(0f, 1f, 0f), Quaternion.identity, nextBlock);
			Instantiate(yellowCubie, cubeScale * new Vector3(0f, 0f, 1f), Quaternion.identity, nextBlock);
			Instantiate(yellowCubie, cubeScale * new Vector3(0f, 0f, -1f), Quaternion.identity, nextBlock);
			nextBlockSize=5;
		}

		nextBlock.position = skyBlock.position + ((0.5f * cubeScale)) * Vector3.up;
		//nextBlock.position = nextBlock.position + (((((boardSize - 1f) / 2f ) + 3f ) * cubeScale) * Vector3.right);
		nextBlock.position = nextBlock.position + (((startHeight + 5f) * cubeScale) * Vector3.up);


		//moves skyBlock to the origin
		skyBlock.position = skyBlock.position + ((0.5f * cubeScale) * Vector3.up);
		if(boardSize % 2 == 0){
			skyBlock.position = skyBlock.position + ((0.5f * cubeScale) * Vector3.right);
			skyBlock.position = skyBlock.position + ((0.5f * cubeScale) * Vector3.forward);
		}

		//moves skyBlock to start of fall location
		skyBlock.position = skyBlock.position + ((startHeight * cubeScale) * Vector3.up);
	}

	void resetSkyBlock(){
		changeParent(skyBlock, groundBlock);
		skyBlock.position = Vector3.zero;
		skyBlock.rotation = Quaternion.identity;
	}


}
