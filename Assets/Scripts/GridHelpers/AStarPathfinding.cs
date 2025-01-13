using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AStarPathfinding : MonoBehaviour {
    public static AStarPathfinding Instance;

    private readonly HashSet<Vector2Int> _obstaclePositions = new HashSet<Vector2Int>(); // List of obstacles in the form of grid positions

    private Dictionary<Vector2Int, Node> _grid;
    private readonly HashSet<Node> _openList = new HashSet<Node>();
    private readonly HashSet<Node> _closedList = new HashSet<Node>();

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        StartCoroutine(WaitAndInit());
    }

    private IEnumerator WaitAndInit() {
        yield return new WaitForEndOfFrame();
        InitializeGrid();
        InvokeRepeating(nameof(UpdateWalkable), 0, 1);
    }

    // Initialize the grid with walkable and blocked nodes
    private void InitializeGrid() {
        Rect rect = GridManager.Instance.GridSize;
        Vector2Int min = new Vector2Int((int)rect.x, (int)rect.y);
        Vector2Int max = new Vector2Int((int)rect.width, (int)rect.height);
        _grid = new Dictionary<Vector2Int, Node>((max.x-min.x) * (max.y-min.y));
       
        for (int x = min.x; x < max.x; x++) {
            for (int y = min.x; y < max.y; y++) {
                Vector2Int position = new Vector2Int(x, y);
                _grid.Add(position, new Node(position, true));
            }
        }
    }

    private void FindObstacles() {
        _obstaclePositions.Clear();
        Gridable[] r = FindObjectsByType<Gridable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Where(r => r.IsBlockingPath)
            .ToArray();
        foreach (Gridable gridable in r) {
            foreach (Vector2Int blockedPos in gridable.GetOccupiedPositions()) {
                _obstaclePositions.Add(blockedPos);
            }
        }
    }

    private void UpdateWalkable() {
        FindObstacles();
        foreach (Vector2Int key in _grid.Keys) {
            _grid[key].Walkable = true; // Assume walkable by default
        }

        foreach (Vector2Int obstacle in _obstaclePositions.Where(obstacle => _grid.ContainsKey(obstacle))) {
            _grid[obstacle].Walkable = false;
        }
    }

    // The A* pathfinding method
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end) {
        Node startNode = _grid[start];
        Node targetNode = _grid[end];

        _openList.Clear();
        _closedList.Clear();

        _openList.Add(startNode);

        while (_openList.Count > 0) {
            // Get the node with the lowest fCost
            Node currentNode = GetNodeWithLowestFCost(_openList);
            _openList.Remove(currentNode);
            _closedList.Add(currentNode);

            // If we reach the target, reconstruct the path
            if (currentNode.PosX == targetNode.PosX && currentNode.PosY == targetNode.PosY) {
                return RetracePath(startNode, currentNode);
            }

            // Evaluate each of the neighbors
            foreach (Node neighbor in GetNeighbors(currentNode)) {
                if (!neighbor.Walkable || _closedList.Contains(neighbor))
                    continue;

                short newGCost = (short)(currentNode.GCost + GetDistance(currentNode, neighbor));
                if (newGCost >= neighbor.GCost && _openList.Contains(neighbor)) {
                    continue;
                }

                neighbor.GCost = newGCost;
                neighbor.HCost = GetDistance(neighbor, targetNode);
                neighbor.Parent = currentNode;

                _openList.Add(neighbor);
            }
        }

        return new List<Vector2Int>(); // Return an empty path if no path is found
    }

    // Get the node with the lowest fCost from the open list
    private Node GetNodeWithLowestFCost(HashSet<Node> list) {
        Node lowestFCostNode = list.First();
        foreach (Node node in list.Where(node => node.FCost < lowestFCostNode.FCost)) {
            lowestFCostNode = node;
        }

        return lowestFCostNode;
    }

    // Get the neighbors of a node (up, down, left, right)
    private HashSet<Node> GetNeighbors(Node node) {
        HashSet<Node> neighbors = new HashSet<Node>();

        Vector2Int[] directions = new Vector2Int[] {
            new Vector2Int(0, 1), // Up
            new Vector2Int(1, 0), // Right
            new Vector2Int(0, -1), // Down
            new Vector2Int(-1, 0), // Left
        };

        foreach (Vector2Int direction in directions) {
            Vector2Int neighborPos = new Vector2Int(node.PosX, node.PosY) + direction;
            if (IsValidPosition(neighborPos)) {
                neighbors.Add(_grid[neighborPos]);
            }
        }

        return neighbors;
    }

    // Check if a position is within the bounds of the grid
    private bool IsValidPosition(Vector2Int position) {
        return _grid.ContainsKey(position);
    }

    // Get the Manhattan distance (heuristic) between two nodes
    private short GetDistance(Node a, Node b) {
        return (short)(Mathf.Abs(a.PosX - b.PosX) + Mathf.Abs(a.PosY - b.PosY));
    }

    // Retrace the path from the target to the start
    private List<Vector2Int> RetracePath(Node startNode, Node endNode) {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        while (currentNode != startNode) {
            path.Add(new Vector2Int(currentNode.PosX, currentNode.PosY));
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path;
    }
}