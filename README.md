# Procedural Generation

In this repo you'll find tools and 4 algorithms of procedural generation applied on a grid. You can use them to learn more about the different kind of procedural generation method or directly to create your games.

## Table of Content
 - [1. Grid Systems](#1-grid-systems)
 - [2. The Simple Room Placement](#2-the-simple-room-placement)
 - [3. The Binary Space Partition (BSP)](#3-the-binary-space-partition-bsp)
 - [4. The Cellular Automata](#4-the-cellular-automata)
 - [5. The Noise Based generation](#5-the-noise-based-generation)

## 1. Grid Systems
there are systems to generate the grid in order to visualize your procedural generation. It is made so that you can tweak directly in the editor different parameters and see the impacts on your grid.
### The Grid
The Grid is composed of Cells that can hold a GameObject (or not). You can check if a cell has a GameObject and access it with different methods. 
If you want to modify GameObject in cells, you would usually do this :


            if (!Grid.TryGetCellByCoordinates(x, y, out var chosenCell))
            {
                Debug.LogError($"Unable to get cell on coordinates : ({x}, {y})");
                continue;
            }
            else
            {
            AddTileToCell(chosenCell,"Water",true);
            }


Here the parameter "Water" is linked to a GridObjectTemplate that you can create yourself in the editor by rightClicking=>Create=>Grid=>GameObjectTemplate and by adding a view in it.

<img width="825" height="670" alt="image" src="https://github.com/user-attachments/assets/efc990f0-b179-4406-9bb1-bf3e4b6cc204" />

The view is just a prefab with an empty GameObject with the component GridObjectController and a its sprite as its enfant.

<img width="528" height="222" alt="image" src="https://github.com/user-attachments/assets/caacfd27-6763-408a-a915-0c62ef67473d" />

You can find the exemples in the Component folder. 
### The RandomService
It is used to do all the randomised things but it is based on a seed so that there is repeatability to your experimentation with procedural generation.

Note that every change in the code can induce a change in your results even without changing the seed.

The seed is directly accessible in the editor in the ProceduralGridGenerator so you can change it and have fun.
### The ProceduralGridGenerator

It is used to generate a grid based on a ProceduralGenerationMethod.

You can switch ProceduralGenerationMethod directly in the editor by sliding a new one right there.

<img width="1152" height="648" alt="generation method" src="https://github.com/user-attachments/assets/7d567ad4-eddb-45d8-a3a2-83c4fe6f0ee5" />

### The ProceduralGenerationMethod
It defines a method of procedural generation used by the ProceduralGridGenerator.

There are actually 4 (use BSP2 not BSP if you want BSP) that you can already create by rightClicking=>Create=>Procedural Generation Method=>

<img width="822" height="698" alt="image" src="https://github.com/user-attachments/assets/6e66859f-d016-40ac-91dc-0163284e9405" />

You can make your own ProceduralgenerationMethod, all you need to do is creating a new script and add the elements according to this code snippet:

              using UnityEngine;
              using Components.ProceduralGeneration;
              using Cysharp.Threading.Tasks;
              using System.Threading;

              [CreateAssetMenu(menuName = "Procedural Generation Method/OtherGenerationmethod")]
              
              public class OtherGenerationMethod : ProceduralGenerationMethod
              {
                  protected override UniTask ApplyGeneration(CancellationToken cancellationToken)
                  {
                      throw new System.NotImplementedException();
                  }
              }

note that you need UniTask.

You can put the logic of your procedural generation method in the ApplyGeneration fonction.
## 2. The Simple Room Placement

### The Results
This is the simplest method there is here :

It puts randomly generated rectangular rooms in the grid and connects their center by red corridors :

<img width="700" height="695" alt="image" src="https://github.com/user-attachments/assets/286eccba-31c7-4da9-b83a-19926218ee0a" />

You can play with various parameters such as the size or the numbers of the rooms directly in the editor :

<img width="782" height="775" alt="image" src="https://github.com/user-attachments/assets/0cd34eee-cb74-441f-a937-5a5208179532" />

### The logic

Basicly we take a random X and Y in the grid which will be the most bottom right cell of the room, a random Width and Length and we place the room if it is not overalpping with another one thanks to the PlaceRoom function :

        private void PlaceRoom(RectInt room)
        {
            if (!CanPlaceRoom(room, 1)) return;

            for (int ix = room.xMin; ix < room.xMax; ix++)
            {
                for (int iy = room.yMin; iy < room.yMax; iy++)
                {
                    if (!Grid.TryGetCellByCoordinates(ix, iy, out Cell cell))
                        continue;

                    AddTileToCell(cell, ROOM_TILE_NAME, true);
                }
            }
        }
        
Every time we place a room we add it to a list then we sort this list by there X and we connect each of the center of the rooms with corridors with the function PlaceCorridors :

        private void PlaceCorridor(RectInt room1, RectInt room2)
        {
            Vector2 center1 = room1.center;
            Vector2 center2 = room2.center;

            for (int ix = (int)center1.x; ix <= center2.x; ix++)
            {
                if (!Grid.TryGetCellByCoordinates(ix, (int)center1.y, out Cell cell))
                    continue;

                AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
            }
            if (center2.y < center1.y)
            {
                int temp = (int)center2.y;
                center2.y = (int)center1.y;
                center1.y = temp;
            }
            for (int iy = (int)center1.y; iy <= center2.y; iy++)
            {
                if (!Grid.TryGetCellByCoordinates((int)center2.x, iy, out Cell cell))
                    continue;

                AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
            }
        }

## 3. The Binary Space Partition (BSP)

### The Results

This method is a bit more complex since it uses recursivity. It basicly devides the grid in zones to ensure that the rooms can't overlap and then we place 1 room in each zone before linking them with corridors. 

Since the zones are created by pairs we can easily link the pairs to create corridors, this is the great strength of BSP compared to Simple Room Placement.

Here are the results :

<img width="563" height="566" alt="image" src="https://github.com/user-attachments/assets/81bac070-ebf3-4734-978b-e6e9d6e2644f" />

As always you can play with the parameters in the editor.

### The logic

The logic between this method is a little tricky but very clever.

The principle is to create nodes by randomly splitting first the grid then the nodes in two until the nodes are too small to be split.
To do that we recurcively create nodes (a new class):

    private Node _child1;
    private Node _child2;
    
    public Node(RandomService randomService, BSP2 bsp2, RectInt room)
    {
        _randomService = randomService;
        _bsp2 = bsp2;
        _gridGenerator = bsp2.GridGenerator;
        _room = room;
        
        Split();
    }

Here every time we create a Node we call Split wich potentially call the creation of 2 Nodes and each of them call the Split function. 

You can see the possible chain reaction. We have to be carefull of infinite loops so we put conditions in Split() : 

      private void Split()
      {
          RectInt splitBoundsLeft = default;
          RectInt splitBoundsRight = default;
          bool splitFound = false;
      
          for (int i = 0; i < _bsp2.MaxSplitAttempt; i++)
          {
              bool horizontal = _randomService.Chance(_bsp2.HorizontalSplitChance);
              float splitRatio = _randomService.Range(_bsp2.SplitRatio.x, _bsp2.SplitRatio.y);
          
              if (horizontal)
              {
                  if (!CanSplitHorizontally(splitRatio, out splitBoundsLeft, out splitBoundsRight))
                  {
                      continue;
                  }
              }
              else
              {
                  if (!CanSplitVertically(splitRatio, out splitBoundsLeft, out splitBoundsRight))
                  {
                      continue;
                  }
              }
              
              splitFound = true;
              break;
          }
      
          // Stop recursion, it's a Leaf !
          if (!splitFound)
          {
              _isLeaf = true;
              PlaceRoom(_room);
              
              return;
          }
      
          _child1 = new Node(_randomService, _bsp2, splitBoundsLeft);
          _child2 = new Node(_randomService, _bsp2, splitBoundsRight);
          
          _bsp2.Tree.Add(_child1);
          _bsp2.Tree.Add(_child2);
      }

This Split function makes 2 Nodes if the parent Node can be split horizontaly or verticaly. If not then we have reach a node sufficiently small and we place a room in it.

Given that Nodes are created by pairs that we call sisters and that the only nodes that give rooms don't have children because they couldn't split, we just have to connect sisters that don't have children to form corridors.

Here comes the fonction ConnectSisters in wich we recursively search for these nodes and we create corridors between them :

    public void ConnectSisters()
    {
        // It's a leaf, nothing to do here.
        if (_child1 == null || _child2 == null) 
            return;
        
        // Connect sisters
        ConnectNodes(_child1, _child2);
            
        // Connect child of sisters
        _child1.ConnectSisters();
        _child2.ConnectSisters();
    }

    private void ConnectNodes(Node node1, Node node2)
    {
        var center1 = node1.GetLastChild()._room.GetCenter();
        var center2 = node2.GetLastChild()._room.GetCenter();
        
        CreateDogLegCorridor(center1, center2);
    }
    
    /// Creates an L-shaped corridor between two points, randomly choosing horizontal-first or vertical-first
    private void CreateDogLegCorridor(Vector2Int start, Vector2Int end)
    {
        bool horizontalFirst = _randomService.Chance(0.5f);

        if (horizontalFirst)
        {
            // Draw horizontal line first, then vertical
            CreateHorizontalCorridor(start.x, end.x, start.y);
            CreateVerticalCorridor(start.y, end.y, end.x);
        }
        else
        {
            // Draw vertical line first, then horizontal
            CreateVerticalCorridor(start.y, end.y, start.x);
            CreateHorizontalCorridor(start.x, end.x, end.y);
        }
    }

## 4. The Cellular Automata

### The Results
Here we leave the creation of rooms to create what could be more associated with maps.

This method consists of creating a chaotic grid first and then we scan this grid multiple times to change each cell of the grid according to the type of its 8 neighboors.

It transforms chaos into continents and oceans :

![ezgif com-animated-gif-maker](https://github.com/user-attachments/assets/62e18092-7c7f-455a-bbf4-6d7b589cf0a8)


### The logic

First we generate a white noise of the size of the grid (each tile has a certain chance to be eaither grass or water).

    private void GenerateNoiseGrid()
    {
        var earthTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");
        var waterTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Water");
        for (int y = 0; y < Grid.Lenght; y++)
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                if (!Grid.TryGetCellByCoordinates(x, y, out var chosenCell))
                {
                    Debug.LogError($"Unable to get cell on coordinates : ({x}, {y})");
                    continue;
                }
                int rdm = RandomService.Range(0, 100);
                if (rdm < _earthDensity)
                {
                    GridGenerator.AddGridObjectToCell(chosenCell, earthTemplate, false);
                }
                if (rdm >= _earthDensity)
                {
                    GridGenerator.AddGridObjectToCell(chosenCell, waterTemplate, false);
                }
            }
        }
    }
    
The basic principle of this method is to check the neighboors of each tile and if there are more than 3 of them that are grass then the tile automatically becomes grass itself. To do this we have these 2 functions :

    private bool CheckNeighboors(int x, int y)
    {
        int earthCount = 0;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i != 0 || j != 0)
                {
                    if (!Grid.TryGetCellByCoordinates(x + i, y + j, out Cell chosenCell)) continue;
                    //Debug.Log(chosenCell.GridObject.Template.Name);
                    if (chosenCell.GridObject.Template.Name == "Grass")
                    {
                        earthCount++;
                        //Debug.Log(earthCount);

                    }
                }

            }
        }
        if (earthCount > 3)
        {
            //Debug.Log("true");
            return true;
        }
        else return false;
    }
}

here we make the extra step to make an array of bool in wich each bool represents a cell and if it has to become grass or not based on its neighboors.

    private void GenerateStepGrid()
    {
        var earthTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");
        var waterTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Water");
        bool[][] stepGrid = new bool[Grid.Width][];

        for (int y = 1; y < Grid.Lenght-1; y++)
        {
            stepGrid[y] = new bool[Grid.Width];
            for (int x = 1; x < Grid.Width-1; x++)
            {
                stepGrid[y][x] = CheckNeighboors(x,y);
            }
        }

        for (int y = 1; y < Grid.Lenght-1; y++)
        {
            for (int x = 1; x < Grid.Width-1; x++)
            {
                if (!Grid.TryGetCellByCoordinates(x, y, out var chosenCell))
                {
                    Debug.LogError($"Unable to get cell on coordinates : ({x}, {y})");
                    continue;
                }
                if (stepGrid[y][x])
                {
                    GridGenerator.AddGridObjectToCell(chosenCell, earthTemplate, true);
                }
                else GridGenerator.AddGridObjectToCell(chosenCell, waterTemplate, true);
            }
        }
    }

In order to appreciate how this generation method works and its educationnal aspect, we render each step and we keep checking neighboors in the grid of cells, therefor calling each time demanding methods such as TryGetCellByCoordinates or AddGridObjectToCell.

For the sake of performances we could be checking neighboors on the array of booleans and only render the final step.

## 5. The Noise Based generation

### The Results
Here we generate a map based on a noise (generated with FastNoiseLight). Basically we generate a noise (like a lengthwave between -1 and 1) and we check the height of the noise in every cell, this height will dictate what type of GameObject we will place in the cell (water, sand, grass or rock).

This method gives birth to beautifull maps and we can tweak the parameters to make different biomes, there i made what looks like lagoons :

<img width="656" height="662" alt="image" src="https://github.com/user-attachments/assets/5f79fc52-3678-4891-9fc3-f1a0a51fa547" />

### The logic

All we do here is just generating a noise the dimensions of the grid thanks to the FastNoiseLight Library.

Then we make an array of float values that stocks the height of the noise for X and Y coordinates and we go through this array attaching a different GameObject to the cell with the given X and Y coordinates according to different ranges of values in the function FromNoiseToMap :

          private void FromNoiseToMap(float[,] noise)
          {
              for (int x = 0; x < Grid.Lenght; x++)
              {
                  for (int y = 0; y < Grid.Width; y++)
                  {
                      if (!Grid.TryGetCellByCoordinates(x, y, out var chosenCell))
                      {
                          Debug.LogError($"Unable to get cell on coordinates : ({x}, {y})");
                          continue;
                      }
                      else if (noise[x,y]<=_waterHeight)
                      {
                      AddTileToCell(chosenCell,"Water",true);
                      }
                      else if (noise[x,y] > _waterHeight && noise[x,y]<=_sandHeight)
                      {
                      AddTileToCell(chosenCell,"Sand",true);
                      }
                      else if (noise[x,y] > _sandHeight && noise[x,y]<=_grassHeight)
                      {
                      AddTileToCell(chosenCell,"Grass",true);
                      }
                      else if (noise[x,y] > _grassHeight)
                      {
                      AddTileToCell(chosenCell,"Rock",true);
                      }
      
                  }
              }
          }

Changing the ranges of values is possible directly in the editor and influence the type of landscape the generation will produce. Explore and have Fun !

## Special Thanks

Special thanks to those who worked on FastNoiseLight (https://github.com/Auburn/FastNoiseLite).

Special thanks to Yona Rutkowski, this repo comes from his transmission of knowledge!
