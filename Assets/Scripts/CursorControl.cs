using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorControl : MonoBehaviour
{
    [SerializeField] private PlayerControls playerControls;
    [SerializeField] private Player player;

    public static CursorControl instance;

    private void Awake() {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        playerControls = GameManager.instance.GetInputActions();
        Cursor.visible = false;
    }

    private void Update()
    {
        Vector3 realMousePosition = playerControls.Player.Aim.ReadValue<Vector2>();
        realMousePosition.z = 10;
        Vector3 cursorPos = Camera.main.ScreenToWorldPoint(realMousePosition);
        transform.position = cursorPos;

        Vector3 direction = (cursorPos - player.transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.eulerAngles = new Vector3(0, 0, angle);
    }
}
