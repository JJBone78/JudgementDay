using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;

[ExecuteInEditMode]
public class Grid : MonoBehaviour//, IGrid 
{
	//Singleton
	public static Grid main;
	
	//Member variables
	private static bool _show_grid = false;	
	private static bool _show_open_tiles = true;
	private static bool _show_closed_tiles = true;
	private static bool _show_bridge_tiles = true;
	private static bool _show_tunnel_tiles = true;
	
	private static float _tile_size = 7.5f;
	private static int _width = 133;//200;
	private static int _length = 133;//200;
	private static float _width_offset = 0;//400; //region on the start of the left of the map to ignore move commands
	private static float _length_offset = 0;//400; //region on the start of the bottom of the map to ignore move commands
    private static float _max_steepness = 2.5f;
	private static float _passable_height = 2.0f;
	private static int _block_indent = 0;//5; //border size around the map to ignore the move commands
	
	private static Tile[,] _grid;
	
	private static readonly List<Vector3> _debug_algorithm = new List<Vector3>();
	
	//Properties
	public static bool ShowGrid
	{
		get
		{
			return _show_grid;
		}
		set
		{
			if (_grid == null)
			{
				Initialise ();
			}
			
			_show_grid = value;
		}
	}
	
	public static bool ShowOpenTiles
	{
		get
		{
			return _show_open_tiles;
		}
		set
		{
			_show_open_tiles = value;
		}
	}
	
	public static bool ShowClosedTiles
	{
		get
		{
			return _show_closed_tiles;
		}
		set
		{
			_show_closed_tiles = value;
		}
	}
	
	public static bool ShowBridgeTiles
	{
		get
		{
			return _show_bridge_tiles;
		}
		set
		{
			_show_bridge_tiles = value;
		}
	}
	
	public static bool ShowTunnelTiles
	{
		get
		{
			return _show_tunnel_tiles;
		}
		set
		{
			_show_tunnel_tiles = value;
		}
	}
	
	public static float TileSize
	{
		get
		{
			return _tile_size;
		}
		set
		{
			if (Equals (_tile_size, value))
			{
				return;
			}
			
			_tile_size = value;
			
			Initialise ();
		}
	}
	
	public static int Width
	{
		get
		{
			return _width;
		}
		set
		{
			if (Equals (_width, value))
			{
				return;
			}
		
			_width = value;
			
			Initialise ();
		}
	}
	
	public static int Length
	{
		get
		{
			return _length;
		}
		set
		{
			if (Equals (_length, value))
			{
				return;
			}
			
			_length = value;
			
			Initialise ();
		}
	}
	
	public static float WidthOffset
	{
		get
		{
			return _width_offset;
		}
		set
		{
			if (Equals (_width_offset, value))
			{
				return;
			}
			
			_width_offset = value;
			
			Initialise ();
		}
	}
	
	public static float LengthOffset
	{
		get
		{
			return _length_offset;
		}
		set
		{
			if (Equals (_length_offset, value))
			{
				return;
			}
			
			_length_offset = value;
			
			Initialise ();
		}
	}
	
	public static float MaxSteepness
	{
		get
		{
			return _max_steepness;
		}
		set
		{
			if (Equals (_max_steepness, value))
			{
				return;
			}
			
			_max_steepness = value;
			
			Initialise ();
		}
	}
	
	public static float PassableHeight
	{
		get
		{
			return _passable_height;
		}
		set
		{
			if (Equals (_passable_height, value))
			{
				return;
			}
			
			_passable_height = value;
			
			Initialise ();
		}
	}
	
	public static int BlockIndent
	{
		get
		{
			return _block_indent;
		}
		set
		{
			if (Equals (_block_indent, value))
			{
				return;
			}
			
			if (value > 0)
			{
				_block_indent = value;
			}
			else
			{
				_block_indent = 1;
			}
			
			Initialise ();
		}
	}
	
	void Awake()
	{
		main = this;
	}
	
	void Start()
	{
		if (Application.isPlaying)
		{
			StartCoroutine (InitialiseAsRoutine ());
		}
	}
	
	void Update()
	{
		
	}
	
	void OnDrawGizmos()
	{
		if (_show_grid && Application.isEditor && _grid != null)
		{
			foreach (Tile tile in _grid)
			{
				if (tile.Status == Const.TILE_Blocked) 
				{
					if (ShowClosedTiles)
					{
						Gizmos.color = Color.red;
						Gizmos.DrawWireCube (tile.Center, new Vector3(_tile_size, 0.1f, _tile_size));
					}
				}
				else
				{
					if (ShowOpenTiles)
					{
						Gizmos.color = Color.white;
						Gizmos.DrawWireCube (tile.Center, new Vector3(_tile_size, 0.1f, _tile_size));
					}
				}
				
				
				foreach (Tile t in tile.LayeredTiles)
				{
					if (t.IsBridge)
					{
						if (ShowBridgeTiles)
						{
							Gizmos.color = Color.green;
							Gizmos.DrawWireCube (t.Center, new Vector3(_tile_size, 0.1f, _tile_size));
						}
					}
					else
					{
						if (ShowTunnelTiles)
						{
							Gizmos.color = Color.cyan;
							Gizmos.DrawWireCube (t.Center, new Vector3(_tile_size, 0.1f, _tile_size));
						}
					}
				}
			}
		}
		
		for (int i = 1; i < _debug_algorithm.Count; i++)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine (_debug_algorithm[i-1], _debug_algorithm[i]);
		}
	}
	
	public static IEnumerator InitialiseAsRoutine()
	{
		ILevelLoader levelLoader = ManagerResolver.Resolve<ILevelLoader>();
		
		_grid = new Tile[Width, Length];
		
		//Create tiles
		for (int i = 0; i < Width; i++)
		{
            for (int j = 0; j < Length; j++)
			{
                float xCenter = _width_offset + ((i * _tile_size) + (_tile_size / 2.0f)); //m_WidthOffset = 400
                float zCenter = _length_offset + ((j * _tile_size) + (_tile_size / 2.0f)); //m_TileSize = 7.5f;
                Vector3 center = new Vector3(xCenter, 0, zCenter);               //Width = 200
                center.y = Terrain.activeTerrain.SampleHeight(center);
				
				_grid[i,j] = new Tile(i, j, center);
			}
		}
		
		if (levelLoader != null) levelLoader.ChangeText ("Evaluating tiles");
		yield return null;
		
		List<Collider> bridgeList = new List<Collider>();
		List<Collider> tunnelList = new List<Collider>();
		
		//Evaluate
		for (int i=0; i<Width; i++)
		{
			for (int j=0; j<Length; j++)
			{
				_grid[i,j].Evaluate (bridgeList, tunnelList);
			}
		}
		
		if (levelLoader != null) levelLoader.ChangeText ("Evaluating bridges");
		yield return null;
		
		//Create the bridges
		foreach (Collider collider in bridgeList)
		{
			BuildBridge(collider);
		}
		
		if (levelLoader != null) levelLoader.ChangeText ("Evaluating Tunnels");
		yield return null;
		
		//Create the tunnels
		foreach (Collider collider in tunnelList)
		{
			BuildTunnel(collider);
		}
		
		if (levelLoader != null) levelLoader.ChangeText ("Populating internal array");
		yield return null;
		
		//Now all the tiles have been initialised, we need to populate the tiles internal array with accessible tiles
		for (int i=0; i<Width; i++)
		{
			for (int j=0; j<Length; j++)
			{
				FindAccessibleTiles (_grid[i,j]);
			}
		}
		
		if (levelLoader != null) levelLoader.FinishLoading ();
		ManagerResolver.Resolve<ICursorManager>().ShowCursor ();
	}
	
	public static void Initialise()
	{
		_grid = new Tile[Width, Length];
		
		//Create tiles
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Length; j++)
			{
                float xCenter = _width_offset + ((i * _tile_size) + (_tile_size / 2.0f));
                float zCenter = _length_offset + ((j * _tile_size) + (_tile_size / 2.0f));
                Vector3 center = new Vector3(xCenter, 0, zCenter);
                center.y = Terrain.activeTerrain.SampleHeight(center);
				
				_grid[i,j] = new Tile(i, j, center);
			}
		}
		
		List<Collider> bridgeList = new List<Collider>();
		List<Collider> tunnelList = new List<Collider>();
		
		//Evaluate
		for (int i=0; i<Width; i++)
		{
			for (int j=0; j<Length; j++)
			{
				_grid[i,j].Evaluate (bridgeList, tunnelList);
			}
		}
		
		//Create the bridges
		foreach (Collider collider in bridgeList)
		{
			BuildBridge(collider);
		}
		
		//Create the tunnels
		foreach (Collider collider in tunnelList)
		{
			BuildTunnel(collider);
		}
	}
	
	public static Tile GetClosestTile(Vector3 position)
	{
		int iValue = (int)((position.x - _width_offset)/_tile_size);
		int jValue = (int)((position.z - _length_offset)/_tile_size);
		
		if (iValue < 0) iValue = 0;
		else if (iValue >= Width) iValue = Width-1;
		
		if (jValue < 0) jValue = 0;
		else if (jValue >= Length) jValue = Length-1;
		
		Tile tileToReturn = _grid[iValue, jValue];
		
		float distance = Mathf.Abs (tileToReturn.Center.y - position.y);
		Tile lTile = null;
		foreach (Tile tile in tileToReturn.LayeredTiles)
		{
			if (Mathf.Abs (tile.Center.y - position.y) < distance)
			{
				lTile = tile;
				distance = Mathf.Abs (tile.Center.y - position.y);
			}
		}
		
		if (lTile != null)
		{
			tileToReturn = lTile;
		}
		
		return tileToReturn;
	}
	
	public static Tile GetClosestAvailableTile(Vector3 position)
	{
		_debug_algorithm.Clear ();
		int iValue = (int)((position.x - _width_offset)/_tile_size);
		int jValue = (int)((position.z - _length_offset)/_tile_size);
		
		if (iValue < 0) iValue = 0;
		else if (iValue >= Width) iValue = Width-1;
		
		if (jValue < 0) jValue = 0;
		else if (jValue >= Length) jValue = Length-1;
		
		Tile tileToReturn = _grid[iValue, jValue];
		
		if (tileToReturn.LayeredTiles.Count > 0)
		{
			if (Mathf.Abs (tileToReturn.Center.y - position.y) > Mathf.Abs (tileToReturn.LayeredTiles[0].Center.y - position.y))
			{
				tileToReturn = tileToReturn.LayeredTiles[0];
			}
		}
		
		if (tileToReturn.Status == Const.TILE_Blocked)
		{
			//Need to iterate to find closest available tile
			int directionCounter = Const.DIRECTION_Right;
			int widthCounter = 1;
			int lengthCounter = 1;
			int IValue = tileToReturn.I;
			int JValue = tileToReturn.J;
			
		    while (tileToReturn.Status == Const.TILE_Blocked)
			{
				int counter;
				
				//If we're travelling left or right use the width counter, up or down use the length counter
				if (directionCounter == Const.DIRECTION_Right || directionCounter == Const.DIRECTION_Left)
				{
					counter = widthCounter;
				}
				else
				{
					counter = lengthCounter;
				}
				
				for (int i=0; i<counter; i++)
				{
					switch (directionCounter)
					{
					case Const.DIRECTION_Right:
						//Increase I value (go right)
						IValue++;
						
						//Check if we're at the width so we don't get an exception
						if (IValue >= Width)
						{
							//We're past the width, decrease I value
							IValue = Width-1;
							
							//Set JValue to whatever it is minus lengthcounter (no point checking tiles we've already checked!)
							JValue = JValue - lengthCounter;
							
							//Since we've skipped all the downward tiles, go left
							directionCounter = Const.DIRECTION_Left;
							
							//Update the length counter as we're skipping it out
							lengthCounter++;
						}
						else
						{
							//Have we travelled far enough?
							if (i == widthCounter - 1)
							{
								//We've travelled as far as we want to, change the direction and increase the width counter
								directionCounter = Const.DIRECTION_Down;
								widthCounter++;
							}
						}
						
						break;
						
					case Const.DIRECTION_Down:
						
						JValue--;
						if (JValue < 0)
						{
							JValue = 0;
							
							IValue = IValue - widthCounter;
							
							directionCounter = Const.DIRECTION_Up;
							
							widthCounter++;
						}
						else
						{
							if (i == lengthCounter - 1)
							{
								directionCounter = Const.DIRECTION_Left;
								lengthCounter++;
							}
						}
						
						break;
						
					case Const.DIRECTION_Left:
						
						IValue--;
						if (IValue < 0)
						{
							IValue = 0;
							
							JValue = JValue + lengthCounter;
							
							directionCounter = Const.DIRECTION_Right;
							
							lengthCounter++;
						}
						else
						{
							if (i == widthCounter - 1)
							{
								directionCounter = Const.DIRECTION_Up;
								widthCounter++;
							}
						}
						
						break;
						
					case Const.DIRECTION_Up:
						
						JValue++;
						if (JValue >= Length)
						{
							JValue = Length-1;
							
							IValue = IValue + widthCounter;
							
							directionCounter = Const.DIRECTION_Down;
							
							widthCounter++;
						}
						else
						{
							if (i == lengthCounter - 1)
							{
								directionCounter = Const.DIRECTION_Right;
								lengthCounter++;
							}
						}
						
						break;
					}
					
					tileToReturn = _grid[IValue, JValue];
					_debug_algorithm.Add (tileToReturn.Center);
				}
			}
		}
		
		return tileToReturn;
	}
	
	public static Tile GetClosestAvailableFreeTile(Vector3 position)
	{
		int iValue = (int)((position.x - _width_offset)/_tile_size);
		int jValue = (int)((position.z - _length_offset)/_tile_size);
		
		if (iValue < 0) iValue = 0;
		else if (iValue >= Width) iValue = Width-1;
		
		if (jValue < 0) jValue = 0;
		else if (jValue >= Length) jValue = Length-1;
		
		Tile tileToReturn = _grid[iValue, jValue];
		
		float yVal = Mathf.Abs (tileToReturn.Center.y - position.y);
		foreach (Tile tile in tileToReturn.LayeredTiles)
		{
			if (Mathf.Abs (tile.Center.y - position.y) < yVal)
			{
				yVal = Mathf.Abs (tile.Center.y - position.y);
				tileToReturn = tile;
			}
		}
		
		if (tileToReturn.Status == Const.TILE_Blocked || tileToReturn.ExpectingArrival)
		{
			//Need to iterate to find closest available tile
			int directionCounter = Const.DIRECTION_Right;
			int widthCounter = 1;
			int lengthCounter = 1;
			int IValue = tileToReturn.I;
			int JValue = tileToReturn.J;
			
		    while (tileToReturn.Status == Const.TILE_Blocked || tileToReturn.ExpectingArrival)
			{
				int counter;
				
				//If we're travelling left or right use the width counter, up or down use the length counter
				if (directionCounter == Const.DIRECTION_Right || directionCounter == Const.DIRECTION_Left)
				{
					counter = widthCounter;
				}
				else
				{
					counter = lengthCounter;
				}
				
				for (int i=0; i<counter; i++)
				{
					switch (directionCounter)
					{
					case Const.DIRECTION_Right:
						//Increase I value (go right)
						IValue++;
						
						//Check if we're at the width so we don't get an exception
						if (IValue >= Width)
						{
							//We're past the width, decrease I value
							IValue = Width-1;
							
							//Set JValue to whatever it is minus lengthcounter (no point checking tiles we've already checked!)
							JValue = JValue - lengthCounter;
							
							//Since we've skipped all the downward tiles, go left
							directionCounter = Const.DIRECTION_Left;
							
							//Update the length counter as we're skipping it out
							lengthCounter++;
						}
						else
						{
							//Have we travelled far enough?
							if (i == widthCounter - 1)
							{
								//We've travelled as far as we want to, change the direction and increase the width counter
								directionCounter = Const.DIRECTION_Down;
								widthCounter++;
							}
						}
						
						break;
						
					case Const.DIRECTION_Down:
						
						JValue--;
						if (JValue < 0)
						{
							JValue = 0;
							
							IValue = IValue - widthCounter;
							
							directionCounter = Const.DIRECTION_Up;
							
							widthCounter++;
						}
						else
						{
							if (i == lengthCounter - 1)
							{
								directionCounter = Const.DIRECTION_Left;
								lengthCounter++;
							}
						}
						
						break;
						
					case Const.DIRECTION_Left:
						
						IValue--;
						if (IValue < 0)
						{
							IValue = 0;
							
							JValue = JValue + lengthCounter;
							
							directionCounter = Const.DIRECTION_Right;
							
							lengthCounter++;
						}
						else
						{
							if (i == widthCounter - 1)
							{
								directionCounter = Const.DIRECTION_Up;
								widthCounter++;
							}
						}
						
						break;
						
					case Const.DIRECTION_Up:
						
						JValue++;
						if (JValue >= Length)
						{
							JValue = Length-1;
							
							IValue = IValue + widthCounter;
							
							directionCounter = Const.DIRECTION_Down;
							
							widthCounter++;
						}
						else
						{
							if (i == lengthCounter - 1)
							{
								directionCounter = Const.DIRECTION_Right;
								lengthCounter++;
							}
						}
						
						break;
					}
					
					tileToReturn = _grid[IValue, JValue];
					
					yVal = Mathf.Abs (tileToReturn.Center.y - position.y);
					foreach (Tile tile in tileToReturn.LayeredTiles)
					{
						if (Mathf.Abs (tile.Center.y - position.y) < yVal)
						{
							yVal = Mathf.Abs (tile.Center.y - position.y);
							tileToReturn = tile;
						}
					}
				}
			}
		}
		
		return tileToReturn;
	}
	
	public static Tile GetClosestArrivalTile(Vector3 position)
	{
		int iValue = (int)((position.x - _width_offset)/_tile_size);
		int jValue = (int)((position.z - _length_offset)/_tile_size);
		
		if (iValue < 0) iValue = 0;
		else if (iValue >= Width) iValue = Width-1;
		
		if (jValue < 0) jValue = 0;
		else if (jValue >= Length) jValue = Length-1;
		
		Tile tileToReturn = _grid[iValue, jValue];
		
		float yVal = Mathf.Abs (tileToReturn.Center.y - position.y);
		foreach (Tile tile in tileToReturn.LayeredTiles)
		{
			if (Mathf.Abs (tile.Center.y - position.y) < yVal)
			{
				yVal = Mathf.Abs (tile.Center.y - position.y);
				tileToReturn = tile;
			}
		}
		
		if (tileToReturn.Status == Const.TILE_Blocked || tileToReturn.ExpectingArrival)
		{
			Queue<Tile> tilesToCheck = new Queue<Tile>();
			tilesToCheck.Enqueue (tileToReturn);
			
			while (tilesToCheck.Count > 0)
			{
				tileToReturn = tilesToCheck.Dequeue ();
				
				foreach (Tile tile in tileToReturn.AccessibleTiles)
				{
					if (tile.Status != Const.TILE_Blocked && !tile.ExpectingArrival)
					{
						return tile;
					}
					
					if (!tilesToCheck.Contains (tile))
					{
						tilesToCheck.Enqueue (tile);
					}
				}
			}
			
			tileToReturn = null;
		}
		
		return tileToReturn;
	}
	
	public static void FindAccessibleTiles(Tile tile)
	{
		//Need to find which tiles this tile can travel to
		try
		{
		    Tile topLeft;
		    Tile topRight;
		    Tile bottomRight;
		    Tile bottomLeft;
            Tile tileLeft = tile.I != 0 ? _grid[tile.I-1, tile.J] : _grid[tile.I, tile.J];
            Tile tileRight = tile.I < _grid.GetUpperBound(0) ? _grid[tile.I+1, tile.J] : _grid[tile.I, tile.J];
            Tile tileUp = tile.J < _grid.GetUpperBound(1) ? _grid[tile.I, tile.J+1] : _grid[tile.I, tile.J];
            Tile tileDown = tile.J != 0 ? _grid[tile.I, tile.J-1] : _grid[tile.I, tile.J];

            if (tile.I != 0 && tile.J < _grid.GetUpperBound(1))
			    topLeft = _grid[tile.I-1, tile.J+1];
            else
                topLeft = _grid[tile.I, tile.J];

            if (tile.I < _grid.GetUpperBound(0) && tile.J < _grid.GetUpperBound(1))
			    topRight = _grid[tile.I+1, tile.J+1];
            else
                topRight = _grid[tile.I, tile.J];

            if (tile.I < _grid.GetUpperBound(0) && tile.J != 0)
			    bottomRight = _grid[tile.I+1, tile.J-1];
            else
                bottomRight = _grid[tile.I, tile.J];

            if (tile.I != 0 && tile.J != 0)
			    bottomLeft = _grid[tile.I-1, tile.J-1];
			else
                bottomLeft = _grid[tile.I, tile.J];
            CheckTileConnection (tile, tileLeft);
			CheckTileConnection (tile, tileRight);
			CheckTileConnection (tile, tileUp);
			CheckTileConnection (tile, tileDown);
			CheckTileConnection (tile, topLeft);
			CheckTileConnection (tile, topRight);
			CheckTileConnection (tile, bottomRight);
			CheckTileConnection (tile, bottomLeft);
		}
		catch 
		{
			//Silently ignored, bad programmer! ;P
		}
	}
	
	private static void CheckTileConnection(Tile currentTile, Tile tileToCheck)
	{
		float acceptableHeightDiff = 2.5f;
		if (tileToCheck.Status == Const.TILE_Unvisited && Mathf.Abs (tileToCheck.Center.y - currentTile.Center.y) < acceptableHeightDiff)
		{
			currentTile.AccessibleTiles.Add (tileToCheck);
		}
		
		//Check if any layered tiles are accessible
		foreach (Tile layeredTile in tileToCheck.LayeredTiles)
		{
			if (Mathf.Abs (currentTile.Center.y - layeredTile.Center.y) < acceptableHeightDiff && layeredTile.BridgeTunnelEntrance)
			{
				currentTile.AccessibleTiles.Add (layeredTile);
			}
		}
		
		//Check if the current tile's layered tiles can access any of the other tiles
		foreach (Tile currentLayeredTile in currentTile.LayeredTiles)
		{
			if (Mathf.Abs (currentLayeredTile.Center.y - tileToCheck.Center.y) < acceptableHeightDiff && tileToCheck.Status == Const.TILE_Unvisited && currentLayeredTile.BridgeTunnelEntrance)
			{
				currentLayeredTile.AccessibleTiles.Add (tileToCheck);
			}
			
			foreach (Tile layeredTile in tileToCheck.LayeredTiles)
			{
				if (Mathf.Abs (currentLayeredTile.Center.y - layeredTile.Center.y) < acceptableHeightDiff)
				{
					currentLayeredTile.AccessibleTiles.Add (layeredTile);
				}
			}
		}
	}
	
	public static void SetTunnelSideTileToBlocked(int I, int J)
	{
		_grid[I, J].Status = Const.TILE_Blocked;
	}
	
	//public static void AddLayeredTile(int gridI, int gridJ, float height, bool isBridge, Collider collider)
	//{
	//	Vector3 baseCenter = m_Grid[gridI, gridJ].Center;
	//	Vector3 centerPos = new Vector3(baseCenter.x, height, baseCenter.z);
	//	m_Grid[gridI, gridJ].LayeredTiles.Add (new Tile(gridI, gridJ, centerPos, isBridge, !isBridge));
	//}
	
	public static void AssignGrid(Tile[,] grid)
	{
		_grid = grid;
	}
	
	private static void BuildBridge(Collider bridgeCollider)
	{
		//Find min and max tiles
		Tile maxTile = GetClosestTile (bridgeCollider.bounds.max);
		Tile minTile = GetClosestTile (bridgeCollider.bounds.min);
		
		bool leftToRight = bridgeCollider.bounds.size.x > bridgeCollider.bounds.size.z;
		
		int firstMinNumber, firstMaxNumber, secondMinNumber, secondMaxNumber;
		
		if (leftToRight)
		{
			firstMinNumber = minTile.I;
			firstMaxNumber = maxTile.I;
			
			secondMinNumber = minTile.J;
			secondMaxNumber = maxTile.J;
		}
		else
		{
			firstMinNumber = minTile.J;
			firstMaxNumber = maxTile.J;
			
			secondMinNumber = minTile.I;
			secondMaxNumber = maxTile.I;
		}
		
		for (int i = firstMinNumber; i<= firstMaxNumber; i++)
		{
			for (int j = secondMinNumber; j <= secondMaxNumber; j++)
			{
				Tile terrainTile;
				
				if (leftToRight)
				{
					terrainTile = _grid[i,j];
				}
				else
				{
					terrainTile = _grid[j,i];
				}
				
				if (j != secondMinNumber && j != secondMaxNumber)
				{
					//Create tile and check if tile underneath is passable, we're raycasting as ClosestPointOnBounds wouldn't return the value we wanted if the bridge is sloped
					Tile tileToAdd;
					
					if (leftToRight)
					{
						Ray yValueRay = new Ray(_grid[i,j].Center+(Vector3.up*1000), Vector3.down);
						RaycastHit hitInfo;
						if (bridgeCollider.Raycast (yValueRay, out hitInfo, Mathf.Infinity))
						{
							tileToAdd = new Tile(i, j, new Vector3(_grid[i,j].Center.x, hitInfo.point.y, _grid[i,j].Center.z), true, false, bridgeCollider);
						}
						else
						{
							//We didn't hit the collider, that means it's either an entrance or exit, now we have to use ClosestPointOnBounds instead
							tileToAdd = new Tile(i, j, new Vector3(_grid[i,j].Center.x, bridgeCollider.ClosestPointOnBounds (_grid[i,j].Center+(Vector3.up*10)).y, _grid[i,j].Center.z), true, false, bridgeCollider);
						}
					}
					else
					{
						Ray yValueRay = new Ray(_grid[j,i].Center+(Vector3.up*1000), Vector3.down);
						RaycastHit hitInfo;
						if (bridgeCollider.Raycast (yValueRay, out hitInfo, Mathf.Infinity))
						{
							tileToAdd = new Tile(j, i, new Vector3(_grid[j,i].Center.x, hitInfo.point.y, _grid[j,i].Center.z), true, false, bridgeCollider);
						}
						else
						{
							tileToAdd = new Tile(j, i, new Vector3(_grid[j,i].Center.x, bridgeCollider.ClosestPointOnBounds (_grid[j,i].Center+(Vector3.up*10)).y, _grid[j,i].Center.z), true, false, bridgeCollider);
						}
					}
					
					if (i == firstMinNumber || i == firstMaxNumber)
					{
						tileToAdd.BridgeTunnelEntrance = true;
					}
					
					terrainTile.LayeredTiles.Add(tileToAdd);
				}
				
				//Raycast to find height (but beware the bridge might not cover the center point)
				//Since the bridge might not be over the center point, we'll raycast in 3 places
				Vector3 startPoint = terrainTile.Center+(Vector3.down*5);
				Ray rayCenter = new Ray(startPoint, Vector3.up);
				Ray ray1;
				Ray ray2;
				RaycastHit hit;
				
				if (leftToRight)
				{
					//ray1 wants to be above, ray2 below
					Vector3 zHalfTileSize = new Vector3(0, 0, TileSize/2.0f);
					ray1 = new Ray(startPoint + zHalfTileSize, Vector3.up);
					ray2 = new Ray(startPoint - zHalfTileSize, Vector3.up);
				}
				else
				{
					//ray1 wants to be to the left, ray2 to the right
					Vector3 xHalfTileSize = new Vector3(TileSize/2.0f, 0, 0);
					ray1 = new Ray(startPoint - xHalfTileSize, Vector3.up);
					ray2 = new Ray(startPoint + xHalfTileSize, Vector3.up);
				}
				
				float heightVal;
				if (bridgeCollider.Raycast (rayCenter, out hit, Mathf.Infinity))
				{
					heightVal = hit.point.y;
				}
				else if (bridgeCollider.Raycast (ray1, out hit, Mathf.Infinity))
				{
					heightVal = hit.point.y;
				}
				else if (bridgeCollider.Raycast (ray2, out hit, Mathf.Infinity))
				{
					heightVal = hit.point.y;
				}
				else
				{
					//We haven't hit any of our points, so we must an entrance/exit tile, set it to blocked
					terrainTile.Status = Const.TILE_Blocked;
					continue;
				}
				
				if (Mathf.Abs (heightVal - terrainTile.Center.y) < PassableHeight)
				{
					terrainTile.Status = Const.TILE_Blocked;
				}
			}
		}
	}
	
	private static void BuildTunnel(Collider tunnelCollider)
	{
		//Find min and max tiles
		Tile maxTile = GetClosestTile (tunnelCollider.bounds.max);
		Tile minTile = GetClosestTile (tunnelCollider.bounds.min);
		
		bool leftToRight = tunnelCollider.bounds.size.x > tunnelCollider.bounds.size.z;
		
		int firstMinNumber, firstMaxNumber, secondMinNumber, secondMaxNumber;
		
		if (leftToRight)
		{
			firstMinNumber = minTile.I;
			firstMaxNumber = maxTile.I;
			
			secondMinNumber = minTile.J;
			secondMaxNumber = maxTile.J;
		}
		else
		{
			firstMinNumber = minTile.J;
			firstMaxNumber = maxTile.J;
			
			secondMinNumber = minTile.I;
			secondMaxNumber = maxTile.I;
		}
		
		for (int i = firstMinNumber; i<= firstMaxNumber; i++)
		{
			for (int j = secondMinNumber; j <= secondMaxNumber; j++)
			{
				Tile terrainTile;
				
				if (leftToRight)
				{
					terrainTile = _grid[i,j];
				}
				else
				{
					terrainTile = _grid[j,i];
				}
				
				if (j != secondMinNumber && j != secondMaxNumber)
				{
					//Create tile and check if tile underneath is passable
					Tile tileToAdd;
					
					if (leftToRight)
					{
						tileToAdd = new Tile(i, j, new Vector3(_grid[i,j].Center.x, tunnelCollider.bounds.max.y, _grid[i,j].Center.z), false, true, tunnelCollider);
					}
					else
					{
						tileToAdd = new Tile(j, i, new Vector3(_grid[j,i].Center.x, tunnelCollider.bounds.max.y, _grid[j,i].Center.z), false, true, tunnelCollider);
					}
					
					if (i == firstMinNumber || i == firstMaxNumber)
					{
						tileToAdd.BridgeTunnelEntrance = true;
					}
					
					terrainTile.LayeredTiles.Add(tileToAdd);
				}
				
				//Check if tile underneath is passable
				if (Mathf.Abs (tunnelCollider.bounds.max.y - terrainTile.Center.y) < PassableHeight)
				//if (terrainTile.Center.y < tunnelCollider.bounds.max.y)
				{
					terrainTile.Status = Const.TILE_Blocked;
				}
			}
		}
	}
}
