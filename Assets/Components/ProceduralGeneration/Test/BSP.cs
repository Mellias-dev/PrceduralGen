using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VTools.Grid;
using VTools.RandomService;
using VTools.ScriptableObjectDatabase;

[CreateAssetMenu(menuName = "Procedural Generation Method/BSP")]

public class BSP : ProceduralGenerationMethod
{
    [SerializeField] private List<BSPNode> Tree;
    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        var allGrid = new RectInt(0, 0, Grid.Width, Grid.Lenght);
        Tree = new List<BSPNode>();

        var root = new BSPNode(allGrid, RandomService, Tree, 0, this);
        Tree.Add(root);
        BuildGround();
        ConnectSisters(root);
    }
    public void PlaceRoom(RectInt room)
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

    public void PlaceCorridor(RectInt room1, RectInt room2)
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
        //rooms.Sort((a, b) => a.x.CompareTo(b.x));
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            PlaceCorridor(rooms[i], rooms[i + 1]);
        }
    }
    private void ConnectSisters(BSPNode room)
    {
        if (room._child1 != null)
        {
            ConnectSisters(room._child1);
            ConnectSisters(room._child2);

            Vector2 c1 = room._child1._bounds.center;
            Vector2 c2 = room._child2._bounds.center;

            int fixedY = (int)c1.y;
            for (int x = (int)Mathf.Min(c1.x, c2.x); x < Mathf.Max(c1.x, c2.x); x++)
            {
                if (!Grid.TryGetCellByCoordinates(x, fixedY, out Cell cell))
                    continue;
                //if (cell.GridObject.Template.Name == "Grass")
                    AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
            }

            int fixedX = (int)c2.x;
            for (int y = (int)Mathf.Min(c1.y, c2.y); y < Mathf.Max(c1.y, c2.y); y++)
            {
                if (!Grid.TryGetCellByCoordinates(fixedX, y, out Cell cell))
                    continue;

                //if (cell.GridObject.Template.Name == "Grass")
                    AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
            }
        }
    }
}

[Serializable]
public class BSPNode
{
    [SerializeField] public RectInt _bounds;
    [SerializeField] private int _minimumSize = 20;
    [SerializeField] private int _step = 0;
    public BSPNode _child1, _child2;
    private RandomService _rs;
    private BSP _bsp;
    public BSPNode(RectInt bounds, RandomService rs, List<BSPNode> tree, int step, BSP bsp)
    {
        _bounds = bounds;
        _rs = rs;
        _step = step;
        _bsp = bsp;
        int coinflip = rs.Range(0, 2);
        if (coinflip == 0)
        {
            if (_bounds.width >= _minimumSize)
            {
                int widthCutingbound = rs.Range(10, _bounds.width - 5);
                _child1 = new BSPNode(new RectInt(_bounds.x, _bounds.y, widthCutingbound, _bounds.height), _rs, tree, _step + 1, _bsp);
                _child2 = new BSPNode(new RectInt(_bounds.x + widthCutingbound, _bounds.y, _bounds.width - widthCutingbound, _bounds.height), _rs, tree, step + 1, _bsp);
                tree.Add(_child1);
                tree.Add(_child2);
                _bounds = FromNodeToRoom(rs);

            }
            _bounds = FromNodeToRoom(rs);
            _bsp.PlaceRoom(_bounds);
        }
        else
        {
            if (_bounds.height >= _minimumSize)
            {
                int heightCutingbound = rs.Range(10, _bounds.height - 5);
                _child1 = new BSPNode(new RectInt(_bounds.x, _bounds.y, _bounds.width, heightCutingbound), _rs, tree, _step + 1, _bsp);
                _child2 = new BSPNode(new RectInt(_bounds.x, _bounds.y + heightCutingbound, _bounds.width, _bounds.height - heightCutingbound), _rs, tree, step + 1, _bsp);
                tree.Add(_child1);
                tree.Add(_child2);
                _bounds = FromNodeToRoom(rs);

            }
            _bounds = FromNodeToRoom(rs);
            _bsp.PlaceRoom(_bounds);
        }

    }

    public RectInt FromNodeToRoom(RandomService rs)
    {
        int rx = rs.Range(_bounds.x + 1, _bounds.x + _bounds.width / 2);
        int ry = rs.Range(_bounds.y + 1, _bounds.y + _bounds.height / 2);
        RectInt room = new RectInt(rx, ry, _bounds.width / 2, _bounds.height / 2);
        return room;
    }

}