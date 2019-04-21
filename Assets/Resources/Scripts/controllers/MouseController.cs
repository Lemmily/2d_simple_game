using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class MouseController : MonoBehaviour
{

    public GameObject circleCursorPrefab;
    private Vector3 dragStartPosition;
    private Vector3 currentMousePos;
    Vector3 lastMousePosition;

    List<GameObject> dragPlaceholderPreviews;
    private bool objectTypeNotDraggable;
    private bool isDragging = false;


    FurnitureSpriteController fsc;


    enum MouseMode
    {
        BUILD,
        SELECT
    }

    MouseMode currentMode = MouseMode.SELECT;


    // Use this for initialization
    void Start() {
        dragPlaceholderPreviews = new List<GameObject>();
    }

    public Vector3 GetMousePosition() {
        return currentMousePos;
    }


    public Tile GetMouseOverTile() {
        return WorldController.Instance.GetTileAtWorldCoord(GetMousePosition() );
    }


    // Update is called once per frame
    void Update() {

        currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentMousePos.z = 0;


        if ( Input.GetKeyUp(KeyCode.Escape))
        {
            if (currentMode == MouseMode.BUILD)
            {
                currentMode = MouseMode.SELECT;
            } else if (currentMode == MouseMode.SELECT)
            {
                Debug.Log("Show game menu ??");
            }
        }

        if(Input.GetKeyUp(KeyCode.Mouse1) || Input.GetKeyUp(KeyCode.Q)) {
            if(currentMode == MouseMode.BUILD) {
                currentMode = MouseMode.SELECT;
            }
        }

        Tile tileUnderMouse = WorldController.Instance.GetTileAtWorldCoord(currentMousePos);

        UpdateCameraMovement(currentMousePos);
        UpdateDragging(currentMousePos);

        lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastMousePosition.z = 0;
    }

    private void UpdateDragging(Vector3 currentMousePos) {
        //if over UI - bail!

        if (EventSystem.current.IsPointerOverGameObject()) {
            return;
        }

        //start dragging.
        if (Input.GetMouseButtonDown(0)) {
            //start making a list of affected tiles.
            dragStartPosition = currentMousePos;
            isDragging = true;
        } else if (isDragging == false)
        {
            dragStartPosition = currentMousePos;
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKey(KeyCode.Escape))
        {
            // the RIGHT-hand mouse button was released, 
            isDragging = false;
        }


        //clean up old placeholders
        while (dragPlaceholderPreviews.Count > 0)
        {
            GameObject go = dragPlaceholderPreviews[0];
            dragPlaceholderPreviews.RemoveAt(0);
            SimplePool.Despawn(go);
        }

        if (!BuildModeController.Instance.IsObjectDraggable())
        {
            dragStartPosition = currentMousePos;
        }

        if (currentMode != MouseMode.BUILD)
        {
            return;
        }



        int start_x =   Mathf.FloorToInt(dragStartPosition.x + 0.5f);
        int end_x =     Mathf.FloorToInt(currentMousePos.x + 0.5f);
        int start_y =   Mathf.FloorToInt(dragStartPosition.y + 0.5f);
        int end_y =     Mathf.FloorToInt(currentMousePos.y + 0.5f);

        if (end_x < start_x) {
            int tmp = end_x;
            end_x = start_x;
            start_x = tmp;
        }

        if (end_y < start_y) {
            int tmp = end_y;
            end_y = start_y;
            start_y = tmp;
        }


        //Is dragging
        //display preview of drag area.
        for (int x = start_x; x <= end_x; x++) {
            for (int y = start_y; y <= end_y; y++) {
                Tile t = WorldController.Instance.world.GetTileAt(x, y);
                if (t != null) {
                    if (BuildModeController.Instance.buildMode == BuildMode.FLOOR || BuildModeController.Instance.buildMode == BuildMode.DECONSTRUCT)
                    {
                        GameObject go = SimplePool.Spawn(circleCursorPrefab, new Vector3(x, y, 0), Quaternion.identity);
                        go.transform.parent = this.transform;
                        dragPlaceholderPreviews.Add(go);
                    } else
                    {
                        ShowFurnitureSpriteAtCoordinate(BuildModeController.Instance.objectBuildType, t);
                    }
                }
            }
        }
        

        //end dragging
        if (isDragging && Input.GetMouseButtonUp(0)) {

            isDragging = false;

            for (int x = start_x; x <= end_x; x++) {
                for (int y = start_y; y <= end_y; y++) {
                    Tile t = WorldController.Instance.world.GetTileAt(x, y);
                    if (t != null) {
                        BuildModeController.Instance.DoBuild(t);
                    }
                }
            }
        } 
    }


    void ShowFurnitureSpriteAtCoordinate(string furnitureType, Tile t)
    {
        GameObject go = new GameObject();
        go.transform.SetParent(this.transform, true);
        dragPlaceholderPreviews.Add(go);
        
  
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = ResourceLoader.GetFurnitureSprite(furnitureType);
        sr.sortingLayerName = "Jobs";

        if (WorldController.Instance.world.IsFurniturePlacementValid(furnitureType, t))
        {
            sr.color = new Color(0.8f, 1f, 0.8f, 0.4f);
        }
        else
        {
            sr.color = new Color(1f, 0.5f, 0.8f, 0.4f);
        }

        Furniture proto = t.world.furnitureProto[furnitureType];
        go.transform.position = new Vector3(t.X + ((proto.Width - 1) / 2f), t.Y + (proto.Height - 1) / 2f, 0);

    }


    private void UpdateCameraMovement(Vector3 currentMousePos) {
        //screen dragging
        if (Input.GetMouseButton(2)) {

            Vector3 difference = lastMousePosition - currentMousePos;
            Camera.main.transform.position = Camera.main.transform.localPosition + difference;
        }

        Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel");

        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 1, 25);
    }


    public void StartBuildMode()
    {
        currentMode = MouseMode.BUILD;
    }
}
