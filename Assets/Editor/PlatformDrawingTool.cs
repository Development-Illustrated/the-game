using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlatformDrawingTool : EditorWindow
{
    private bool isDrawing = false;
    private List<Vector2> currentPoints = new List<Vector2>();
    private CircularWorldController worldController;
    private Material defaultPlatformMaterial;
    private float snapDistance = 0.5f;
    private bool alignToCircle = true;
    private bool continuousMode = false;
    private bool showHelp = true;

    [MenuItem("Tools/Platform Drawing Tool")]
    public static void ShowWindow()
    {
        GetWindow<PlatformDrawingTool>("Platform Tool");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        worldController = FindFirstObjectByType<CircularWorldController>();
        defaultPlatformMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
        
        // Create Editor folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Editor"))
        {
            AssetDatabase.CreateFolder("Assets", "Editor");
            AssetDatabase.Refresh();
        }
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        if (worldController == null)
        {
            worldController = FindFirstObjectByType <CircularWorldController>();
            if (worldController == null)
            {
                EditorGUILayout.HelpBox("No CircularWorldController found in the scene!", MessageType.Error);
                return;
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Platform Drawing Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        defaultPlatformMaterial = (Material)EditorGUILayout.ObjectField("Platform Material", defaultPlatformMaterial, typeof(Material), false);
        snapDistance = EditorGUILayout.Slider("Snap Distance", snapDistance, 0.1f, 2f);
        alignToCircle = EditorGUILayout.Toggle("Align to Circle", alignToCircle);
        continuousMode = EditorGUILayout.Toggle("Continuous Drawing", continuousMode);
        showHelp = EditorGUILayout.Toggle("Show Help", showHelp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Status: " + (isDrawing ? "Drawing Platform" : "Not Drawing"));
        EditorGUILayout.LabelField("Points: " + currentPoints.Count);

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Start Drawing"))
        {
            isDrawing = true;
            currentPoints.Clear();
        }

        if (GUILayout.Button("Cancel"))
        {
            isDrawing = false;
            currentPoints.Clear();
            SceneView.RepaintAll();
        }
        
        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginDisabledGroup(currentPoints.Count < 2);
        if (GUILayout.Button("Finish Platform"))
        {
            FinishDrawing();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Click in Scene view to add points.\n" +
            "Hold Shift to snap to existing platforms.\n" +
            "Press Escape to cancel.\n" + 
            "Press Enter to finish drawing.", 
            MessageType.Info);
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!isDrawing) return;

        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Vector3 mousePosition = ray.origin;
        mousePosition.z = 0; // Ensure we're in 2D space

        // Snap to circle if enabled
        if (alignToCircle)
        {
            Vector2 toCenter = (Vector2)worldController.transform.position - (Vector2)mousePosition;
            float currentDistance = toCenter.magnitude;
            float angle = Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg;
            mousePosition = worldController.GetPointOnCircle(angle + 180, currentDistance);
        }

        // Check for snapping to existing platforms
        if (e.shift)
        {
            Platform[] allPlatforms = FindObjectsByType<Platform>(FindObjectsSortMode.None);
            float closestDistance = float.MaxValue;
            Vector2 closestPoint = Vector2.zero;
            bool foundSnapPoint = false;

            foreach (Platform platform in allPlatforms)
            {
                List<Vector2> platformPoints = platform.GetPoints();
                foreach (Vector2 point in platformPoints)
                {
                    Vector2 worldPoint = platform.transform.TransformPoint(point);
                    float distance = Vector2.Distance(worldPoint, mousePosition);
                    if (distance < snapDistance && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPoint = worldPoint;
                        foundSnapPoint = true;
                    }
                }
            }

            if (foundSnapPoint)
            {
                mousePosition = closestPoint;
            }
        }

        // Draw the current platform preview
        Handles.color = Color.yellow;
        if (currentPoints.Count > 0)
        {
            Handles.DrawLine(currentPoints[currentPoints.Count - 1], mousePosition);
            
            // Draw all lines between points
            for (int i = 0; i < currentPoints.Count - 1; i++)
            {
                Handles.DrawLine(currentPoints[i], currentPoints[i + 1]);
            }
            
            // Draw points
            for (int i = 0; i < currentPoints.Count; i++)
            {
                Handles.color = (i == 0) ? Color.green : (i == currentPoints.Count - 1) ? Color.red : Color.yellow;
                Handles.DrawSolidDisc(currentPoints[i], Vector3.forward, 0.1f);
            }
        }

        // Show help text in scene view
        if (showHelp)
        {
            Handles.BeginGUI();
            GUI.Label(new Rect(10, 10, 300, 60), 
                "Left Click: Add Point\n" +
                "Shift: Snap to Existing Platforms\n" +
                "Enter: Finish | Escape: Cancel");
            Handles.EndGUI();
        }

        // Handle mouse click to add points
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (!e.alt) // Prevent adding points when alt is pressed (camera controls)
            {
                currentPoints.Add(mousePosition);
                e.Use();
                sceneView.Repaint();
            }
        }
        
        // Handle key presses
        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Escape)
            {
                isDrawing = false;
                currentPoints.Clear();
                e.Use();
                sceneView.Repaint();
                Repaint();
            }
            else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            {
                if (currentPoints.Count >= 2)
                {
                    FinishDrawing();
                    e.Use();
                }
            }
        }
        
        // Force the scene view to repaint continuously
        sceneView.Repaint();
    }

    private void FinishDrawing()
    {
        if (currentPoints.Count < 2) return;

        // Create new GameObject with Platform component
        GameObject platformObj = new GameObject("Platform");
        platformObj.transform.position = Vector3.zero;
        
        Platform platform = platformObj.AddComponent<Platform>();
        EdgeCollider2D edgeCollider = platformObj.GetComponent<EdgeCollider2D>();
        MeshFilter meshFilter = platformObj.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = platformObj.GetComponent<MeshRenderer>();
        
        // Set the material
        if (defaultPlatformMaterial != null)
        {
            meshRenderer.material = defaultPlatformMaterial;
        }
        
        // Convert positions from world space to local space
        List<Vector2> localPoints = new List<Vector2>();
        foreach (Vector2 point in currentPoints)
        {
            localPoints.Add(point - (Vector2)platformObj.transform.position);
        }
        
        platform.SetPoints(localPoints);
        
        // Register undo
        Undo.RegisterCreatedObjectUndo(platformObj, "Create Platform");
        
        // Reset for next platform or end drawing
        if (continuousMode)
        {
            currentPoints.Clear();
        }
        else
        {
            isDrawing = false;
            currentPoints.Clear();
        }
        
        Repaint();
    }
}
