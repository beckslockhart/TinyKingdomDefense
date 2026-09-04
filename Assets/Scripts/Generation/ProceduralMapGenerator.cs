using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ProceduralMapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private int gridSizeX = 25;
    [SerializeField] private int gridSizeZ = 17;
    [SerializeField] private float tileSize = 2f;
    [SerializeField] private float grassHeightVariation = 0.25f;
    [SerializeField] private int numberOfPaths = 4;

    [Header("Materials")]
    [SerializeField] private Material grassMaterial;
    [SerializeField] private Material pathMaterial;

    [Header("Defender Placement")]
    [SerializeField] private GameObject placementSpotPrefab;
    [SerializeField] private int numberOfPlacementSpots = 12;

    [Header("Forest Decoration")]
    [SerializeField] private GameObject forestClusterPrefab;
    [SerializeField] private int numberOfForestClusters = 14;
    [SerializeField] private float forestScale = 1f;
    [SerializeField] private float forestYOffset = 0.03f;
    [SerializeField] private int forestPathClearance = 2;
    [SerializeField] private int minimumForestSpacing = 3;

    [Header("Combat")]
    [SerializeField] private AttackProjectile projectilePrefab;

    private readonly HashSet<Vector2Int> pathCells = new();
    private readonly HashSet<Vector2Int> placementCells = new();
    private readonly HashSet<Vector2Int> forestCells = new();

    private readonly List<List<Vector3>> generatedPaths = new();
    private readonly List<GameObject> generatedPlacementSpots = new();
    private readonly List<GameObject> generatedForestClusters = new();
    
    [Header("Castle Visual")]
    [SerializeField] private GameObject castleVisualPrefab;
    [SerializeField] private float castleVisualScale = 1f;
    [SerializeField] private Vector3 castleVisualOffset = Vector3.zero;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private GameObject placeholderTower;

    public IReadOnlyList<List<Vector3>> GeneratedPaths =>
        generatedPaths;

    
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
        ClearGeneratedForestClusters();

        pathCells.Clear();
        placementCells.Clear();
        forestCells.Clear();
        generatedPaths.Clear();

        GeneratePaths();
        GenerateTerrainMesh();
        GeneratePlacementSpots();
        GenerateForestClusters();
        CreatePlaceholderTower();
    }

    
    private void GeneratePaths()
    {
        Vector2Int centre = GetCentreCell();

        List<int> availableEdges = new()
        {
            0,
            1,
            2,
            3
        };

        ShuffleList(availableEdges);

        int pathsToCreate = Mathf.Min(
            numberOfPaths,
            availableEdges.Count
        );

        for (int i = 0; i < pathsToCreate; i++)
        {
            Vector2Int startingCell =
                GetRandomEdgeCell(availableEdges[i]);

            CreatePath(startingCell, centre);
        }
    }

    
    private Vector2Int GetRandomEdgeCell(int edge)
    {
        int randomX = Random.Range(2, gridSizeX - 2);
        int randomZ = Random.Range(2, gridSizeZ - 2);

        return edge switch
        {
            0 => new Vector2Int(0, randomZ),
            1 => new Vector2Int(gridSizeX - 1, randomZ),
            2 => new Vector2Int(randomX, 0),
            _ => new Vector2Int(randomX, gridSizeZ - 1)
        };
    }

    
    private void CreatePath(
        Vector2Int startingCell,
        Vector2Int destination)
    {
        Vector2Int currentCell = startingCell;
        List<Vector3> worldPath = new();

        AddPathCell(currentCell, worldPath);

        while (currentCell != destination)
        {
            bool canMoveAlongX =
                currentCell.x != destination.x;

            bool canMoveAlongZ =
                currentCell.y != destination.y;

            bool moveAlongX;

            if (!canMoveAlongZ)
            {
                moveAlongX = true;
            }
            else if (!canMoveAlongX)
            {
                moveAlongX = false;
            }
            else
            {
                int xDistance = Mathf.Abs(
                    destination.x - currentCell.x
                );

                int zDistance = Mathf.Abs(
                    destination.y - currentCell.y
                );

                if (xDistance > zDistance)
                {
                    moveAlongX = Random.value < 0.65f;
                }
                else if (zDistance > xDistance)
                {
                    moveAlongX = Random.value < 0.35f;
                }
                else
                {
                    moveAlongX = Random.value < 0.5f;
                }
            }

            if (moveAlongX)
            {
                currentCell.x +=
                    currentCell.x < destination.x ? 1 : -1;
            }
            else
            {
                currentCell.y +=
                    currentCell.y < destination.y ? 1 : -1;
            }

            AddPathCell(currentCell, worldPath);
        }

        generatedPaths.Add(worldPath);
    }

    
    private void AddPathCell(
        Vector2Int cell,
        List<Vector3> worldPath)
    {
        pathCells.Add(cell);
        worldPath.Add(GridToWorldPosition(cell));
    }

    
    private void GenerateTerrainMesh()
    {
        List<Vector3> vertices = new();
        List<int> grassTriangles = new();
        List<int> pathTriangles = new();

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                Vector2Int cell = new(x, z);
                bool isPath = pathCells.Contains(cell);

                float height = isPath
                    ? 0f
                    : Random.Range(
                        0.05f,
                        grassHeightVariation
                    );

                AddTile(
                    x,
                    z,
                    height,
                    vertices,
                    isPath
                        ? pathTriangles
                        : grassTriangles
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

        meshRenderer.materials = new[]
        {
            grassMaterial,
            pathMaterial
        };
    }

    
    private void AddTile(
        int x,
        int z,
        float height,
        List<Vector3> vertices,
        List<int> triangles)
    {
        float mapOffsetX =
            (gridSizeX - 1) * tileSize * 0.5f;

        float mapOffsetZ =
            (gridSizeZ - 1) * tileSize * 0.5f;

        float left =
            x * tileSize
            - mapOffsetX
            - tileSize * 0.5f;

        float right = left + tileSize;

        float bottom =
            z * tileSize
            - mapOffsetZ
            - tileSize * 0.5f;

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
            Debug.LogWarning(
                "No placement spot prefab has been assigned."
            );

            return;
        }

        Vector2Int centre = GetCentreCell();
        HashSet<Vector2Int> candidateSet = new();

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (Vector2Int pathCell in pathCells)
        {
            foreach (Vector2Int direction in directions)
            {
                Vector2Int candidate =
                    pathCell + direction;

                if (!IsInsideMap(candidate))
                {
                    continue;
                }

                if (pathCells.Contains(candidate))
                {
                    continue;
                }

                int distanceFromTower =
                    GetManhattanDistance(candidate, centre);

                if (distanceFromTower <= 2)
                {
                    continue;
                }

                candidateSet.Add(candidate);
            }
        }

        List<Vector2Int> candidates =
            new(candidateSet);

        ShuffleCells(candidates);

        int spotsToCreate = Mathf.Min(
            numberOfPlacementSpots,
            candidates.Count
        );

        for (int i = 0; i < spotsToCreate; i++)
        {
            Vector2Int selectedCell = candidates[i];

            Vector3 spawnPosition =
                GridToWorldPosition(selectedCell)
                + Vector3.up * 0.1f;

            GameObject newSpot = Instantiate(
                placementSpotPrefab,
                spawnPosition,
                Quaternion.identity
            );

            newSpot.name =
                $"Defender Placement Spot {i + 1}";

            placementCells.Add(selectedCell);
            generatedPlacementSpots.Add(newSpot);
        }
    }

    
    private void GenerateForestClusters()
    {
        if (forestClusterPrefab == null)
        {
            Debug.LogWarning(
                "No forest cluster prefab has been assigned."
            );

            return;
        }

        Vector2Int centre = GetCentreCell();
        List<Vector2Int> candidates = new();

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                Vector2Int cell = new(x, z);

                if (pathCells.Contains(cell))
                {
                    continue;
                }

                if (placementCells.Contains(cell))
                {
                    continue;
                }

                if (GetManhattanDistance(cell, centre) <= 4)
                {
                    continue;
                }

                if (IsNearPath(cell, forestPathClearance))
                {
                    continue;
                }

                candidates.Add(cell);
            }
        }

        ShuffleCells(candidates);

        foreach (Vector2Int candidate in candidates)
        {
            if (generatedForestClusters.Count >=
                numberOfForestClusters)
            {
                break;
            }

            if (IsTooCloseToForest(candidate))
            {
                continue;
            }

            SpawnForestCluster(candidate);
        }
    }

    
    private void SpawnForestCluster(Vector2Int cell)
    {
        Vector3 spawnPosition =
            GridToWorldPosition(cell);

        spawnPosition.y = forestYOffset;

        float randomRotation =
            Random.Range(0, 4) * 90f;

        float randomScale =
            Random.Range(0.9f, 1.1f) * forestScale;

        GameObject forestCluster = Instantiate(
            forestClusterPrefab,
            spawnPosition,
            Quaternion.Euler(0f, randomRotation, 0f)
        );

        forestCluster.transform.localScale *= randomScale;
        forestCluster.name =
            $"Forest Cluster {cell.x}, {cell.y}";

        forestCells.Add(cell);
        generatedForestClusters.Add(forestCluster);
    }

    private bool IsNearPath(
        Vector2Int cell,
        int clearance)
    {
        foreach (Vector2Int pathCell in pathCells)
        {
            if (GetManhattanDistance(cell, pathCell) <=
                clearance)
            {
                return true;
            }
        }

        return false;
    }

    
    private bool IsTooCloseToForest(Vector2Int candidate)
    {
        foreach (Vector2Int forestCell in forestCells)
        {
            int xDistance = candidate.x - forestCell.x;
            int zDistance = candidate.y - forestCell.y;

            float squaredDistance =
                xDistance * xDistance
                + zDistance * zDistance;

            if (squaredDistance <
                minimumForestSpacing * minimumForestSpacing)
            {
                return true;
            }
        }

        return false;
    }

    
    private void CreatePlaceholderTower()
    {
        if (placeholderTower != null)
        {
            Destroy(placeholderTower);
        }

        placeholderTower = new GameObject("Central Castle");

        placeholderTower.transform.position =
            GridToWorldPosition(GetCentreCell());

        BoxCollider towerCollider =
            placeholderTower.AddComponent<BoxCollider>();

        towerCollider.size = new Vector3(4f, 4f, 4f);
        towerCollider.center = new Vector3(0f, 2f, 0f);

        if (castleVisualPrefab != null)
        {
            GameObject castleVisual = Instantiate(
                castleVisualPrefab,
                placeholderTower.transform
            );

            castleVisual.name = "Castle Visual";
            castleVisual.transform.localPosition =
                castleVisualOffset;

            castleVisual.transform.localRotation =
                Quaternion.identity;

            castleVisual.transform.localScale *=
                castleVisualScale;
        }
        else
        {
            GameObject temporaryCube =
                GameObject.CreatePrimitive(PrimitiveType.Cube);

            temporaryCube.name = "Temporary Castle Cube";
            temporaryCube.transform.SetParent(
                placeholderTower.transform
            );

            temporaryCube.transform.localPosition =
                new Vector3(0f, 2f, 0f);

            temporaryCube.transform.localScale =
                new Vector3(3f, 4f, 3f);

            Collider cubeCollider =
                temporaryCube.GetComponent<Collider>();

            if (cubeCollider != null)
            {
                Destroy(cubeCollider);
            }
        }

        placeholderTower.AddComponent<TowerHealth>();

        TowerAttack towerAttack =
            placeholderTower.AddComponent<TowerAttack>();

        towerAttack.SetProjectilePrefab(projectilePrefab);
    }

    
    private Vector2Int GetCentreCell()
    {
        return new Vector2Int(
            gridSizeX / 2,
            gridSizeZ / 2
        );
    }

    private Vector3 GridToWorldPosition(
        Vector2Int gridPosition)
    {
        float mapOffsetX =
            (gridSizeX - 1) * tileSize * 0.5f;

        float mapOffsetZ =
            (gridSizeZ - 1) * tileSize * 0.5f;

        return new Vector3(
            gridPosition.x * tileSize - mapOffsetX,
            0.1f,
            gridPosition.y * tileSize - mapOffsetZ
        );
    }

    private bool IsInsideMap(Vector2Int cell)
    {
        return cell.x >= 0
               && cell.x < gridSizeX
               && cell.y >= 0
               && cell.y < gridSizeZ;
    }

    private int GetManhattanDistance(
        Vector2Int first,
        Vector2Int second)
    {
        return Mathf.Abs(first.x - second.x)
               + Mathf.Abs(first.y - second.y);
    }

    private void ClearGeneratedPlacementSpots()
    {
        foreach (GameObject spot in generatedPlacementSpots)
        {
            if (spot != null)
            {
                Destroy(spot);
            }
        }

        generatedPlacementSpots.Clear();
    }

    private void ClearGeneratedForestClusters()
    {
        foreach (
            GameObject forestCluster
            in generatedForestClusters)
        {
            if (forestCluster != null)
            {
                Destroy(forestCluster);
            }
        }

        generatedForestClusters.Clear();
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

    private void ShuffleList(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            (list[i], list[randomIndex]) =
                (list[randomIndex], list[i]);
        }
    }
}