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
## 2. The Simple Room Placement

## 3. The Binary Space Partition (BSP)
## 4. The Cellular Automata
## 5. The Noise Based generation
