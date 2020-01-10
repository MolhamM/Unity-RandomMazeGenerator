using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Cell  {
	Material lineMaterial;
	int i,j;
	bool[] walls; 
	bool Isvisited;
	int canvasWidth;
	float cellWidth;
	int canvasHight;
	float cellHight;
	int cols;
	int step;
	int rows;
	int beginRow;
	int beginCol;
	int targetRow;
	int targetCol;
	GameObject[] RendererWalls;
	public int dijkstraValue;
	public Cell dijkstraPrevious{ get; set; }
	public int Astar_H;
	public int Astar_G;
	public int Astar_F;
	public Cell Astar_Parent{ get; set; }
	public Cell(int x , int y,int canvaswidth , float cellwidth , int canvashight ,float cellhight,int begin_row,int begin_col,int target_row,int target_col, Material mat)
	{
		lineMaterial = mat;
		this.i = x;
		this.j = y;
		this.canvasWidth = canvaswidth;
		this.cellWidth = cellwidth;
		this.canvasHight = canvashight;
		this.cellHight = cellhight;
		this.step = 0;
		this.beginCol = begin_col;
		this.beginRow = begin_row;
		this.targetCol = target_col;
		this.targetRow = target_row;
		Astar_H = ((Math.Abs (targetCol - beginCol)) + (Math.Abs (targetRow - beginRow)));
		Isvisited = false;
		dijkstraValue = int.MaxValue;
		RendererWalls = new GameObject[4];// top , right , bottom , left
		walls= new bool [4];// top , right , bottom , left
		cols = (int) (this.canvasWidth / this.cellWidth);
		rows = (int) (this.canvasHight / this	.cellHight);
		for (int i = 0; i < 4; i++)
			walls[i] = true;
	}
	void DrawLine(Vector3 start, Vector3 end, ref GameObject originalLine)
	{
		GameObject tempLine = new GameObject();
		tempLine.gameObject.tag = "line2";
		tempLine.transform.position = start;
		tempLine.AddComponent<LineRenderer>();
		LineRenderer lr = tempLine.GetComponent<LineRenderer>();
		//lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
		//lr.material = new Material (Shader.Find("Unlit/Texture"));
		//lr.material = new Material (Shader.Find("Transparent/Diffuse"));
		//lr.material = new Material (Shader.Find("Diffuse"));
		lr.material = lineMaterial;
		lr.startColor = Color.white;
		lr.endColor = Color.white;
		lr.startWidth = 0.1f;
		lr.endWidth = 0.1f;
		lr.SetPosition(0, start);
		lr.SetPosition(1, end);
		originalLine = tempLine;
	}
	public void DrawCells(){
				DrawTopWall ();
				DrawBottomWall ();
				DrawLeftWall ();
				DrawRightWall ();
	}
	void DrawTopWall() {
		Vector3 start = new Vector3 ((this.canvasWidth / 2) - (this.j * this.cellWidth), (this.canvasHight / 2) - (this.i * this.cellHight), 0.0f);
		Vector3 end = new Vector3 (start.x-this.cellWidth, start.y, 0.0f);
			DrawLine (start, end,ref RendererWalls[0]);
	}
	void DrawRightWall (){
		Vector3 start = new Vector3 ((this.canvasWidth / 2) - (this.j * this.cellWidth), (this.canvasHight / 2) - (this.i * this.cellHight), 0.0f);
		Vector3 end = new Vector3 (start.x, start.y - this.cellHight, 0.0f);
			DrawLine (start, end,ref RendererWalls[1]);
	}
	void DrawBottomWall (){
		Vector3 start = new Vector3 ((this.canvasWidth / 2) - (this.j * this.cellWidth), (this.canvasHight / 2) - ((this.i+1) * this.cellHight), 0.0f);
		Vector3 end = new Vector3 (start.x-this.cellWidth, start.y, 0.0f);
			DrawLine (start, end,ref RendererWalls[2]);
	}
	void DrawLeftWall (){
		Vector3 start = new Vector3 ((this.canvasWidth / 2) - ((this.j+1) * this.cellWidth), (this.canvasHight / 2) - (this.i * this.cellHight), 0.0f);
		Vector3 end = new Vector3 (start.x, start.y-this.cellHight, 0.0f);
			DrawLine (start, end,ref RendererWalls[3]);
	}
		
	public void SetVisited(bool temp){
		Isvisited = temp;
	}
	public void SetTop(bool temp){
		walls[0] = temp;
	}
	public void SetTopLine(bool temp){
		RendererWalls [0].SetActive (temp);
	}
	public void SetRight(bool temp){
		walls[1] = temp;
	}
	public void SetRightLine(bool temp){
		RendererWalls [1].SetActive (temp);
	}
	public void SetBottom(bool temp){
		walls[2] = temp;
	}
	public void SetBottomLine(bool temp){
		RendererWalls [2].SetActive (temp);
	}
	public void SetLeft(bool temp){
		walls[3] = temp;
	}
	public void SetLeftLine(bool temp){
		RendererWalls [3].SetActive (temp);
	}
	public void SetSteps(int x){
		this.step = x;
	}
	public int Get_i(){
		return i;
	}
	public int Get_j(){
		return j;
	}
	public bool GetTop(){
		return walls[0];
	}
	public bool GetRight(){
		return walls[1];
	}
	public bool GetBottom(){
		return walls[2];
	}
	public bool GetLeft(){
		return walls[3];
	}
	public bool GetVisited(){
		return Isvisited;
	}
	public int GetSteps(){
		return this.step;
	}
	public void  Update_Astar_F(){
		Astar_F = Astar_G + Astar_H;
	}
}
