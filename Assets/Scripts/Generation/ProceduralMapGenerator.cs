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

    [Header("Defender Placement")]
    [SerializeField] private GameObject placementSpotPrefab;
    [SerializeField] private int numberOfPlacementSpots = 10;
    
    [Header("Combat")]
    [SerializeField] private AttackProjectile projectilePrefab;

    private readonly HashSet<Vector2Int> pathCells = new();
    private readonly List<List<Vector3>> generatedPaths = new();
    private readonly List<GameObject> generatedPlacementSpots = new();

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
        ClearGeneratedPlacementSpots();

        pathCells.Clear();
        generatedPaths.Clear();

        GeneratePaths();
        GenerateTerrainMesh();
        GeneratePlacementSpots();
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

    
   private void GeneratePlacementSpots()
{
    if (placementSpotPrefab == null)
    {
        Debug.LogWarning("No placement spot prefab has been assigned.");
        return;
    }

    Vector2Int centre = new(gridSize / 2, gridSize / 2);
    HashSet<Vector2Int> candidateSet = new();

    Vector2Int[] neighbourDirections =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    foreach (Vector2Int pathCell in pathCells)
    {
        foreach (Vector2Int direction in neighbourDirections)
        {
            Vector2Int candidate = pathCell + direction;

            if (!IsInsideMap(candidate))
            {
                continue;
            }

            if (pathCells.Contains(candidate))
            {
                continue;
            }

            int distanceFromTower =
                Mathf.Abs(candidate.x - centre.x)
                + Mathf.Abs(candidate.y - centre.y);

            if (distanceFromTower <= 2)
            {
                continue;
            }

            candidateSet.Add(candidate);
        }
    }

    List<Vector2Int> candidates = new(candidateSet);
    ShuffleCells(candidates);

    int spotsToCreate = Mathf.Min(
        numberOfPlacementSpots,
        candidates.Count
    );

    for (int i = 0; i < spotsToCreate; i++)
    {
        Vector3 spawnPosition =
            GridToWorldPosition(candidates[i]) + Vector3.up * 0.1f;

        GameObject newSpot = Instantiate(
            placementSpotPrefab,
            spawnPosition,
            Quaternion.identity
        );

        newSpot.name = $"Defender Placement Spot {i + 1}";
        generatedPlacementSpots.Add(newSpot);
    }
}

   
private bool IsInsideMap(Vector2Int cell)
{
    return cell.x >= 0
           && cell.x < gridSize
           && cell.y >= 0
           && cell.y < gridSize;
}


private void ShuffleCells(List<Vector2Int> cells)
{
    for (int i = cells.Count - 1; i > 0; i--)
    {
        int randomIndex = Random.Range(0, i + 1);

        (cells[i], cells[randomIndex]) =
            (cells[randomIndex], cells[i]);
    }
}


private void ClearGeneratedPlacementSpots()
{
    foreach (GameObject placementSpot in generatedPlacementSpots)
    {
        if (placementSpot != null)
        {
            Destroy(placementSpot);
        }
    }

    generatedPlacementSpots.Clear();
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

        TowerAttack towerAttack =
            placeholderTower.AddComponent<TowerAttack>();

        towerAttack.SetProjectilePrefab(projectilePrefab);
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