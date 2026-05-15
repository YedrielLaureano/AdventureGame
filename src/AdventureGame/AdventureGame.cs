namespace AdventureGame;

public class AdventureGame
{
	public readonly string GO_NORTH = "W";
	public readonly string GO_SOUTH = "S";
	public readonly string GO_EAST = "D";
	public readonly string GO_WEST = "A";
	public readonly string GET_LAMP = "L";
	public readonly string GET_KEY = "K";
	public readonly string OPEN_CHEST = "O";
	public readonly string QUIT = "Q";

	private Adventurer adventurer;
	private Room[,] dungeon;
	private int aRow;
	private int aCol;
	private int grueRow;
	private int grueCol;
	private int exitRow;
	private int exitCol;
	private bool isChestOpen;
	private bool hasPlayerQuit;
	private bool isAdventureAlive;
    private bool grueActive;
	private bool hasAdventurerDied;
	private bool hasAdventurerExitDungeon;
	private string lastDirection;
	private string lastMessage;

	private const char Wall = '#';

	public AdventureGame()
	{

	}

	public void Start()
	{
        Init();

        ShowGameStartScreen();

		string input;

		do
		{
			Console.Clear();

			ShowScene();

			if (!string.IsNullOrEmpty(lastMessage))
			{
   				Console.WriteLine(lastMessage);
    			lastMessage = "";
			}

			do
			{
				ShowInputOptions();

				input = GetInput();
			}
			while(!IsValidInput(input));

			ProcessInput(input);

			UpdateGameState();
		}
		while(!IsGameOver());

		ShowGameOverScreen();
	}

	private void Init()
	{
		adventurer = new Adventurer();

		dungeon = Load("Dungeon.txt");

		isChestOpen = false;
		hasPlayerQuit = false;
		isAdventureAlive = true;
        grueActive = false;
		hasAdventurerDied = false;
		hasAdventurerExitDungeon = false;

		lastDirection = string.Empty;
		lastMessage = string.Empty;
	}

	private void ShowGameStartScreen()
	{
		lastMessage = "Welcome to Adventure Game!";
	}

	private void ShowScene()
	{
		var r = dungeon[aRow, aCol];

		if(adventurer.HasLamp() || r.IsLit())
		{
			Console.WriteLine(r.GetDescription());
		}
		else
		{
			Console.WriteLine("This room is pitch black!");
		}

        if(grueActive)
		{
    		var path = FindPathToAdventure();

    		Console.WriteLine("\nGrue path:");

    		foreach (var step in path)
    		{
        		Console.Write($"({step.row},{step.col}) ");
    		}

    		Console.WriteLine();
		}
	}

	private void ShowInputOptions()
	{
		string options = ""
		+ $"GO NORTH [{GO_NORTH}] | GO EAST [{GO_EAST}] | GET LAMP [{GET_LAMP}] | OPEN CHEST [{OPEN_CHEST}]\n"
		+ $"GO SOUTH [{GO_SOUTH}] | GO WEST [{GO_WEST}] | GET KEY  [{GET_KEY}] | QUIT       [{QUIT}]\n"
		+ $"> ";

		Console.Write(options);
	}

	private string GetInput()
	{
		return Console.ReadLine()!.ToUpper();
	}

	private bool IsValidInput(string input)
	{
		string[] validInputs = { GO_NORTH, GO_SOUTH, GO_EAST, GO_WEST, GET_LAMP, GET_KEY, OPEN_CHEST, QUIT };

		if(!validInputs.Contains(input))
		{
			Console.WriteLine("ERROR: Invalid input. Please try again.");
			return false;
		}

		return true;
	}

	private void ProcessInput(string input)
	{
		Room r = dungeon[aRow, aCol];

		if(grueActive && !adventurer.HasLamp() && !r.IsLit() && input != lastDirection)
		{
			Console.WriteLine("You got eaten alive by the Grue!");
			isAdventureAlive = false;
			return;
		}
		else if(input == GO_NORTH)
		{
			GoNorth(r);
		}
		else if(input == GO_SOUTH)
		{
			GoSouth(r);
		}
		else if(input == GO_EAST)
		{
			GoEast(r);
		}
		else if(input == GO_WEST)
		{
			GoWest(r);
		}
		else if(input == GET_LAMP)
		{
			GetLamp(r);
		}
		else if(input == GET_KEY)
		{
			GetKey(r);
		}
		else if(input == OPEN_CHEST)
		{
			OpenChest(r);
		}
		else// if(input == QUIT)
		{
			Quit();
		}

	}

	private void UpdateGameState()
	{
		hasAdventurerExitDungeon = (isChestOpen && aRow == exitRow && aCol == exitCol);

		if(hasAdventurerExitDungeon) { return; }

		if(isChestOpen)
		{
			List<(int row, int col)> path = FindPathToAdventure();

			if(path.Count > 1)
			{
				grueRow = path[1].row;
            	grueCol = path[1].col;
			}
		
			hasAdventurerDied = (grueRow == aRow && grueCol == aCol);

			if(hasAdventurerDied)
			{
				isAdventureAlive = false;

				var lastStep = path[path.Count - 1];
            	Console.WriteLine($"Grue final step: ({lastStep.row},{lastStep.col})\nThe Grue caught you!");
			}
		}
	}


	private bool IsGameOver()
	{
		return hasPlayerQuit || hasAdventurerDied || hasAdventurerExitDungeon;
	}

	private void ShowGameOverScreen()
	{
        if(!isAdventureAlive)
        {
            Console.WriteLine("You died!");
        }
		else if(hasAdventurerExitDungeon)
    	{
        	Console.WriteLine("You escaped with the treasure!");
    	}
	}

	private void GoNorth(Room r)
	{
		if(r.HasNorth())
		{
			aRow -= 1;
			lastDirection = GO_SOUTH;
		}
		else
		{
			lastMessage = "You cannot go north!\a";
		}
	}

	private void GoSouth(Room r)
	{
		if(r.HasSouth())
		{
			aRow += 1;
			lastDirection = GO_NORTH;
		}
		else
		{
			lastMessage = "You cannot go south!\a";
		}
	}

	private void GoEast(Room r)
	{
		if(r.HasEast())
		{
			aCol += 1;
			lastDirection = GO_WEST;
		}
		else
		{
			lastMessage = "You cannot go east!\a";
		}
	}

	private void GoWest(Room r)
	{
		if(r.HasWest())
		{
			aCol -= 1;
			lastDirection = GO_EAST;
		}
		else
		{
			lastMessage = "You cannot go west!\a";
		}
	}

	private void GetLamp(Room r)
	{
		if(r.HasLamp())
		{
			lastMessage = "You got the lamp!";
			adventurer.SetLamp(true);
			r.SetLamp(false);
		}
		else
		{
			lastMessage = "There is no lamp in this room.";
		}
	}

	private void GetKey(Room r)
	{
		if(r.HasKey())
		{
			lastMessage = "You got the key!";
			adventurer.SetKey(true);
			r.SetKey(false);
		}
		else
		{
			lastMessage = "There is no key in this room.";
		}
	}

	private void OpenChest(Room r)
	{
		if(r.HasChest())
		{
			if(adventurer.HasKey())
			{
				Console.WriteLine("You got the treasure!");
				isChestOpen = true;

                grueActive = true;
			}
			else
			{
				lastMessage = "You do not have the key!";
			}
		}
		else
		{
			lastMessage = "There is no chest in this room.";
		}
	}

	private void Quit()
	{
		Console.WriteLine("You quit the game!");
		hasPlayerQuit = true;
	}

	public Room[,] Load(string filePath)
	{
		string[] lines = File.ReadAllLines(filePath);

		int rows = int.Parse(lines[0]);
		int cols = int.Parse(lines[1]);

		this.exitRow = int.Parse(lines[2]);
		this.exitCol = int.Parse(lines[3]);
		int lampRow = int.Parse(lines[4]);
		int lampCol = int.Parse(lines[5]);
		int keyRow = int.Parse(lines[6]);
		int keyCol = int.Parse(lines[7]);
		int chestRow = int.Parse(lines[8]);
		int chestCol = int.Parse(lines[9]);
		this.grueRow = int.Parse(lines[10]);
		this.grueCol = int.Parse(lines[11]);
		this.aRow = int.Parse(lines[12]);
		this.aCol = int.Parse(lines[13]);

		int layoutStart = 14;
		int descriptionsStart = layoutStart + rows;

		if (lines.Length < descriptionsStart)
			throw new FormatException("File does not contain enough layout rows.");

		Room[,] dungeon = new Room[rows, cols];
		List<(int row, int col)> traversableTiles = new();

		for (int row = 0; row < rows; row++)
		{
			string layoutLine = lines[layoutStart + row];

			if (layoutLine.Length != cols)
				throw new FormatException($"Layout row {row} must contain exactly {cols} characters.");

			for (int col = 0; col < cols; col++)
			{
				if (layoutLine[col] != Wall)
				{
					dungeon[row, col] = new Room();
					traversableTiles.Add((row, col));
				}
			}
		}

		int descriptionCount = lines.Length - descriptionsStart;

		if (descriptionCount != traversableTiles.Count)
		{
			throw new FormatException(
					$"Description count ({descriptionCount}) must match traversable tile count ({traversableTiles.Count})."
			);
		}

		for (int i = 0; i < traversableTiles.Count; i++)
		{
			string[] parts = lines[descriptionsStart + i].Split('|', 2);

			if (parts.Length != 2)
				throw new FormatException($"Invalid room description line: {lines[descriptionsStart + i]}");

			bool isLit = parts[0] switch
			{
				"1" => true,
				"0" => false,
				_ => throw new FormatException("Room lit value must be 1 or 0.")
			};

			string description = parts[1];

			var (row, col) = traversableTiles[i];
			Room room = dungeon[row, col];

			room.SetLit(isLit);
			room.SetDescription(description);

			room.SetLamp(row == lampRow && col == lampCol);
			room.SetKey(row == keyRow && col == keyCol);
			room.SetChest(row == chestRow && col == chestCol);

			room.SetNorth(IsTraversable(dungeon, row - 1, col));
			room.SetSouth(IsTraversable(dungeon, row + 1, col));
			room.SetEast(IsTraversable(dungeon, row, col + 1));
			room.SetWest(IsTraversable(dungeon, row, col - 1));
		}

		ValidateTraversableTile(dungeon, exitRow, exitCol, "exit");

		return dungeon;
	}

	private bool IsTraversable(Room[,] dungeon, int row, int col)
	{
		return row >= 0 &&
					row < dungeon.GetLength(0) &&
					col >= 0 &&
					col < dungeon.GetLength(1) &&
					dungeon[row, col] != null;
	}

	private void ValidateTraversableTile(Room[,] dungeon, int row, int col, string name)
	{
		if (!IsTraversable(dungeon, row, col))
			throw new FormatException($"The {name} position must be on a traversable tile.");
	}

    private List<(int, int)> GetAdjacents(int row, int col)
    {
        var adjs = new List<(int, int)>();

        Room r = dungeon[row, col];

        if(r.HasNorth()) {adjs.Add((row - 1, col - 0)); }
        if(r.HasSouth()) {adjs.Add((row + 1, col + 0)); }
        if(r.HasWest()) {adjs.Add((row - 0, col - 1)); }
        if(r.HasEast()) {adjs.Add((row + 0, col + 1)); }

        return adjs;
    }

    private List<(int row, int col)> FindPathToAdventure()
    {
        var start = (row: grueRow, col: grueCol);
        var goal = (row: aRow, col: aCol);

        var openSet = new PriorityQueue<(int row, int col), int>();
        var comeFrom  = new Dictionary<(int row, int col), (int row, int col)>();
        var gScore = new Dictionary<(int row, int col), int>();

        gScore[start] = 0;
        openSet.Enqueue(start, Heuristic(start, goal));

        while (openSet.Count > 0)
        {
            var room = openSet.Dequeue();

            if(room == goal)
            {
                return ReconstructPath(comeFrom, room);
            }

            foreach ( var adjacent in GetAdjacents(room.row, room.col))
            {
                int newScore = gScore[room] + 1;

                if (!gScore.TryGetValue(adjacent, out int oldScore) || newScore < oldScore)
                {
                    comeFrom[adjacent] = room;
                    gScore[adjacent] = newScore;

                    int fScore = newScore + Heuristic(adjacent, goal);
                    openSet.Enqueue(adjacent, fScore);
                }
            }
        }

        return new List<(int row, int col)>();
    }

    private static int Heuristic((int row, int col) a, (int row, int col) b)
    {
        return Math.Abs(a.row - b.row) + Math.Abs(a.col - b.col);
    }

    private static List<(int row, int col)> ReconstructPath(Dictionary<(int row, int col), (int row, int col)> comeFrom,(int row, int col) current)
    {
        var path = new List<(int row, int col)> {current};

        while(comeFrom.ContainsKey(current))
        {
            current = comeFrom[current];
            path.Add(current);
        }

        path.Reverse();
        return path;
    }

}