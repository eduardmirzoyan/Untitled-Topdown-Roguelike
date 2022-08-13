using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;
using Graphs;

public class DungeonGenerator2D : MonoBehaviour {
    enum CellType { None, Room, Hallway };

    [SerializeField] private Vector2Int size;
    [SerializeField] private int roomCount;
    [SerializeField] private Vector2Int roomMaxSize;
    [SerializeField] private Vector2Int roomMinSize;
    [SerializeField] private int bufferSize = 2;
    [SerializeField] private int corridorWidth;
    [SerializeField] private float keepEdgeChance = 0.125f;
    [SerializeField] private int seed = 0;

    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private RuleTile groundTile;
    [SerializeField] private RuleTile wallTile;

    private Random random;
    private Grid2D<CellType> grid;
    private List<Room> rooms;
    private Delaunay2D delaunay;
    private HashSet<Prim.Edge> selectedEdges;

    private Room startingRoom;

    public void GenerateDungeon() {
        if (bufferSize < 0) {
            throw new System.Exception("BufferSize is negative: " + bufferSize);
        }

        if (seed < 0) {
            // If seed is negative, then choose random seed
            seed = UnityEngine.Random.Range(0, int.MaxValue);
        }
        random = new Random(seed);
        grid = new Grid2D<CellType>(size, Vector2Int.zero);
        rooms = new List<Room>();
        startingRoom = null;

        PlaceRooms();
        Triangulate();
        CreateHallways();
        PathfindHallways();
        
        GenerateWalls();
        SetupRooms();
    }

    private void SetupRooms()
    {
        SetStartRoom();
        SetEndRoom();
    }

    private void SetStartRoom() {
        Vector2 corner = new Vector2(0, 0);
        float minDistance = float.MaxValue;

        // Find room closest to bottom left
        foreach (var room in rooms) {
            var distance = Vector2.Distance(corner, room.bounds.center);
            if (distance < minDistance) {
                minDistance = distance;
                startingRoom = room;
            }
        }

        // Check to see if room was set
        if (startingRoom == null) {
            throw new System.Exception("Starting room not set");
        }

        // Set starting room
        startingRoom.Initalize(RoomType.Start);
    }

    private void SetEndRoom() {
        Vector2 corner = new Vector2(size.x, size.y);
        Room endingRoom = null;
        float minDistance = float.MaxValue;

        // Find room closest to bottom left
        foreach (var room in rooms) {
            var distance = Vector2.Distance(corner, room.bounds.center);
            if (distance < minDistance) {
                minDistance = distance;
                endingRoom = room;
            }
        }

        // Check to see if room was set
        if (endingRoom == null) {
            throw new System.Exception("Starting room not set");
        }

        // Set ending room
        endingRoom.Initalize(RoomType.End);
    }

    private void PlaceRooms() {
        // Makes sure there are roomCount amount of rooms
        // DANGEROUS: IF THERE IS A CASE WHERE A NEW ROOM CANNOT BE ADDED, CAN BE INFINITE LOOP
        while (rooms.Count < roomCount) { // int i = 0; i < roomCount; i++
            // Generate random room location
            Vector2Int location = new Vector2Int(
                random.Next(0, size.x),
                random.Next(0, size.y)
            );

            // Generate random room size
            Vector2Int roomSize = new Vector2Int(
                random.Next(roomMinSize.x, roomMaxSize.x + 1),
                random.Next(roomMinSize.y, roomMaxSize.y + 1)
            );

            bool add = true;
            Room newRoom = new Room(location, roomSize);

            // Buffer to ensure that rooms do not touch
            Room buffer = new Room(location + new Vector2Int(-bufferSize / 2, -bufferSize / 2), roomSize + new Vector2Int(bufferSize, bufferSize));

            foreach (var room in rooms) {
                if (Room.Intersect(room, buffer)) {
                    add = false;
                    break;
                }
            }

            if (newRoom.bounds.xMin - bufferSize / 2 < 0 || newRoom.bounds.xMax + bufferSize / 2 >= size.x
                || newRoom.bounds.yMin - bufferSize / 2 < 0 || newRoom.bounds.yMax + bufferSize / 2 >= size.y) {
                add = false;
            }

            if (add) {
                rooms.Add(newRoom);
                // Generate tiles
                PlaceRoom(newRoom.bounds.position, newRoom.bounds.size);

                foreach (var pos in newRoom.bounds.allPositionsWithin) {
                    grid[pos] = CellType.Room;
                }
            }
        }
    }

    private void Triangulate() {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var room in rooms) {
            vertices.Add(new Vertex<Room>((Vector2)room.bounds.position + ((Vector2)room.bounds.size) / 2, room));
        }

        delaunay = Delaunay2D.Triangulate(vertices);
    }

    private void CreateHallways() {
        List<Prim.Edge> edges = new List<Prim.Edge>();

        foreach (var edge in delaunay.Edges) {
            edges.Add(new Prim.Edge(edge.U, edge.V));
        }

        List<Prim.Edge> mst = Prim.MinimumSpanningTree(edges, edges[0].U);

        selectedEdges = new HashSet<Prim.Edge>(mst);
        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(selectedEdges);

        foreach (var edge in remainingEdges) {
            // Roll to see if an edge should be kept, (and loop is made)
            var roll = random.NextDouble();
            if (roll < keepEdgeChance) {
                selectedEdges.Add(edge);
            }
        }
    }

    private void PathfindHallways() {
        DungeonPathfinder2D aStar = new DungeonPathfinder2D(size);

        foreach (var edge in selectedEdges) {
            var startRoom = (edge.U as Vertex<Room>).Item;
            var endRoom = (edge.V as Vertex<Room>).Item;

            var startPosf = startRoom.bounds.center;
            var endPosf = endRoom.bounds.center;
            var startPos = new Vector2Int((int)startPosf.x, (int)startPosf.y);
            var endPos = new Vector2Int((int)endPosf.x, (int)endPosf.y);

            var path = aStar.FindPath(startPos, endPos, (DungeonPathfinder2D.Node a, DungeonPathfinder2D.Node b) => {
                var pathCost = new DungeonPathfinder2D.PathCost();
                
                // Heuristic cost
                pathCost.cost = Vector2Int.Distance(b.Position, endPos);    

                if (grid[b.Position] == CellType.Room) {
                    pathCost.cost += 10;
                } else if (grid[b.Position] == CellType.None) {
                    pathCost.cost += 5;
                } else if (grid[b.Position] == CellType.Hallway) {
                    pathCost.cost += 1;
                }

                pathCost.traversable = true;

                return pathCost;
            });

            if (path != null) {
                for (int i = 0; i < path.Count; i++) {
                    var current = path[i];

                    if (grid[current] == CellType.None) {
                        grid[current] = CellType.Hallway;
                    }

                    if (i > 0) {
                        var prev = path[i - 1];

                        var delta = current - prev;
                    }
                }

                foreach (var pos in path) {
                    if (grid[pos] == CellType.Hallway) {
                        PlaceHallway(pos);
                    }
                }
            }
        }
    }

    private void PlaceRoom(Vector2Int location, Vector2Int size) {
        for (int i = 0; i < size.x; i++) {
            for (int j = 0; j < size.y; j++) {
                Vector3Int position = (Vector3Int) (location + new Vector2Int(i, j));
                groundTilemap.SetTile(position, groundTile);
            }
        }
    }

    private void PlaceHallway(Vector2Int location) {
        // Place 3x3
        for (int i = -corridorWidth; i <= corridorWidth; i++) {
            for (int j = -corridorWidth; j <= corridorWidth; j++) {
                // Set tile
                Vector3Int position = (Vector3Int) (location + new Vector2Int(i, j));
                groundTilemap.SetTile(position, groundTile);
            }
        }
    }

    private void GenerateWalls() {
        for (int i = 0; i < size.x; i++) {
            for (int j = 0; j < size.y; j++) {
                // If there is no ground there, then place wall
                if (!groundTilemap.HasTile(new Vector3Int(i, j, 0))) {
                    wallTilemap.SetTile(new Vector3Int(i, j, 0), wallTile);
                }
            }
        }
    }

    public Room GetStartingRoom() {
        return startingRoom;
    }

    private void OnDrawGizmosSelected() {
        if (rooms != null) {
            foreach (var room in rooms) {
                switch(room.roomType) {
                    case RoomType.Undefined:
                    Gizmos.color = Color.blue;
                    break;
                    case RoomType.Start:
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(room.bounds.center, 1f);
                    break;
                    case RoomType.Normal:
                    Gizmos.color = Color.green;
                    break;
                    case RoomType.End:
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(room.bounds.center, 1f);
                    break;
                }
                Gizmos.DrawWireCube(room.bounds.center, (Vector3Int)room.bounds.size);
            }
        }
    }
}
