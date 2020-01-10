using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Lines : MonoBehaviour {

	public Material lineMaterial;
	public  int canvasWidth;
	public   float cellWidth;
	public  int canvasHight;
	public   float cellHight;
	public GameObject Zombie;
	public GameObject brain;
	public GameObject StartingText;
	public GameObject Finishing1Text;
	public GameObject Finishing2Text;
	public GameObject Choices;
	public GameObject Menu;
    public GameObject PlayModeMenu;
    public GameObject Stop;
    public GameObject Congratz;
	bool apply;
	int AlgorithmType;
	int begain_row;
	int begain_col;
	int target_row;
	int target_col;
	int overLine=0;
    int playerModeRow;
    int playerModeColumn;
	public bool menu;
    public bool playerMode;
	bool haveReached;
	bool activateDfsOnce;
	bool activateDijkstraOnce;
	bool activeAstarOnce;
	bool dijestraProcessFinished;
	bool AstarProcessFinished;
	public int MazeGeneratefps;
	public int dijkstraFps;
	public int DFSFPS;
	int randStartPointRow;
	int randStartPointCol;
	Vector3 startofDrawingMaze;
	Vector3 endofDrawingMaze;
	Vector3 startofDrawingBacktrackSolution;
	Vector3 endofDrawingBacktrackSolution;
	Vector3 StartofDrawingDijkstra;
	bool mazeFinished;
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
	//Queue<pathOfMaze> MazeGenerationPath = new Queue<pathOfMaze> ();
	Queue<pathOfMaze> AiPath = new Queue<pathOfMaze> ();
	Queue<pathOfMaze> DijkstraPathQueue = new Queue<pathOfMaze> ();
	Queue<pathOfMaze> AstarPathQueue = new Queue<pathOfMaze> ();
	Queue<index> BackTrackPath = new Queue<index>();
	Stack<pathOfMaze> BackTrackOnAiWrongPath = new Stack<pathOfMaze>();
	Queue<index> ShortestPath = new Queue<index>();
	Queue <index> AStarShortestpath = new Queue<index>();
	void Awake(){
		QualitySettings.vSyncCount = 0;
		cols = (int) (canvasWidth / cellWidth);
		rows = (int) (canvasHight / cellHight);
		grids= new Cell[rows,cols];
		begain_row = rows;
		begain_col = cols;
		target_row = 1;
		target_col = 1;
		if (begain_row > rows || begain_row < 1)
			begain_row = rows;
		if (target_row > rows || target_row < 1) 
			target_row = 1;
		if (begain_col > cols || begain_col < 1)
			begain_col = cols;
		if (target_col > cols || target_col < 1) 
			target_col = 1;
		AlgorithmType = 0;
		startofDrawingBacktrackSolution = new Vector3 (((canvasWidth / 2) - ((target_col-1) * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - ((target_row-1) * this.cellHight)) - (cellHight / 2), 0.0f);
		StartofDrawingDijkstra=new Vector3 (((canvasWidth / 2) - ((begain_col-1) * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - ((begain_row-1) * this.cellHight)) - (cellHight / 2), 0.0f);
		brain.transform.localScale = new Vector3 (cellWidth, cellHight, 0.0f);
		brain.transform.position=new Vector3 (((canvasWidth / 2) - ((target_col-1) * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - ((target_row-1) * this.cellHight)) - (cellHight / 2), 0.0f);
		Zombie.transform.localScale = new Vector3 (cellWidth, cellHight, 0.0f);
		Zombie.transform.position = new Vector3(((canvasWidth / 2) - ((begain_col-1) * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - ((begain_row-1) * this.cellHight)) - (cellHight / 2), 0.0f);
		brain.SetActive (false);
		Zombie.SetActive (false);
	}
	void Start () {
		apply = false;
		menu = false;
        playerMode = false;
		haveReached = false;
		mazeFinished = false;
		activateDfsOnce = false;
		activateDijkstraOnce = false;
		activeAstarOnce = false;
		dijestraProcessFinished = false;
		AstarProcessFinished = false;
		StartingText.SetActive(false);
		Finishing1Text.SetActive(false);
		Finishing2Text.SetActive(false);
        Congratz.SetActive(false);
		Menu.SetActive (false);
        PlayModeMenu.SetActive(false);
        Choices.SetActive (false);
        Stop.SetActive(false);
        playerModeRow = begain_row-1;
        playerModeColumn = begain_col-1;
		fillCells ();
		randStartPointRow = Random.Range (0, rows);
		randStartPointCol = Random.Range (0, cols);
		Traverse (  grids [randStartPointRow,randStartPointCol]);
	}

	void Update(){
		if (menu) 
			Choices.SetActive (true);
		else
			Choices.SetActive (false);
		
		if (!mazeFinished) {
			Application.targetFrameRate = MazeGeneratefps;
			DrawTheMaze ();
			if (mazeFinished) {
				brain.SetActive (true);
				Zombie.SetActive (true);
				StartingText.gameObject.SetActive (true);
				Menu.SetActive (true);
                PlayModeMenu.SetActive(true);
            }
		}else if (playerMode)
        {
            if(playerModeColumn == target_col-1 && playerModeRow == target_row-1)
            {
                StartCoroutine (Win());
            }
            Vector3 StartPlayerMode = new Vector3(((canvasWidth / 2) - ((playerModeRow ) * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - ((playerModeColumn ) * this.cellHight)) - (cellHight / 2), 0.0f);
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
               
                if (!grids[playerModeColumn, playerModeRow].GetRight())
                {
                    playerModeRow--;
                    Vector3 EndPlayerMode = new Vector3(((canvasWidth / 2) - ((playerModeRow ) * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - ((playerModeColumn ) * this.cellHight)) - (cellHight / 2), 0.0f);
                    DrawLine(StartPlayerMode, EndPlayerMode, Color.cyan, overLine++);
                }
            }
            else if(Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (!grids[playerModeColumn, playerModeRow].GetLeft())
                {
                    playerModeRow++;
                    Vector3 EndPlayerMode = new Vector3(((canvasWidth / 2) - ((playerModeRow ) * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - ((playerModeColumn ) * this.cellHight)) - (cellHight / 2), 0.0f);
                    DrawLine(StartPlayerMode, EndPlayerMode, Color.cyan, overLine++);
                }
            }
            else if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (!grids[playerModeColumn, playerModeRow].GetTop())
                {
                    playerModeColumn--;
                    Vector3 EndPlayerMode = new Vector3(((canvasWidth / 2) - ((playerModeRow ) * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - ((playerModeColumn ) * this.cellHight)) - (cellHight / 2), 0.0f);
                    DrawLine(StartPlayerMode, EndPlayerMode, Color.cyan, overLine++);
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (!grids[playerModeColumn, playerModeRow].GetBottom())
                {
                    playerModeColumn++;
                    Vector3 EndPlayerMode = new Vector3(((canvasWidth / 2) - ((playerModeRow ) * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - ((playerModeColumn ) * this.cellHight)) - (cellHight / 2), 0.0f);
                    DrawLine(StartPlayerMode, EndPlayerMode, Color.cyan, overLine++);
                }
            }

        }
        else if (apply) {
            Menu.SetActive(false);
            PlayModeMenu.SetActive(false);
            if (AlgorithmType == 1) {
				Choices.SetActive (false);
				menu = false;
				if (!activateDijkstraOnce) {
					Application.targetFrameRate = dijkstraFps;
					Clear ();
					ClearText ();
					startofDrawingBacktrackSolution = new Vector3 (((canvasWidth / 2) - ((target_col - 1) * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - ((target_row - 1) * this.cellHight)) - (cellHight / 2), 0.0f);
					resetValuesOfDjkstravVal ();
					activateDijkstraOnce = true;
					DijkstraButton ();
				}
				if (dijestraProcessFinished) {
					if (DijkstraPathQueue.Count != 0) {
						DrawPath (DijkstraPathQueue, Color.yellow);
					} else {
						if (ShortestPath.Count != 0) {
							BackTrackSolution (ShortestPath, Color.red);
						} else {
                            ResetValuesOnUpdate();
						}
					}
				}
			} else if (AlgorithmType == 2) {
				Choices.SetActive (false);
				menu = false;
				if (!activateDfsOnce) {
					Application.targetFrameRate = DFSFPS;
					Clear ();
					ClearText ();
					startofDrawingBacktrackSolution = new Vector3 (((canvasWidth / 2) - ((target_col - 1) * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - ((target_row - 1) * this.cellHight)) - (cellHight / 2), 0.0f);
					activateDfsOnce = true;
					DFSButton ();
				}
				if (haveReached) {
					if (AiPath.Count != 0) {
						DrawPath (AiPath, Color.cyan);
					} else {
						if (BackTrackPath.Count != 0) {
							BackTrackSolution (BackTrackPath, Color.blue);
						} else {
                            ResetValuesOnUpdate();
                        }
					}
				}
			} else if (AlgorithmType == 3) {
				Choices.SetActive (false);
				menu = false;
				if (!activeAstarOnce) {
					Application.targetFrameRate = dijkstraFps;
					Clear ();
					ClearText ();
					startofDrawingBacktrackSolution = new Vector3 (((canvasWidth / 2) - ((target_col - 1) * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - ((target_row - 1) * this.cellHight)) - (cellHight / 2), 0.0f);
					activeAstarOnce = true;
					AstarButton ();
				}
				if (AstarProcessFinished) {
					if (AstarPathQueue.Count != 0) {
						DrawPath (AstarPathQueue, Color.gray);
					} else {
						if (AStarShortestpath.Count != 0) {
							BackTrackSolution (AStarShortestpath, Color.magenta );
						} else {
                            ResetValuesOnUpdate();
                        }
					}
				}
			}
		}
	}
    public void ResetValuesOnUpdate()
    {
        Stop.SetActive(false);
        playerModeRow = begain_row;
        playerModeColumn = begain_col;
        playerMode = false;
        Menu.SetActive(true);
        PlayModeMenu.SetActive(true);
        AlgorithmType = 0;
        apply = false;
        activeAstarOnce = false;
        activateDfsOnce = false;
        activateDijkstraOnce = false;
    }
	void BackTrackSolution(Queue<index> backtrack , Color color){
		if (backtrack.Count != 0) {
			index temp;
			temp = backtrack.Dequeue ();
			endofDrawingBacktrackSolution = new Vector3 (((canvasWidth / 2) - (temp.j * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - (temp.i * this.cellHight)) - (cellHight / 2), 0.0f);
			DrawLine (startofDrawingBacktrackSolution, endofDrawingBacktrackSolution, color, overLine++);
			startofDrawingBacktrackSolution = endofDrawingBacktrackSolution;
		} 
	}
	void DrawPath(Queue<pathOfMaze> mypath,Color color){
		if (mypath.Count != 0) {
			pathOfMaze path;
			path = mypath.Dequeue ();
			startofDrawingMaze = new Vector3 (((canvasWidth / 2) - (path.currentj * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - (path.currenti * this.cellHight)) - (cellHight / 2), 0.0f);
			endofDrawingMaze = new Vector3 (((canvasWidth / 2) - (path.nextj * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - (path.nexti * this.cellHight)) - (cellHight / 2), 0.0f);
			DrawLine (startofDrawingMaze, endofDrawingMaze, color,overLine++);
		}
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
				Cell temp = new Cell(i,j,this.canvasWidth,this.cellWidth,this.canvasHight,this.cellHight,this.begain_row,this.begain_col,this.target_row,this.target_col,lineMaterial);
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
		//lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
		//lr.material = new Material (Shader.Find("Specular"));
		//lr.material = new Material (Shader.Find("Transparent/Diffuse"));
		//lr.material = new Material (Shader.Find("Diffuse"));	
		//lr.material = new Material (Shader.Find("Unlit/Texture"));
		lr.material = lineMaterial;
		lr.startColor = color;
		lr.endColor = color;
		lr.startWidth = 0.1f;
		lr.endWidth = 0.1f;
		lr.SetPosition(0, start);
		lr.SetPosition(1, end);
		Zombie.transform.position = end;
	}
    int [] RandomArray()
    {
        System.Random randnum = new System.Random();
        int[] arr = new int[4];
        HashSet<int> RandomHash = new HashSet<int>();
        while(RandomHash.Count != 4)
        {
            RandomHash.Add(randnum.Next(1, 5));
        }
        RandomHash.CopyTo(arr);
        return arr;
    }
	void Traverse(  Cell currentCell){
		currentCell.SetVisited (true);
		print ("i : " + currentCell.Get_i ());
		print ("j : " + currentCell.Get_j ());
		int []arr= RandomArray();
		for (int i = 0; i < 4; i++) {
			if (arr [i] == 1) {
				TraverseTop (currentCell);
			} else if (arr [i] == 2) {
				TraverseRight (currentCell);
			} else if (arr [i] == 3) {
				TraverseBottom (currentCell);
			} else if (arr [i] == 4) {
				TraverseLeft (  currentCell);
			}
		}
	}
	void TraverseTop(  Cell currentCell){
		if (currentCell.Get_i () > 0) {
			if (!grids [currentCell.Get_i () - 1, currentCell.Get_j ()].GetVisited ()) {
				Cell nextCell = grids [currentCell.Get_i()- 1, currentCell.Get_j()];
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
				//MazeGenerationPath.Enqueue(new pathOfMaze(currentCell.Get_i(),currentCell.Get_j(),nextCell.Get_i(),nextCell.Get_j()));
				Traverse (  grids [currentCell.Get_i()- 1, currentCell.Get_j()]);
			}
		}
	}
	void TraverseBottom(  Cell currentCell){		
		if (currentCell.Get_i () < rows - 1) {
			if (!grids [currentCell.Get_i () + 1, currentCell.Get_j ()].GetVisited ()) {
				Cell nextCell = grids [currentCell.Get_i()+1 , currentCell.Get_j()];
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
				//MazeGenerationPath.Enqueue(new pathOfMaze(currentCell.Get_i(),currentCell.Get_j(),nextCell.Get_i(),nextCell.Get_j()));
				Traverse (  grids [currentCell.Get_i()+1 , currentCell.Get_j()]);
			}
		}
	}
	void TraverseRight(  Cell currentCell){
		if (currentCell.Get_j () > 0) {
			if (!grids [currentCell.Get_i (), currentCell.Get_j () - 1].GetVisited ()) {
				Cell nextCell = grids [currentCell.Get_i() , currentCell.Get_j()-1];
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
				//MazeGenerationPath.Enqueue(new pathOfMaze(currentCell.Get_i(),currentCell.Get_j(),nextCell.Get_i(),nextCell.Get_j()));
				Traverse (  grids [currentCell.Get_i() , currentCell.Get_j()-1]);
			}
		}
	}
	void TraverseLeft(  Cell currentCell){ 
		if (currentCell.Get_j () < cols - 1) {
			if (!grids [currentCell.Get_i (), currentCell.Get_j () + 1].GetVisited ()) {
				Cell nextCell = grids [currentCell.Get_i() , currentCell.Get_j()+1];
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
				//MazeGenerationPath.Enqueue(new pathOfMaze(currentCell.Get_i(),currentCell.Get_j(),nextCell.Get_i(),nextCell.Get_j()));
				Traverse (  grids [currentCell.Get_i() , currentCell.Get_j()+1]);
			}
		}
	}
	void Findingpath(  Cell currentCell){
		if (currentCell.Get_i () == target_row - 1 && currentCell.Get_j () == target_col - 1)
			haveReached = true;
		currentCell.SetVisited (true);
		print ("i : " + currentCell.Get_i ());
		print ("j : " + currentCell.Get_j ());
		int []arr= RandomArray();
		for (int i = 0; i < 4; i++) {
			if (arr [i] == 1) {

				FindTop (  currentCell);
			} else if (arr [i] == 2) {

				FindRight (  currentCell);
			} else if (arr [i] == 3) {

				FindBottom (  currentCell);
			} else if (arr [i] == 4) {
			
				FindLeft (  currentCell);
			}
		}
		if (haveReached) {
			BackTrackPath.Enqueue (new index (currentCell.Get_i (), currentCell.Get_j ()));
			return;
		}
		while (currentCell.GetSteps()!=0) {
			currentCell.SetSteps (currentCell.GetSteps () - 1);
			AiPath.Enqueue (BackTrackOnAiWrongPath.Pop ());
		}
	}
	void FindTop(  Cell currentCell){
		if (!currentCell.GetTop () && !haveReached ) {
			print ("top");
			if (!grids [currentCell.Get_i () - 1, currentCell.Get_j ()].GetVisited ()) {
				grids [currentCell.Get_i()-1 , currentCell.Get_j()].SetSteps (grids [currentCell.Get_i()-1 , currentCell.Get_j()].GetSteps () + 1);
				AiPath.Enqueue (new pathOfMaze (currentCell.Get_i(),currentCell.Get_j(),currentCell.Get_i()-1 , currentCell.Get_j()));
				BackTrackOnAiWrongPath.Push (new pathOfMaze ( currentCell.Get_i () - 1, currentCell.Get_j(),currentCell.Get_i (), currentCell.Get_j ()));
				Findingpath(grids [currentCell.Get_i()-1 , currentCell.Get_j()]);
			}
		}
	}
	void FindRight(  Cell currentCell){
		if (!currentCell.GetRight () && !haveReached ) {
			print ("right");
			if (!grids [currentCell.Get_i (), currentCell.Get_j () - 1].GetVisited ()) {
				grids [currentCell.Get_i() , currentCell.Get_j()-1].SetSteps (grids [currentCell.Get_i() , currentCell.Get_j()-1].GetSteps () + 1);
				AiPath.Enqueue (new pathOfMaze (currentCell.Get_i(),currentCell.Get_j(),currentCell.Get_i() , currentCell.Get_j()-1));
				BackTrackOnAiWrongPath.Push(new pathOfMaze (currentCell.Get_i() , currentCell.Get_j()-1,currentCell.Get_i(),currentCell.Get_j()));
				Findingpath(   grids [currentCell.Get_i() , currentCell.Get_j()-1]);
			}
		}
	}
	void FindBottom(  Cell currentCell){
		if (!currentCell.GetBottom () && !haveReached ) {
			print ("bot");
			if (!grids [currentCell.Get_i () + 1, currentCell.Get_j ()].GetVisited ()) {
				grids [currentCell.Get_i()+1 , currentCell.Get_j()].SetSteps (grids [currentCell.Get_i()+1 , currentCell.Get_j()].GetSteps () + 1);
				AiPath.Enqueue (new pathOfMaze (currentCell.Get_i(),currentCell.Get_j(),currentCell.Get_i()+1 , currentCell.Get_j()));
				BackTrackOnAiWrongPath.Push(new pathOfMaze (currentCell.Get_i()+1 , currentCell.Get_j(),currentCell.Get_i(),currentCell.Get_j()));
				Findingpath(   grids [currentCell.Get_i()+1 , currentCell.Get_j()]);
			}
		}
	}
	void FindLeft(  Cell currentCell){
		if (!currentCell.GetLeft () && !haveReached ) {
			print ("left");
			if (!grids [currentCell.Get_i (), currentCell.Get_j () + 1].GetVisited ()) {
				grids [currentCell.Get_i() , currentCell.Get_j()+1].SetSteps (grids [currentCell.Get_i() , currentCell.Get_j()+1].GetSteps () + 1);
				AiPath.Enqueue (new pathOfMaze (currentCell.Get_i(),currentCell.Get_j(),currentCell.Get_i() , currentCell.Get_j()+1));
				BackTrackOnAiWrongPath.Push(new pathOfMaze (currentCell.Get_i(),currentCell.Get_j()+1,currentCell.Get_i() , currentCell.Get_j()));
				Findingpath(   grids [currentCell.Get_i() , currentCell.Get_j()+1]);
			}
		}
	}
	IEnumerator WaitAfterMaze(){
		yield return new WaitForSeconds (3.0f);
		//startAi = true;
	}
	IEnumerator WaitAfterPath(){
		yield return new WaitForSeconds (4.0f);
		//startBackTracking = true;
	}
	void DijkstraAlgorithm (Cell start , Cell end){
		List<Cell> Unvisited = new List<Cell> ();
		//List<Cell> Visited = new List<Cell> ();
		Unvisited = fillDijkstra();
		Cell CheckedCell = start;
		start.dijkstraValue = 0;
		while (CheckedCell!= end) {
			//Visited.Add (CheckedCell);
			DijkstraCheckNeighbours (CheckedCell);
			print ("count "+Unvisited.Count);
			print ("removed  i " + CheckedCell.Get_i () +"  j " + CheckedCell.Get_j ()+ "  short  " + CheckedCell.dijkstraValue );
			Unvisited.Remove (CheckedCell);
			Unvisited.Sort ((x, y) => x.dijkstraValue.CompareTo (y.dijkstraValue));
			print ("now " + Unvisited[0].Get_i () +"  j " + Unvisited[0].Get_j () + "  short  " +  Unvisited[0].dijkstraValue);
			CheckedCell = Unvisited[0];
		}
		DijkstraPath(end);
	}
	void DijkstraCheckNeighbours(Cell parent){
		if(!parent.GetLeft())
			LefDijkstra (parent);
		if(!parent.GetTop())
			TopDijkstra (parent);
		if(!parent.GetRight())
			RightDijkstra (parent);
		if(!parent.GetBottom())
			BottomDijkstra (parent);
	}
	void LefDijkstra(Cell parent){
		Cell leftCell=grids[parent.Get_i(),parent.Get_j()+1];
		pathOfMaze temp = new pathOfMaze (parent.Get_i (), parent.Get_j (), leftCell.Get_i (), leftCell.Get_j ());
		DijkstraPathQueue.Enqueue (temp);
			if(parent.dijkstraValue+1< leftCell.dijkstraValue)
			{
				leftCell.dijkstraValue = parent.dijkstraValue+1;
				leftCell.dijkstraPrevious = parent;
			}
	}
	void TopDijkstra(Cell parent){
		Cell TopCell=grids [parent.Get_i()-1 , parent.Get_j()];
		pathOfMaze temp = new pathOfMaze (parent.Get_i (), parent.Get_j (), TopCell.Get_i (), TopCell.Get_j ());
		DijkstraPathQueue.Enqueue (temp);
		if(parent.dijkstraValue+1< TopCell.dijkstraValue)
		{
			TopCell.dijkstraValue = parent.dijkstraValue+1;
			TopCell.dijkstraPrevious=parent;
		}
	}
	void RightDijkstra(Cell parent){
		Cell RightCell=grids[parent.Get_i(),parent.Get_j()-1];
		pathOfMaze temp = new pathOfMaze (parent.Get_i (), parent.Get_j (), RightCell.Get_i (), RightCell.Get_j ());
		DijkstraPathQueue.Enqueue (temp);
		if(parent.dijkstraValue+1< RightCell.dijkstraValue)
		{
			RightCell.dijkstraValue = parent.dijkstraValue+1;
			RightCell.dijkstraPrevious=parent;
		}
	}
	void BottomDijkstra(Cell parent){
		Cell BottomCell=grids [parent.Get_i()+1 , parent.Get_j()];
		pathOfMaze temp = new pathOfMaze (parent.Get_i (), parent.Get_j (), BottomCell.Get_i (), BottomCell.Get_j ());
		DijkstraPathQueue.Enqueue (temp);
		if(parent.dijkstraValue+1< BottomCell.dijkstraValue)
		{
			BottomCell.dijkstraValue = parent.dijkstraValue+1;
			BottomCell.dijkstraPrevious=parent;
		}
	}
	void DijkstraPath(Cell grapParent){
		index temp = new index(grapParent.Get_i (), grapParent.Get_j ());
		ShortestPath.Enqueue (temp);
		if (grapParent.dijkstraPrevious != null) {
			DijkstraPath (grapParent.dijkstraPrevious);
		} else {
			dijestraProcessFinished = true;
		}
	}
	List<Cell> fillDijkstra(){
		List<Cell> filling = new List<Cell> ();
		
		for (int i = 0; i < rows; i++)
			for (int j = 0; j < cols; j++)
				filling.Add (grids [i, j]);
		return filling;
	}
	void AStarAlgorithm (Cell start , Cell end){
		start.Astar_G = 0;
		
		List<Cell> OpenedSet = new List<Cell> ();
		Cell checkCell = start;
		OpenedSet.Add (checkCell);
		checkCell.Update_Astar_F ();
		while (checkCell != end) {
			checkCell.SetVisited (true);
			OpenedSet.Remove (checkCell);
			AstarCheckNeighbours (checkCell,OpenedSet);
			OpenedSet.Sort ((x, y) => x.Astar_F.CompareTo (y.Astar_F));
			checkCell = OpenedSet [0];
		}
		AstarPathbacktracking (end);
	}
	void AstarPathbacktracking(Cell parentCell){
		index temp = new index(parentCell.Get_i (), parentCell.Get_j ());
		AStarShortestpath.Enqueue (temp);
		if (parentCell.Astar_Parent != null) {
			AstarPathbacktracking (parentCell.Astar_Parent);
		} else {
			AstarProcessFinished = true;
		}
	}
	void AstarCheckNeighbours(Cell currentCell,List<Cell> openedSet){
		if (!currentCell.GetRight ()) {
			AstarCheckRight (currentCell, openedSet);
		}
		if (!currentCell.GetLeft ()) {
			AstarCheckLeft (currentCell, openedSet);
		}
		if (!currentCell.GetTop ()) {
			AstarCheckTop (currentCell, openedSet);
		}if (!currentCell.GetBottom ()) {
			AstarCheckBottom (currentCell, openedSet);
		}

	}
	void AstarCheckRight(Cell currentCell , List<Cell> openedSet){
		Cell rightCell = grids [currentCell.Get_i (), currentCell.Get_j()-1];
		if (!rightCell.GetVisited ()) {
			pathOfMaze temp = new pathOfMaze (currentCell.Get_i (), currentCell.Get_j (), rightCell.Get_i (), rightCell.Get_j ());
			AstarPathQueue.Enqueue (temp);
			openedSet.Add (rightCell);
			rightCell.Astar_G = currentCell.Astar_G + 1;
			rightCell.Update_Astar_F ();
			rightCell.Astar_Parent = currentCell;
		}
	}
	void AstarCheckLeft(Cell currentCell , List<Cell> openedSet){
		Cell LeftCell = grids [currentCell.Get_i (), currentCell.Get_j ()+1];
		if (!LeftCell.GetVisited ()) {
			pathOfMaze temp = new pathOfMaze (currentCell.Get_i (), currentCell.Get_j (), LeftCell.Get_i (), LeftCell.Get_j ());
			AstarPathQueue.Enqueue (temp);
			openedSet.Add (LeftCell);
			LeftCell.Astar_G = currentCell.Astar_G + 1;
			LeftCell.Update_Astar_F ();
			LeftCell.Astar_Parent = currentCell;
		}
	}
	void AstarCheckTop(Cell currentCell , List<Cell> openedSet){
		Cell TopCell = grids [currentCell.Get_i ()-1, currentCell.Get_j()];
		if (!TopCell.GetVisited ()) {
			pathOfMaze temp = new pathOfMaze (currentCell.Get_i (), currentCell.Get_j (), TopCell.Get_i (), TopCell.Get_j ());
			AstarPathQueue.Enqueue (temp);
			openedSet.Add (TopCell);
			TopCell.Astar_G = currentCell.Astar_G + 1;
			TopCell.Update_Astar_F ();
			TopCell.Astar_Parent = currentCell;
		}
	}
	void AstarCheckBottom(Cell currentCell , List<Cell> openedSet){
		Cell BottomCell = grids [currentCell.Get_i ()+1, currentCell.Get_j()];
		if (!BottomCell.GetVisited ()) {
			pathOfMaze temp = new pathOfMaze (currentCell.Get_i (), currentCell.Get_j (), BottomCell.Get_i (), BottomCell.Get_j ());
			AstarPathQueue.Enqueue (temp);
			openedSet.Add (BottomCell);
			BottomCell.Astar_G = currentCell.Astar_G + 1;
			BottomCell.Update_Astar_F ();
			BottomCell.Astar_Parent = currentCell;
		}
	}
	void ResetVisited(){
		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < cols; j++) {
				grids [i, j].SetVisited(false);
			}
		}
	}
	public void DijkstraButton(){
		DijkstraAlgorithm (grids[begain_row-1,begain_col-1],grids[target_row-1,target_col-1]);
		StartofDrawingDijkstra=new Vector3 (((canvasWidth / 2) - ((begain_col-1) * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - ((begain_row-1) * this.cellHight)) - (cellHight / 2), 0.0f);
	}
	public void DFSButton(){
		ResetVisited ();
		AiPath.Clear ();
		haveReached = false;
		Findingpath(  grids [begain_row-1,begain_col-1]);
	}
	public void AstarButton(){
		ResetVisited ();
		AStarAlgorithm(grids[begain_row-1,begain_col-1],grids[target_row-1,target_col-1]);
		StartofDrawingDijkstra=new Vector3 (((canvasWidth / 2) - ((begain_col-1) * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - ((begain_row-1) * this.cellHight)) - (cellHight / 2), 0.0f);
	}
	public void SetAlgorithm(int x){
		AlgorithmType = x;
	}
	void Clear ()
	{
		GameObject[] lines = GameObject.FindGameObjectsWithTag ("line");
		foreach (GameObject line in lines) {
			Destroy (line);
		}
	}
	void ClearText(){
		StartingText.SetActive(false);
		Finishing1Text.SetActive(false);
		Finishing2Text.SetActive(false);
	}
	public void Confirm(){
		apply = true;
	}
	public void ChangeMenu(){
		if (menu)
			menu = false;
		else
			menu = true;
	}
	void resetValuesOfDjkstravVal(){
		for (int i = 0; i < rows; i++)
			for (int j = 0; j < cols; j++) {
				grids [i, j].dijkstraValue = int.MaxValue;
			}
	}
	public void Reset(){
		SceneManager.LoadScene ("SampleScene");
	}
    public void  setPlayerMode(bool x)
    {
        playerMode = x;
        Stop.SetActive(x);
        playerModeRow = begain_row-1  ;
        playerModeColumn = begain_col -1;
        Menu.SetActive(!x);
        PlayModeMenu.SetActive(!x);
        if (x)
        {
            Zombie.transform.position= new Vector3(((canvasWidth / 2) - ((begain_col-1) * cellWidth)) - (cellWidth / 2), ((this.canvasHight / 2) - ((begain_row-1) * this.cellHight)) - (cellHight / 2), 0.0f);
            Clear();
            StartingText.SetActive(false);
        }
    }
    IEnumerator Win()
    {
        Congratz.SetActive(true);
        brain.SetActive(false);
        playerMode = false;
        yield return new WaitForSeconds(2);
        Congratz.SetActive(false);
        brain.SetActive(true);
        setPlayerMode(false);
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
	/*int[] RandomArray(){
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
	}*/