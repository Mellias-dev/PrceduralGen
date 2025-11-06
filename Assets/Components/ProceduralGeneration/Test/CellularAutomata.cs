using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

[CreateAssetMenu(menuName = "Procedural Generation Method/Cellular_Automata")]
public class CellularAutomata : ProceduralGenerationMethod
{
    [SerializeField] int _earthDensity = 70;
    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        GenerateNoiseGrid();
        for (int i = 0; i < _maxSteps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            GenerateStepGrid();


            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
        }
    }

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

