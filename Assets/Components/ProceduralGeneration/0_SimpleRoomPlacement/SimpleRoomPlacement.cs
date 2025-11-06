using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;
using System.Collections.Generic;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Simple Room Placement")]
    public class SimpleRoomPlacement : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [SerializeField] private int _maxRooms = 10;
        [SerializeField] private int _roomsMaxWidth = 5;
        [SerializeField] private int _roomsMinWidth = 2;
        [SerializeField] private int _roomsMaxLength = 5;
        [SerializeField] private int _roomsMinLength = 2;
        [SerializeField] private int _spacing = 1;

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            List<RectInt> _Rooms = new List<RectInt>();
            for (int i = 0; i < _maxRooms && i < _maxSteps; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int roomWidth = RandomService.Range(_roomsMinWidth, _roomsMaxWidth);
                int roomLength = RandomService.Range(_roomsMinWidth, _roomsMaxLength);
                int x = RandomService.Range(0, Grid.Width - roomWidth);
                int y = RandomService.Range(0, Grid.Lenght - roomLength);
                RectInt room = new RectInt(x, y, roomWidth, roomLength);
                if (!CanPlaceRoom(room, 1))
                {
                    i--;
                    continue;
                }
                PlaceRoom(room);
                _Rooms.Add(room);
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
            }
            BuildGround();
            BuildCorridors(_Rooms);
        }

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

        private void BuildCorridors(List<RectInt> rooms)
        {
            rooms.Sort((a, b) => a.x.CompareTo(b.x));
            for (int i = 0; i < rooms.Count-1; i++)
            {
                PlaceCorridor(rooms[i], rooms[i + 1]);
            }
        }

        private void BuildGround()
        {
            var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");

            // Instantiate ground blocks
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int z = 0; z < Grid.Lenght; z++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
                    {
                        Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
                        continue;
                    }

                    GridGenerator.AddGridObjectToCell(chosenCell, groundTemplate, false);
                }
            }
        }
    }
}