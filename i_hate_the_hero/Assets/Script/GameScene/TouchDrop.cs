using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchDrop : MonoBehaviour {

	public GameObject ballPrefab;
	public Sprite[] spriteBall;

	private GameObject firstBall;
	private List<GameObject> removableBallList;
	private GameObject lastBall;
	private string currentName;

	private void Update(){
		if (Input.GetMouseButton(0) && firstBall == null) {
			//ボールをドラッグしはじめたとき
			OnDragStart();
		} else if (Input.GetMouseButtonUp(0)) {
			//ボールをドラッグし終わったとき
			OnDragEnd();
		} else if (firstBall != null) {
			//ボールをドラッグしている途中
			OnDragging();
		}
	}
	
	private void OnDragStart(){
	}

	private void OnDragEnd(){

	}

	private void OnDragging(){

	}
}
