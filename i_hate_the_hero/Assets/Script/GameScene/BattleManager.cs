using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class BattleManager : MonoBehaviour {

	public GameObject containerLine; //ラインが入ってる
	public GameObject containerDrop; //Dropが入ってる
	public GameObject containerChara; //連れてきたキャラ
	public GameObject circlePrefab; //飛んでくたまのプレハブ
	public int[] charaState = new int[5]; //キャラのステイト 0→攻撃しない 1→火 2→水 3→木
	public GameObject[] effectPrefab = new GameObject[3]; //エフェクトプレハブ
	public GameObject hero; //敵
	public int[] charaType = new int[5];

	private int ballCount = 0; //消すボールの個数
	private Color[] colorarr = {Color.red,Color.blue,Color.green};

	private void Start(){

	}

	public void Attack(List<List<GameObject>> ballArr){

		ConstrainSwitch(true); //重力を0に

		StartCoroutine("DeleteBall",ballArr); //順々にボールとラインを消す

		//this.GetComponent<PuzzleManager>().DropBallP(ballCount);
		/*
		for(int i=0;i<3;i++){
			GameObject lines = containerLine.transform.GetChild(i).gameObject;
			for(int j=0;j<lines.transform.childCount;j++){
				
				Destroy(lines.transform.GetChild(j).gameObject);
			}
		}
		*/
	}

	private void ConstrainSwitch(bool b){
		for(int i=0;i<containerDrop.transform.childCount;i++){
			GameObject ball = containerDrop.transform.GetChild(i).gameObject;
			if(b){
				//固定
				ball.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePosition;
			}else{
				//固定を解除
				ball.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
			}
		}
	}

	private IEnumerator DeleteBall(List<List<GameObject>> ballArr){
		this.GetComponent<PuzzleManager>().isPlaying = false; //画面タッチ無効
		for(int i=0;i<3;i++){

			int id = int.Parse(ballArr[i][0].name[5].ToString());
			ShowAttackValue(id);

			List<GameObject> balls = ballArr[i];
			GameObject lines = containerLine.transform.GetChild(i).gameObject;
			//Ballの削除
			for(int j=0;j<balls.Count;j++){
				Destroy(balls[j]);
				ballCount++;
			}
			//Lineの削除
			for(int j=0;j<lines.transform.childCount;j++){
				
				Destroy(lines.transform.GetChild(j).gameObject);
			}

			if(i==2){
				//最後まで終わったら
				
				ballArr.Clear(); //ballArrを初期化
				ConstrainSwitch(false); //重力戻す
				this.GetComponent<PuzzleManager>().DropBallP(ballCount); //ボールの生成
				ballCount = 0; //ballCountを初期化
				StartCoroutine("AttackAnimation");

			}

			yield return new WaitForSeconds(0.7f);

		}
	}

	private void ShowAttackValue(int id){
		if(id==3)return; //回復だったらリターン

		for(int i=0;i<charaType.Length;i++){
			if(charaType[i] == id){
				//キャラの属性と消したボールの属性が一緒だったら
				GameObject text = containerChara.transform.GetChild(i).GetChild(2).gameObject;
				charaState[i] = id+1; //キャラに攻撃を設定
				text.GetComponent<Text>().color = colorarr[id];
				text.gameObject.SetActive(true);
				PopAnimation(text,10f);
			}
		}

	}

	private void PopAnimation(GameObject obj,float y){
		float timeAnim = 0.5f;
		//float nowY = obj.transform.localPosition.y;
		iTween.MoveTo(obj, iTween.Hash("y",y,"time",timeAnim/2f,"isLocal",true,"easetype",iTween.EaseType.easeOutQuart));
		
		StartCoroutine(DelayMethod(timeAnim/2f,() => {
			iTween.MoveTo(obj, iTween.Hash("y",-1 * y,"time",timeAnim/2f,"isLocal",true
				,"easetype",iTween.EaseType.easeInQuart));
		}));
	}

	private IEnumerator AttackAnimation(){

		for(int i=0;i<charaType.Length;i++){
			yield return new WaitForSeconds(0.5f);

			while(charaState[i] == 0){
				i++;
				if(i == charaType.Length)break;
			}

			if(i < charaType.Length){
				int attackId = charaState[i];
				float timeAnim = 1f;
				/* circleの作成 */
				GameObject circle = (GameObject)Instantiate(circlePrefab);
				circle.transform.SetParent(GameObject.Find("CanvasGame").transform,false);
				circle.transform.position = containerChara.transform.GetChild(i).position;
				circle.GetComponent<Image>().color = colorarr[attackId-1];
				RectTransform rect = circle.GetComponent<RectTransform>();

				//軌跡設定
				Vector3[] path = {
					new Vector3(rect.position.x - 0.25f, 3.5f,0f), //中間地点
					hero.transform.position, //到達点
				};

				//DOTweenを使ったアニメ作成
				rect.DOPath(path,timeAnim,PathType.CatmullRom)
					.SetEase(Ease.OutQuart);
				/* circleの消去とエフェクトの表示 */
				StartCoroutine(DelayMethod(timeAnim,() => {
					Destroy(circle);
					ShowEffect(attackId);
				}));
				

				
			}else{
				StartCoroutine(DelayMethod(1.5f,() => {
					Reset();
				}));
				
			}
		}
	}

	private void Reset(){
		/* 初期化 */
		this.GetComponent<PuzzleManager>().isPlaying = true; //画面タッチ有効
		//テキスト非表示
		for(int i=0;i<containerChara.transform.childCount;i++){
			containerChara.transform.GetChild(i).GetChild(2).gameObject.SetActive(false);
		}
		//CharaState初期化
		for(int i=0;i<charaState.Length;i++){
			charaState[i] = 0;
		}
	}

	private void ShowEffect(int id){
		GameObject containerEffect = hero.transform.GetChild(1).gameObject;

		GameObject effect = (GameObject)Instantiate(effectPrefab[id-1]);
		effect.transform.SetParent(containerEffect.transform,false);
		
		StartCoroutine(DelayMethod(1.5f,() => {
			Destroy(effect);
		}));

	}

	private IEnumerator DelayMethod(float waitTime, Action action)
	{
		yield return new WaitForSeconds(waitTime);
		action();
	}
}
