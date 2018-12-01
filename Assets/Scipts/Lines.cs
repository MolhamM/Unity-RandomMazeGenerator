using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Lines : MonoBehaviour {

	public  int canvasWidth;
	public   float cellWidth;
	public  int canvasHight;
	public   float cellHight;
	public GameObject Zombie;
	public GameObject brain;
	public GameObject StartingText;
	public GameObject Finishing1Text;
	public GameObject Finishing2Text;
	int begain_row;
	int begain_col;
	int target_row;
	int target_col;
	bool haveReached;
	bool startAi;
	bool startBackTracking;
	public int MazeGeneratefps;
	public int Pathfps;
	int randStartPointRow;
	int randStartPointCol;
	Vector3 startofDrawingMaze;
	Vector3 endofDrawingMaze;
	Vector3 startofDrawingBacktrackSolution;
	Vector3 endofDrawingBacktrackSolution;
	bool mazeFinished;
	bool aiFinished;
	bool backTrackFinished;
	int cols;
	int rows;
	Cell[,]grids;
	struct cellEdge{
		public int i,j;
		public char wall;
	};
	struct pathOfMaze{
		public int currenti,currentj,nexti,nextj;
		public pathOfMaze(int i , int j , int ii , int jj){
			currenti = i;
			currentj=j;
			nexti=ii;
			nextj=jj;
		}
	};
	struct index{
		public int i, j;
		public index(int i , int j){
			this.i=i;
			this.j=j;
		}
	};
	Queue <cellEdge> DestroyEdges = new Queue<cellEdge>();
	Queue<pathOfMaze> MazeGenerationPath = new Queue<pathOfMaze> ();
	Queue<pathOfMaze> AiPath = new Queue<pathOfMaze> ();
	Queue<index> BackTrackPath = new Queue<index>();
	Stack<pathOfMaze> BackTrackOnAiWrongPath = new Stack<pathOfMaze>();
	void Awake(){
		QualitySettings.vSyncCount = 0;
		cols = (int) (canvasWidth / cellWidth);
		rows = (int) (canvasHight / cellHight);
		grids= new Cell[rows,cols];
		begain_row = rows;
		begain_col = cols;
		target_row = 1;
		target_col = 1;
		startofDrawingBacktrackSolution = new Vector3 (((canvasWidth / 2) - ((target_row-1) * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - ((target_col-1) * this.cellHight)) - (cellHight / 2), 0.0f);
		brain.transform.localScale = new Vector3 (cellWidth, cellHight, 0.0f);
		brain.transform.position=new Vector3 ((canvasWidth / 2)  - (cellWidth / 2), (this.canvasHight / 2) - (cellHight / 2), 0.0f);
		Zombie.transform.localScale = new Vector3 (cellWidth, cellHight, 0.0f);
		Zombie.transform.position = new Vector3(((canvasWidth / 2) - ((cols-1) * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - ((rows-1) * this.cellHight)) - (cellHight / 2), 0.0f);
		brain.SetActive (false);
		Zombie.SetActive (false);
		//DrawCanvas ();
	}
	void Start () {
		haveReached = false;
		mazeFinished = false;
		aiFinished = false;
		backTrackFinished = false;
		startAi = false;
		startBackTracking = false;
		StartingText.SetActive(false);
		Finishing1Text.SetActive(false);
		Finishing2Text.SetActive(false);
		fillCells ();
		randStartPointRow = Random.Range (0, rows);
		randStartPointCol = Random.Range (0, cols);
		Traverse (ref grids [randStartPointRow,randStartPointCol]);
		ResetVisited ();
		Findingpath(ref grids [begain_row-1,begain_col-1]);
	}

	void Update(){
		if (!mazeFinished) {
			Application.targetFrameRate = MazeGeneratefps;
			DrawTheMaze ();
			if (mazeFinished) {
				brain.SetActive (true);
				Zombie.SetActive (true);
				StartingText.gameObject.SetActive(true);
				StartCoroutine (WaitAfterMaze ());
			}
		} 
		else if (!aiFinished && startAi) {
			Application.targetFrameRate = Pathfps;
			StartingText.SetActive(false);
			DrawPath (AiPath);
			if (aiFinished) {
				brain.transform.position=new Vector3 (((canvasWidth / 2) - ((cols-1) * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - ((rows-1)* this.cellHight)) - (cellHight / 2), 0.0f);
				Finishing1Text.SetActive (true);
				StartCoroutine (WaitAfterPath ());
			}
		} else if(aiFinished && startBackTracking ){
			Finishing1Text.SetActive (false);
			BackTrackSolution ();
			if (backTrackFinished) {
				Finishing2Text.SetActive (true);
			}
		}
	}
	void BackTrackSolution(){
		if (BackTrackPath.Count != 0) {
			index temp;
			temp = BackTrackPath.Dequeue ();
			endofDrawingBacktrackSolution = new Vector3 (((canvasWidth / 2) - (temp.j * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - (temp.i * this.cellHight)) - (cellHight / 2), 0.0f);
			DrawLine (startofDrawingBacktrackSolution, endofDrawingBacktrackSolution, Color.green, 1);
			startofDrawingBacktrackSolution = endofDrawingBacktrackSolution;
		} else {
			backTrackFinished = true;
		}
	}
	void DrawPath(Queue<pathOfMaze> mypath){
		if (mypath.Count != 0) {
			pathOfMaze path;
			path = mypath.Dequeue ();
			startofDrawingMaze = new Vector3 (((canvasWidth / 2) - (path.currentj * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - (path.currenti * this.cellHight)) - (cellHight / 2), 0.0f);
			endofDrawingMaze = new Vector3 (((canvasWidth / 2) - (path.nextj * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - (path.nexti * this.cellHight)) - (cellHight / 2), 0.0f);
			DrawLine (startofDrawingMaze, endofDrawingMaze, Color.yellow,0);
		} else
			aiFinished = true;
	}
	void DrawTheMaze(){
		if (DestroyEdges.Count != 0) {
			cellEdge Edge;
			Edge = DestroyEdges.Dequeue ();
			if (Edge.wall == 't') {
				grids [Edge.i, Edge.j].SetTopLine (false);
			} else if (Edge.wall == 'b') {
				grids [Edge.i, Edge.j].SetBottomLine (false);
			} else if (Edge.wall == 'r') {
				grids [Edge.i, Edge.j].SetRightLine (false);
			} else if (Edge.wall == 'l') {
				grids [Edge.i, Edge.j].SetLeftLine (false);
			}
		} else {
			mazeFinished = true;
			grids [0, 0].SetRightLine (false);
			grids [rows - 1, cols - 1].SetLeftLine (false);
		} 
	}


	void fillCells(){
		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < cols; j++) {
				Cell temp = new Cell(i,j,this.canvasWidth,this.cellWidth,this.canvasHight,this.cellHight);
				temp.DrawCells ();
				grids [i,j] = temp;
			}
		}

	}
	void DrawLine(Vector3 start, Vector3 end,Color color, int sortorder)
	{
		GameObject tempLine = new GameObject();
		tempLine.gameObject.tag = "line";
		tempLine.transform.position = start;
		tempLine.AddComponent<LineRenderer>();
		LineRenderer lr = tempLine.GetComponent<LineRenderer>();
		lr.sortingOrder =sortorder;
		lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
		lr.SetColors(color,color);
		lr.SetWidth(0.1f, 0.1f);
		lr.SetPosition(0, start);
		lr.SetPosition(1, end);
		Zombie.transform.position = end;
	}
	int[] RandomArray(){
		System.Random randnum = new System.Random ();
		int[] arr2 = new int[4];
		int i =0;
		int[] arr = { 1, 2, 3, 4 };
		while (true) {
			int x = randnum.Next (1, 5);
			for(int j = 0 ; j <4 ; j ++)
			{
				if(arr[j]==x){
					arr2 [i++] = arr [j];
					arr [j] = 0;
				}
			}
			if (i == 4)
				break;
		}
		return arr2;		
	}
	void Traverse(ref Cell currentCell){
		currentCell.SetVisited (true);
		print ("i : " + currentCell.Get_i ());
		print ("j : " + currentCell.Get_j ());
		int []arr= RandomArray();
		for (int i = 0; i < 4; i++) {
			if (arr [i] == 1) 
				TraverseTop (ref currentCell);
			else if (arr [i] == 2) 
				TraverseRight (ref currentCell);
		 	else if (arr [i] == 3) 
				TraverseBottom (ref currentCell);
			else if (arr [i] == 4) 
				TraverseLeft (ref currentCell);
		}
	}
	void TraverseTop(ref Cell currentCell){
		if (currentCell.Get_i()> 0) {
			Cell nextCell = grids [currentCell.Get_i()- 1, currentCell.Get_j()];
			if (!nextCell.GetVisited()) {
				cellEdge temp;
				currentCell.SetTop (false);
				temp.i = currentCell.Get_i ();
				temp.j = currentCell.Get_j ();
				temp.wall = 't';
				DestroyEdges.Enqueue (temp);
				nextCell.SetBottom(false);
				temp.i = nextCell.Get_i ();
				temp.j = nextCell.Get_j ();
				temp.wall = 'b';
				DestroyEdges.Enqueue (temp);
				MazeGenerationPath.Enqueue(new pathOfMaze(currentCell.Get_i(),currentCell.Get_j(),nextCell.Get_i(),nextCell.Get_j()));
				Traverse (ref grids [currentCell.Get_i()- 1, currentCell.Get_j()]);
			}
		}
	}
	void TraverseBottom(ref Cell currentCell){
		if (currentCell.Get_i() < rows-1 ) {
			Cell nextCell = grids [currentCell.Get_i()+1 , currentCell.Get_j()];
			if (!nextCell.GetVisited()) {
				cellEdge temp;
				currentCell.SetBottom (false);
				temp.i = currentCell.Get_i ();
				temp.j = currentCell.Get_j ();
				temp.wall = 'b';
				DestroyEdges.Enqueue (temp);
				nextCell.SetTop(false);
				temp.i = nextCell.Get_i ();
				temp.j = nextCell.Get_j ();
				temp.wall ='t';
				DestroyEdges.Enqueue (temp);
				MazeGenerationPath.Enqueue(new pathOfMaze(currentCell.Get_i(),currentCell.Get_j(),nextCell.Get_i(),nextCell.Get_j()));
				Traverse (ref grids [currentCell.Get_i()+1 , currentCell.Get_j()]);
			}
		}
	}
	void TraverseRight(ref Cell currentCell){
		if (currentCell.Get_j () > 0 ) {
			Cell nextCell = grids [currentCell.Get_i() , currentCell.Get_j()-1];
			if (!nextCell.GetVisited()) {
				cellEdge temp;
				currentCell.SetRight (false);
				temp.i = currentCell.Get_i ();
				temp.j = currentCell.Get_j ();
				temp.wall = 'r';
				DestroyEdges.Enqueue (temp);
				nextCell.SetLeft(false);
				temp.i = nextCell.Get_i ();
				temp.j = nextCell.Get_j ();
				temp.wall = 'l';
				DestroyEdges.Enqueue (temp);
				MazeGenerationPath.Enqueue(new pathOfMaze(currentCell.Get_i(),currentCell.Get_j(),nextCell.Get_i(),nextCell.Get_j()));
				Traverse (ref grids [currentCell.Get_i() , currentCell.Get_j()-1]);
			}
		}
	}
	void TraverseLeft(ref Cell currentCell){
		if (currentCell.Get_j () < cols-1 ) {
			Cell nextCell = grids [currentCell.Get_i() , currentCell.Get_j()+1];
			if (!nextCell.GetVisited()) {
				cellEdge temp;
				currentCell.SetLeft (false);
				temp.i = currentCell.Get_i ();
				temp.j = currentCell.Get_j ();
				temp.wall = 'l';
				DestroyEdges.Enqueue (temp);
				nextCell.SetRight(false);
				temp.i = nextCell.Get_i ();
				temp.j = nextCell.Get_j ();
				temp.wall = 'r';
				DestroyEdges.Enqueue (temp);
				MazeGenerationPath.Enqueue(new pathOfMaze(currentCell.Get_i(),currentCell.Get_j(),nextCell.Get_i(),nextCell.Get_j()));
				Traverse (ref grids [currentCell.Get_i() , currentCell.Get_j()+1]);
			}
		}
	}
	void Findingpath(ref Cell currentCell){
		if (currentCell.Get_i () == target_row - 1 && currentCell.Get_j () == target_col - 1)
			haveReached = true;
		currentCell.SetVisited (true);
		print ("i : " + currentCell.Get_i ());
		print ("j : " + currentCell.Get_j ());
		int []arr= RandomArray();
		for (int i = 0; i < 4; i++) {
			if (arr [i] == 1) {

				FindTop (ref currentCell);
			} else if (arr [i] == 2) {

				FindRight (ref currentCell);
			} else if (arr [i] == 3) {

				FindBottom (ref currentCell);
			} else if (arr [i] == 4) {
			
				FindLeft (ref currentCell);
			}
		}
		if (haveReached) {
			BackTrackPath.Enqueue (new index (currentCell.Get_i (), currentCell.Get_j ()));
			return;
		}
		while (currentCell.GetSteps()!=0) {
			print ("of i : " + currentCell.Get_i () + "   "+"of j : " + currentCell.Get_j () );
			print ("stack length " + BackTrackOnAiWrongPath.Count);
			print (" steps : " + currentCell.GetSteps ());
			currentCell.SetSteps (currentCell.GetSteps () - 1);
			AiPath.Enqueue (BackTrackOnAiWrongPath.Pop ());
		}
	}
	void FindTop(ref Cell currentCell){
		if (!currentCell.GetTop () && !haveReached ) {
			print ("top");
			if (!grids [currentCell.Get_i () - 1, currentCell.Get_j ()].GetVisited ()) {
				grids [currentCell.Get_i()-1 , currentCell.Get_j()].SetSteps (grids [currentCell.Get_i()-1 , currentCell.Get_j()].GetSteps () + 1);
				AiPath.Enqueue (new pathOfMaze (currentCell.Get_i(),currentCell.Get_j(),currentCell.Get_i()-1 , currentCell.Get_j()));
				BackTrackOnAiWrongPath.Push (new pathOfMaze ( currentCell.Get_i () - 1, currentCell.Get_j(),currentCell.Get_i (), currentCell.Get_j ()));
				Findingpath( ref grids [currentCell.Get_i()-1 , currentCell.Get_j()]);
			}
		}
	}
	void FindRight(ref Cell currentCell){
		if (!currentCell.GetRight () && !haveReached ) {
			print ("right");
			if (!grids [currentCell.Get_i (), currentCell.Get_j () - 1].GetVisited ()) {
				grids [currentCell.Get_i() , currentCell.Get_j()-1].SetSteps (grids [currentCell.Get_i() , currentCell.Get_j()-1].GetSteps () + 1);
				AiPath.Enqueue (new pathOfMaze (currentCell.Get_i(),currentCell.Get_j(),currentCell.Get_i() , currentCell.Get_j()-1));
				BackTrackOnAiWrongPath.Push(new pathOfMaze (currentCell.Get_i() , currentCell.Get_j()-1,currentCell.Get_i(),currentCell.Get_j()));
				Findingpath( ref grids [currentCell.Get_i() , currentCell.Get_j()-1]);
			}
		}
	}
	void FindBottom(ref Cell currentCell){
		if (!currentCell.GetBottom () && !haveReached ) {
			print ("bot");
			if (!grids [currentCell.Get_i () + 1, currentCell.Get_j ()].GetVisited ()) {
				grids [currentCell.Get_i()+1 , currentCell.Get_j()].SetSteps (grids [currentCell.Get_i()+1 , currentCell.Get_j()].GetSteps () + 1);
				AiPath.Enqueue (new pathOfMaze (currentCell.Get_i(),currentCell.Get_j(),currentCell.Get_i()+1 , currentCell.Get_j()));
				BackTrackOnAiWrongPath.Push(new pathOfMaze (currentCell.Get_i()+1 , currentCell.Get_j(),currentCell.Get_i(),currentCell.Get_j()));
				Findingpath( ref grids [currentCell.Get_i()+1 , currentCell.Get_j()]);
			}
		}
	}
	void FindLeft(ref Cell currentCell){
		if (!currentCell.GetLeft () && !haveReached ) {
			print ("left");
			if (!grids [currentCell.Get_i (), currentCell.Get_j () + 1].GetVisited ()) {
				grids [currentCell.Get_i() , currentCell.Get_j()+1].SetSteps (grids [currentCell.Get_i() , currentCell.Get_j()+1].GetSteps () + 1);
				AiPath.Enqueue (new pathOfMaze (currentCell.Get_i(),currentCell.Get_j(),currentCell.Get_i() , currentCell.Get_j()+1));
				BackTrackOnAiWrongPath.Push(new pathOfMaze (currentCell.Get_i(),currentCell.Get_j()+1,currentCell.Get_i() , currentCell.Get_j()));
				Findingpath( ref grids [currentCell.Get_i() , currentCell.Get_j()+1]);
			}
		}
	}
	IEnumerator WaitAfterMaze(){
		yield return new WaitForSeconds (3.0f);
		startAi = true;
	}
	IEnumerator WaitAfterPath(){
		yield return new WaitForSeconds (4.0f);
		startBackTracking = true;
	}
	void ResetVisited(){
		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < cols; j++) {
				grids [i, j].SetVisited(false);
			}
		}
	}
	void Clear ()
	{
		GameObject[] lines = GameObject.FindGameObjectsWithTag ("line");
		foreach (GameObject line in lines) {
			Destroy (line);
		}
	}
	public void Reset(){
		SceneManager.LoadScene ("SampleScene");
	}
}
/*
	void DrawCanvas(){
		DrawCanvasWidth ();
		DrawCanvasHight ();
	}
	void DrawCanvasWidth (){
		Color myColor = Color.red;
		for (int i = -1; i < 2; i+=2) {
			Vector3 start = new Vector3 (i*canvasWidth/2, i*canvasHight/2, 0.0f);
			Vector3 end = new Vector3 (-1*i*canvasWidth/2, i*canvasHight/2, 0.0f);
			DrawLine (start,end,myColor);
		}
	}
	void DrawCanvasHight(){
		Color myColor = Color.red;
		for (int i = -1; i < 2; i+=2) {
			Vector3 start = new Vector3 (i*canvasWidth/2, i*canvasHight/2, 0.0f);
			Vector3 end = new Vector3 (i*canvasWidth/2, -i*canvasHight/2, 0.0f);
			DrawLine (start,end,myColor);
		}
	}	*/