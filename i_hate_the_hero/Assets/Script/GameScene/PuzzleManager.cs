using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PuzzleManager : MonoBehaviour {

	//オブジェクト参照
	public GameObject ballPrefab; //ボールプレハブ
	public GameObject containerLine; //ラインコンテナ
	public GameObject linePrefab; //ラインプレハブ
	public GameObject countDown; //カウントダウンタイマーとなるオブジェクト
	public GameObject timer; //制限時間タイマーとなるオブジェクト
	public GameObject score; //スコア表示
	public Sprite[] ballSprite = new Sprite[5]; //ボールの画像

	//メンバ変数
	private GameObject canvasGame; //CanvasGame
	private GameObject firstBall; //最初にタッチしたボール
	private GameObject lastBall; //直前にドラッグしたボール
	private List<GameObject> removableBallList; //消去するボールのリスト
	private string currentName; //現在リストにあるボールの名前
	private bool isPlaying = true; //プレイ中かどうか
	private Text timerText; //タイマーのテキスト
	private Text countDownText; //カウントダウンのテキスト
	private Text scoreText; //スコアテキスト
	private float timeLimit = 60; //制限時間
	private int countTime = 5; //カウントダウンの秒数
	private int currentScore = 0; //現在のスコア
	private float screenW; //画面の幅
	private float screenH; //画面の高さ

	// Use this for initialization
	void Start () {
		canvasGame = GameObject.Find ("CanvasGame");
		screenW = canvasGame.GetComponent<RectTransform>().sizeDelta.x;
		screenH = canvasGame.GetComponent<RectTransform>().sizeDelta.y;
		//countDownText = countDown.GetComponent<Text>(); //タイマーを取得
		//timerText = timer.GetComponent<Text>(); //制限時間タイマー
		//scoreText = score.GetComponent<Text>(); //scoreTextを設定
		StartCoroutine ("DropBall", 44);  //ボールを降らす
		//StartCoroutine ("CountDown"); //カウントダウン開始
	}

	private IEnumerator CountDown(){
		int count = countTime;
		while(count > 0){
			countDownText.text = count.ToString (); //カウントダウンのテキスト
			yield return new WaitForSeconds(1); //1秒待つ
			count -= 1; //カウントを１つ減らす
		}
		countDownText.text = "Start!";
		isPlaying = true;
		yield return new WaitForSeconds (1);
		countDown.SetActive (false);
		StartCoroutine ("DropBall", 44);  //ボールを降らす
		StartCoroutine ("StartTimer"); //制限時間のカウントを開始
	}

	private IEnumerator StartTimer(){
		float count = timeLimit;
		while (count > 0) {
			timerText.text = count.ToString ();
			yield return new WaitForSeconds (1);
			count -= 1;
		}
		countDown.SetActive (true);
		countDownText.text = "Finish";
		OnDragEnd ();
		isPlaying = false;
	}

	//count個ボールを降らせる
	private IEnumerator DropBall(int count){
		for (int i = 0; i < count; i++) {
			
			//ボールのプレハブを読み込み
			GameObject ball = (GameObject)Instantiate (ballPrefab); 
			GameObject container = canvasGame.transform.Find("Field").Find("ContainerDrop").gameObject;
			ball.transform.SetParent (container.transform, false);

			int deltaX = UnityEngine.Random.Range(-100,101);
			ball.transform.localPosition = new Vector3(deltaX,screenH/2, 0);

			/*
			//ボールの座標と角度をランダムに設定
			ball.transform.position = new Vector3 (
				Random.Range (-2.0f, 2.0f), 4, 0);
			ball.transform.eulerAngles = new Vector3 (
				0, 0, Random.Range (-40, 40));
			*/

			//ボールの画像のidをランダムに設定し名前と画像をidに合わせて変更
			int spriteId = Random.Range (0, 5);
			ball.name = "Ball" + spriteId;
			//ball.GetComponent<SpriteRenderer> ().sprite = ballSprite [spriteId];
			ball.GetComponent<Image>().sprite = ballSprite [spriteId];

			//次のボールを生成するまで一定時間待つ
			yield return new WaitForSeconds (0.05f);  
		}
	}

	private void Update(){
		
		if (isPlaying) {
			if (Input.GetMouseButtonDown (0) && firstBall == null) {
				//ボールをドラッグし始めた時
				OnDragStart ();
			} else if (Input.GetMouseButtonUp (0)) {
				//ボールをドラッグし終わった時
				OnDragEnd ();
			} else if (firstBall != null) {
				//ボールをドラッグしている途中
				OnDragging ();
			}
		}

		//scoreText.text = currentScore.ToString(); //現在のスコアを表示
	}

	private void OnDragStart(){

		PointerEventData pointer = new PointerEventData(EventSystem.current);
		pointer.position = Input.mousePosition;
		List<RaycastResult> result = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointer, result);

		foreach (RaycastResult raycastResult in result)
		{
			//何かをドラッグしている時
			GameObject colObj = raycastResult.gameObject;
			if(raycastResult.gameObject.name.IndexOf("Ball") != -1){
				//名前に"Ball"を含むオブジェクトをドラッグした時
				removableBallList = new List<GameObject>(); 
				firstBall = colObj; //はじめにドラッグしたボールを現在のボールに設定
				currentName = colObj.name; //現在のリストのボールの名前を設定
				CheckList(colObj);
			}
		}
	}

	private void OnDragEnd(){
		if(firstBall != null){
			//1つ以上のボールをなぞっている時
			int length = removableBallList.Count;
			if (length >= 3) {
				//消去するリストに３個以上のボールがあれば
				for (int i = 0; i < length; i++) {
					Destroy (removableBallList [i]); //リストにあるボールを消去
				}

				currentScore += (CalculateBaseScore(length) + 50 * length);

				StartCoroutine ("DropBall", length); //消した分だけボールを生成
			} else {
				//消去するリストに3個以上ボールがない時
				for (int j = 0; j < length; j++) {
					GameObject listedBall = removableBallList [j];
					ChangeColor (listedBall, 1.0f); //アルファ値を戻す
					listedBall.transform.GetChild(0).gameObject.SetActive(false); //点を非表示
					//listedBall.name = listedBall.name.Substring (1, 5);
				}
			}
			firstBall = null; //変数の初期化
		}
		for(int i=0;i<containerLine.transform.childCount;i++){
			Destroy(containerLine.transform.GetChild(i).gameObject);
		}
	}

	private void OnDragging(){
		PointerEventData pointer = new PointerEventData(EventSystem.current);
		pointer.position = Input.mousePosition;
		List<RaycastResult> result = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointer, result);

		foreach (RaycastResult raycastResult in result)
		{
			//何かをドラッグしている時
			GameObject colObj = raycastResult.gameObject;
			if (colObj.name == currentName) {
				//現在リストに追加している色と同じ色のボールの時
				if (lastBall != colObj) {
					//直前にリストに入れたのと異なるボールの時
					float dist = Vector2.Distance (lastBall.transform.position, colObj.transform.position);
					if (dist <= 1.5) {
						//ボール間の距離が一定値以下の時
						CheckList (colObj); //消去するリストにボールを追加
					}
				}
			}
		}
	}

	private void CheckList(GameObject obj){
		if(removableBallList.Count == 0){
			//最初のボールなら追加
			PushToList(obj);
		}else{
			if(!isExist(obj)){
				//リストに入ってなかったら追加
				PushToList(obj);
			}else{
				//リストに入ってた場合
				if(removableBallList.Count >= 2){
					//現在のボールが一個前のボールだったら
					if(removableBallList[removableBallList.Count-2]==obj)PopList();
				}
			}
		}
	}

	private bool isExist(GameObject obj){
		for(int i=0;i<removableBallList.Count;i++){
			if(removableBallList[i] == obj)return true;
		}
		return false;
	}

	//リストの最後尾を取り除く
	private void PopList(){
		ChangeColor(removableBallList[removableBallList.Count-1],1f); //最後尾のボールを通常に戻す
		removableBallList[removableBallList.Count-1].transform.GetChild(0).gameObject.SetActive(false); //点を非表示
		removableBallList.RemoveAt(removableBallList.Count-1); //最後尾を削除
		lastBall = removableBallList[removableBallList.Count-1]; //lasBallを現在の最後尾に変更
		DrawLine();
		
	}

	void PushToList(GameObject obj){
		lastBall = obj; //直前にドラッグしたボールに現在のボールを追加
		ChangeColor(obj, 0.5f); //現在のボールを半透明にする
		obj.transform.GetChild(0).gameObject.SetActive(true); //点を表示
		removableBallList.Add(obj); //消去するリストに現在のボールを追加
		DrawLine();
		//obj.name = "_" + obj.name; //区別するため、消去するボールのリストに加えたボールの名前を変更
	}

	private void DrawLine(){
		for(int i=0;i<containerLine.transform.childCount;i++){
			Destroy(containerLine.transform.GetChild(i).gameObject);
		}
		for(int i=0;i<removableBallList.Count-1;i++){
			GameObject newLine = (GameObject)Instantiate(linePrefab);
			newLine.transform.SetParent(containerLine.transform,false);
        	//LineRenderer line = newLine.AddComponent<LineRenderer> ();
			LineRenderer line = newLine.GetComponent<LineRenderer>();
			line.SetPosition(0,removableBallList[i].transform.position + new Vector3(0,0,-10));
			line.SetPosition(1,removableBallList[i+1].transform.position + new Vector3(0,0,-10));
			line.startWidth = 0.05f;
			line.endWidth = 0.05f;
		}
	}




	Collider2D GetCurrentHitCollider(){
		RaycastHit2D hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (Input.mousePosition), Vector2.zero);
		return hit.collider;
	}

	private void ChangeColor(GameObject obj, float transparency){

		Color originalColor = obj.GetComponent<Image> ().color;
		obj.GetComponent<Image>().color = new Color (originalColor.r, originalColor.g, originalColor.b, transparency);
	
	}

	private int CalculateBaseScore(int num){
		int tempScore = 50 * (num) * (num + 1) - 300;
		return tempScore;
	}

	public void Reset(){
		SceneManager.LoadScene ("GameScene");
	}
}