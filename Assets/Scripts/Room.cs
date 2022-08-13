using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomType { Undefined, Normal, Start, End }

public class Room {
    public RectInt bounds;
    public RoomType roomType;
    
    public Room(Vector2Int location, Vector2Int size) {
        bounds = new RectInt(location, size);
    }

    public void Initalize(RoomType roomType) {
        // Set up room type and what it contains
        
        // At least differentiate from boss and start point
        this.roomType = roomType;
    }

    public static bool Intersect(Room a, Room b) {
        return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) || ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x)
            || (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) || ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y));
    }
}
