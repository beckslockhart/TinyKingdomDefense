using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class ProceduralMapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private int gridSize = 17;
    [SerializeField] private float tileSize = 2f;
    [SerializeField] private float grassHeightVariation = 0.25f;
    [SerializeField] private int numberOfPaths = 3;

    [Header("Materials")]
    [SerializeField] private Material grassMaterial;
    [SerializeField] private Material pathMaterial;

    private readonly HashSet<Vector2Int> pathCells = new();
    private readonly List<List<Vector3>> generatedPaths = new();

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private GameObject placeholderTower;

   
    public IReadOnlyList<List<Vector3>> GeneratedPaths => generatedPaths;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        GenerateMap();
    }

   
    public void GenerateMap()
    {
        pathCells.Clear();
        generatedPaths.Clear();

        GeneratePaths();
        GenerateTerrainMesh();
        CreatePlaceholderTower();
    }

  
    private void GeneratePaths()
    {
        Vector2Int centre = new(gridSize / 2, gridSize / 2);

        List<int> availableEdges = new() { 0, 1, 2, 3 };
        ShuffleList(availableEdges);

        int pathsToCreate = Mathf.Min(numberOfPaths, availableEdges.Count);

        for (int i = 0; i < pathsToCreate; i++)
        {
            Vector2Int startCell = GetRandomEdgeCell(availableEdges[i]);
            CreatePath(startCell, centre);
        }
    }

  
    private Vector2Int GetRandomEdgeCell(int edge)
    {
        int randomCoordinate = Random.Range(2, gridSize - 2);

        return edge switch
        {
            0 => new Vector2Int(0, randomCoordinate),
            1 => new Vector2Int(gridSize - 1, randomCoordinate),
            2 => new Vector2Int(randomCoordinate, 0),
            _ => new Vector2Int(randomCoordinate, gridSize - 1)
        };
    }

  
    private void CreatePath(Vector2Int start, Vector2Int destination)
    {
        Vector2Int current = start;
        List<Vector3> worldPath = new();

        AddPathCell(current, worldPath);

        while (current != destination)
        {
            bool canMoveHorizontally = current.x != destination.x;
            bool canMoveVertically = current.y != destination.y;

            bool moveHorizontally;

            if (!canMoveVertically)
            {
                moveHorizontally = true;
            }
            else if (!canMoveHorizontally)
            {
                moveHorizontally = false;
            }
            else
            {
                moveHorizontally = Random.value > 0.5f;
            }

            if (moveHorizontally)
            {
                current.x += current.x < destination.x ? 1 : -1;
            }
            else
            {
                current.y += current.y < destination.y ? 1 : -1;
            }

            AddPathCell(current, worldPath);
        }

        generatedPaths.Add(worldPath);
    }

   
    private void AddPathCell(Vector2Int cell, List<Vector3> worldPath)
    {
        pathCells.Add(cell);
        worldPath.Add(GridToWorldPosition(cell));
    }

   
    private void GenerateTerrainMesh()
    {
        List<Vector3> vertices = new();
        List<int> grassTriangles = new();
        List<int> pathTriangles = new();

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                Vector2Int cell = new(x, z);
                bool isPath = pathCells.Contains(cell);

                float height = isPath
                    ? 0f
                    : Random.Range(0.05f, grassHeightVariation);

                AddTile(
                    x,
                    z,
                    height,
                    vertices,
                    isPath ? pathTriangles : grassTriangles
                );
            }
        }

        Mesh terrainMesh = new()
        {
            name = "Procedurally Generated Terrain"
        };

        terrainMesh.SetVertices(vertices);
        terrainMesh.subMeshCount = 2;
        terrainMesh.SetTriangles(grassTriangles, 0);
        terrainMesh.SetTriangles(pathTriangles, 1);
        terrainMesh.RecalculateNormals();
        terrainMesh.RecalculateBounds();

        meshFilter.mesh = terrainMesh;
        meshCollider.sharedMesh = terrainMesh;
        meshRenderer.materials = new[] { grassMaterial, pathMaterial };
    }

  
    private void AddTile(
        int x,
        int z,
        float height,
        List<Vector3> vertices,
        List<int> triangles)
    {
        float mapOffset = (gridSize - 1) * tileSize * 0.5f;

        float left = x * tileSize - mapOffset - tileSize * 0.5f;
        float right = left + tileSize;
        float bottom = z * tileSize - mapOffset - tileSize * 0.5f;
        float top = bottom + tileSize;

        int startingVertex = vertices.Count;

        vertices.Add(new Vector3(left, height, bottom));
        vertices.Add(new Vector3(left, height, top));
        vertices.Add(new Vector3(right, height, top));
        vertices.Add(new Vector3(right, height, bottom));

        triangles.Add(startingVertex);
        triangles.Add(startingVertex + 1);
        triangles.Add(startingVertex + 2);

        triangles.Add(startingVertex);
        triangles.Add(startingVertex + 2);
        triangles.Add(startingVertex + 3);
    }

   
    private void CreatePlaceholderTower()
    {
        if (placeholderTower != null)
        {
            Destroy(placeholderTower);
        }

        placeholderTower = GameObject.CreatePrimitive(PrimitiveType.Cube);
        placeholderTower.name = "Central Tower Placeholder";
        placeholderTower.transform.position =
            GridToWorldPosition(new Vector2Int(gridSize / 2, gridSize / 2))
            + Vector3.up * 2f;

        placeholderTower.transform.localScale = new Vector3(3f, 4f, 3f);
        placeholderTower.AddComponent<TowerHealth>();
        placeholderTower.AddComponent<TowerAttack>();
    }

   
    private Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        float mapOffset = (gridSize - 1) * tileSize * 0.5f;

        return new Vector3(
            gridPosition.x * tileSize - mapOffset,
            0.1f,
            gridPosition.y * tileSize - mapOffset
        );
    }

 
    private void ShuffleList(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}