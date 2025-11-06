using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Procedural Generation Method/Noise")]
public class Noise : ProceduralGenerationMethod
{
    [SerializeField] private float[,] _noise;


    [Header("Noise Parameters")]
    [SerializeField] private FastNoiseLite.NoiseType _noiseType;
    [SerializeField, Range(0, 0.3f)] private float _frequency;
    [SerializeField, Range(0.5f, 1.5f)] private float _amplitude;

    [Header("Fractal Parameters")]
    [SerializeField] private FastNoiseLite.FractalType _fractalType;
    [SerializeField, Range(0, 10)] private int _octaves;
    [SerializeField, Range(0, 10)] private float _lacunarity;
    [SerializeField, Range(0, 1)] private float _fractalGain;

    [Header("Heights")]
    [SerializeField, Range(-1,1)] private float _waterHeight;
    [SerializeField, Range(-1,1)] private float _sandHeight;
    [SerializeField, Range(-1,1)] private float _grassHeight;
    [SerializeField, Range(-1,1)] private float _rockHeight;


    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        GenerateNoise();
        FromNoiseToMap(_noise);
    }

    private void GenerateNoise()
    {
        FastNoiseLite noise = new FastNoiseLite(RandomService.Seed);
        noise.SetNoiseType(_noiseType);
        noise.SetFrequency(_frequency);
        noise.SetFractalType(_fractalType);
        noise.SetFractalOctaves(_octaves);
        noise.SetFractalLacunarity(_lacunarity);
        noise.SetFractalGain(_fractalGain);
        // Gather noise data
        _noise = new float[Grid.Width, Grid.Lenght];

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Lenght; y++)
            {
                _noise[x, y] = noise.GetNoise(x, y) * _amplitude;
            }
        }
    }

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
}
