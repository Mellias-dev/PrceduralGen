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

### The RandomService
### The ProceduralGridGenerator
### The ProceduralGridMethod
## 2. The Simple Room Placement
## 3. The Binary Space Partition (BSP)
## 4. The Cellular Automata
## 5. The Noise Based generation
