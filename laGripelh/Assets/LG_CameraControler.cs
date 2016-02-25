using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using HoneyFramework;

public class LG_CameraControler : CameraControler
{
    Plane        plane = new Plane(Vector3.up, Vector3.zero);
    Vector3i     selected_hex;
    Formation    selected_unit;

    /* Copied from HoneyFramework
     * Allows to control quality level.
     */
    void UpdateTeselationLevel(float height)
    {
        int tesselationLevel = (int)Mathf.Max(3.1f, Mathf.Min(16.0f, 25 - height));
        foreach (KeyValuePair<Vector2i, Chunk> pair in World.GetInstance().chunks)
        {
            if (pair.Value.chunkObject != null)
            {
                MeshRenderer mr = pair.Value.chunkObject.GetComponent<MeshRenderer>();
                mr.material.SetInt("_Tess", tesselationLevel);
            }
        }
    }

    /* Called automaticaly every X second (with x = project timestep)
     * This is where you get user input and act accordingly.
     */
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            return;
        }

        if (World.instance == null) return;

        Vector3 v = transform.position;

        UpdateTeselationLevel(v.y);

        float vertical = Input.GetAxis("Vertical") * speed;
        float horizontal = Input.GetAxis("Horizontal") * speed;

        // Move the camera with arrows/WASD
        if (Input.touchCount > 0)
        {
            Vector2 move = Input.GetTouch(0).deltaPosition;
            vertical += -move.y;
            horizontal += -move.x;
        }
        vertical *= Time.deltaTime;
        horizontal *= Time.deltaTime;

        v.x += horizontal;
        v.z += vertical;

        // Apply the camera movement
        transform.position = v;

        //part which allows to find "click" position in world
        if (World.GetInstance() != null && Input.GetMouseButtonDown(0) && World.GetInstance().status == World.Status.Ready)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float ent = 100.0f;
            if (plane.Raycast(ray, out ent))
            {
                // Get clicked hex coordinates
                Vector3 hitPoint = ray.GetPoint(ent);
                hitPoint -= World.instance.transform.position;
                Vector2 hexWorldPosition = VectorUtils.Vector3To2D(hitPoint);
                Vector3i hexPos = HexCoordinates.GetHexCoordAt(hexWorldPosition);

                // Select an unit by clicking on it
                foreach(Formation unit in GameManager.instance.formations)
                {
                    if (unit.position == hexPos)
                    {
                        Debug.Log("Selecting unit");
                        Debug.Log("Last = " + selected_hex);
                        Debug.Log("Curr = " + hexPos);

                        // Markers
                        HexMarkers.ClearMarkerType(selected_hex, HexMarkers.MarkerType.Friendly);
                        HexMarkers.SetMarkerType(hexPos, HexMarkers.MarkerType.Friendly);

                        Debug.Log("Clicked on unit at " + unit.position);
                        selected_hex = hexPos;

                        selected_unit = unit;
                    }
                }

                // Move selected unit
                if (selected_unit && selected_unit.position != hexPos)
                {
                    Debug.Log("Moving unit");
                    Debug.Log("Last = " + selected_hex);
                    Debug.Log("Curr = " + hexPos);
                    HexMarkers.ClearMarkerType(selected_hex, HexMarkers.MarkerType.Friendly);
                    HexMarkers.SetMarkerType(hexPos, HexMarkers.MarkerType.Friendly);
                    selected_unit.GoTo(hexPos);
                    selected_hex = hexPos;
                }
            }
            else
            {
                Debug.LogWarning("click outside world? e.g. horizontal");
            }
        }
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("", GUILayout.Width(Screen.width - 150));

        GUILayout.BeginVertical();
        GUILayout.Label("Status: " + World.GetInstance().status.ToString());
        GUILayout.Label("DX Mode: " + (MHGameSettings.GetDx11Mode() ? "DX11" : "Non DX11"));

        //feedback for world status
        if (World.GetInstance().status == World.Status.NotReady)
        {
            if (GUILayout.Button("Generate World"))
            {
                DataManager.Reload();
                World.GetInstance().Initialize();
                AstarPath.RegisterSafeUpdate(GameManager.instance.ActivatePathfinder);
            }

            if (GUILayout.Button("Load map"))
            {
                DataManager.Reload();
                World.GetInstance().InitializeFromSave();
                AstarPath.RegisterSafeUpdate(GameManager.instance.ActivatePathfinder);
            }
        }
        else if (World.GetInstance().status == World.Status.Ready)
        {
            if (GUILayout.Button("Generate World"))
            {
                DataManager.Reload();
                World.GetInstance().Initialize();
                AstarPath.RegisterSafeUpdate(GameManager.instance.ActivatePathfinder);
            }

            if (GUILayout.Button("Load map"))
            {
                DataManager.Reload();
                World.GetInstance().InitializeFromSave();
                AstarPath.RegisterSafeUpdate(GameManager.instance.ActivatePathfinder);
            }

            if (GUILayout.Button("Spawn character"))
            {
                Formation f = Formation.CreateFormation("Caps", Vector3i.zero);
                for (int i = 0; i < 10; i++)
                {
                    f.AddCharacter(CharacterActor.CreateCharacter("Characters/CapCharacter", 0.3f));
                }
                GameManager.instance.formations.Add(f);
            }

            if (GUILayout.Button("Save map"))
            {
                SaveManager.Save(World.GetInstance(), false);
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }
}