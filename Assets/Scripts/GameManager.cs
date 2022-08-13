using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private DungeonGenerator2D dungeonGenerator;
    [SerializeField] private PlayerControls playerControls;
    [SerializeField] private Player player;
    public static GameManager instance;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Get controls
        playerControls = new PlayerControls();
        playerControls.Enable();
    }

    private void Start() {
        // Find dungeon gen
        dungeonGenerator = GameObject.FindObjectOfType<DungeonGenerator2D>();
        dungeonGenerator.GenerateDungeon();

        // Find player
        player = GameObject.FindObjectOfType<Player>();
        // Move player to starting room
        player.transform.position = dungeonGenerator.GetStartingRoom().bounds.center;
    }

    public Player GetPlayer() {
        return player;
    }
    
    public PlayerControls GetInputActions() {
        return playerControls;
    }
}
