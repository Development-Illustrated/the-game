using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CurvedPlatform : MonoBehaviour
{
    [Header("Reference Objects")]
    [SerializeField] private SpriteRenderer _spriteToWrap;
    [SerializeField] private CircularWorldController _worldController;
    
    [Header("Curve Settings")]
    [SerializeField, Range(3, 50)] private int _segmentCount = 10;
    [SerializeField] private float _arcAngle = 30f;
    
    [Header("Physics Settings")]
    [SerializeField] private float _rigidbodyMass = 1f;
    [SerializeField] private float _rigidbodyGravityScale = 0f;
    
    [Header("Layer Settings")]
    [SerializeField] private string _layerName = "Ground";

    private GameObject _curvedObject;
    private LineRenderer _lineRenderer;
    private PolygonCollider2D _polygonCollider;
    private Rigidbody2D _rigidbody;

    private bool _addRigidbody = true;
    private float _distanceFromWorldCenter = 0f;

    private bool _initialized = false;
    private Vector3 _lastPosition;
    private Quaternion _lastRotation;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        if (_curvedObject == null && _spriteToWrap != null && _worldController != null)
        {
            CreateCurvedSprite();
        }
        
        _initialized = true;
        _lastPosition = transform.position;
        _lastRotation = transform.rotation;
    }

    private void InitializeComponents()
    {
        if (_worldController == null)
        {
            _worldController = Object.FindFirstObjectByType<CircularWorldController>();
        }
        
        // Check if we already have a curved sprite
        Transform existingCurved = transform.Find("CurvedSprite");
        if (existingCurved != null)
        {
            _curvedObject = existingCurved.gameObject;
            _lineRenderer = _curvedObject.GetComponent<LineRenderer>();
            _polygonCollider = _curvedObject.GetComponent<PolygonCollider2D>();
            _rigidbody = _curvedObject.GetComponent<Rigidbody2D>();
        }
    }

    private void Update()
    {
        if (!_initialized || !Application.isPlaying) return;
        
        // Check if the object has moved or rotated during gameplay
        if (transform.position != _lastPosition || transform.rotation != _lastRotation)
        {
            UpdateCurvedSprite();
            _lastPosition = transform.position;
            _lastRotation = transform.rotation;
        }
    }
    
    private void UpdateCurvedSprite()
    {
        // Calculate new distance from world center
        if (_worldController != null)
        {
            Vector2 toObject = transform.position - _worldController.transform.position;
            float currentDistance = toObject.magnitude - _worldController.WorldRadius;
            
            // Update the distance parameter
            _distanceFromWorldCenter = currentDistance;
            
            // Clean up existing curved object
            if (_curvedObject != null)
            {
                Destroy(_curvedObject);
                _curvedObject = null;
                _lineRenderer = null;
                _polygonCollider = null;
                _rigidbody = null;
            }
            
            // Create a new curved sprite at the updated position
            if (_spriteToWrap != null)
            {
                CreateCurvedSprite();
            }
        }
    }
    
    private void CreateCurvedSprite()
    {
        float worldRadius = _worldController.WorldRadius;
        float bendRadius = worldRadius + _distanceFromWorldCenter;

        _curvedObject = new GameObject("CurvedSprite");
        _curvedObject.transform.SetParent(transform);
        _curvedObject.transform.localPosition = Vector3.zero;

        if (!string.IsNullOrEmpty(_layerName))
        {
            int layerIndex = LayerMask.NameToLayer(_layerName);
            if (layerIndex != -1)
            {
                _curvedObject.layer = layerIndex;
            }
            else
            {
                Debug.LogWarning($"Layer '{_layerName}' not found in project settings!");
            }
        }

        _lineRenderer = _curvedObject.AddComponent<LineRenderer>();
        if (_lineRenderer == null)
        {
            Debug.LogError("Failed to add LineRenderer component!");
            return;
        }

        _lineRenderer.positionCount = _segmentCount;
        _lineRenderer.startWidth = _spriteToWrap.bounds.size.y;
        _lineRenderer.endWidth = _spriteToWrap.bounds.size.y;
        _lineRenderer.material = _spriteToWrap.material;
        _lineRenderer.textureMode = LineTextureMode.Stretch;

        // Get the angle of our position relative to world center
        Vector2 directionToWorld = (_worldController.transform.position - transform.position).normalized;
        float mainAngle = Mathf.Atan2(directionToWorld.y, directionToWorld.x) * Mathf.Rad2Deg + 180f;

        Vector3[] positions = new Vector3[_segmentCount];
        float angleStep = _arcAngle / (_segmentCount - 1);
        float startAngle = -_arcAngle / 2f;

        float angleOffset = mainAngle;

        for (int i = 0; i < _segmentCount; i++)
        {
            float angle = startAngle + (angleStep * i);
            float worldAngle = angle + angleOffset;
            float radians = worldAngle * Mathf.Deg2Rad;

            float x = Mathf.Cos(radians) * bendRadius;
            float y = Mathf.Sin(radians) * bendRadius;

            positions[i] = new Vector3(x, y, 0) + _worldController.transform.position;
        }

        _lineRenderer.SetPositions(positions);

        _polygonCollider = _curvedObject.AddComponent<PolygonCollider2D>();
        if (_polygonCollider != null)
        {
            Vector2[] colliderPoints = new Vector2[_segmentCount * 2];

            float halfHeight = _spriteToWrap.bounds.size.y / 2;

            for (int i = 0; i < _segmentCount; i++)
            {
                Vector3 position = positions[i];
                Vector3 localPos = position - _curvedObject.transform.position;

                Vector3 normalDir;
                if (i < _segmentCount - 1)
                {
                    Vector3 nextPos = positions[i + 1];
                    Vector3 tangent = (nextPos - position).normalized;
                    normalDir = new Vector3(-tangent.y, tangent.x, 0).normalized;
                }
                else
                {
                    Vector3 prevPos = positions[i - 1];
                    Vector3 tangent = (position - prevPos).normalized;
                    normalDir = new Vector3(-tangent.y, tangent.x, 0).normalized;
                }

                colliderPoints[i] = localPos + (Vector3)(normalDir * halfHeight);
            }

            for (int i = 0; i < _segmentCount; i++)
            {
                int reverseIndex = _segmentCount - 1 - i;
                Vector3 position = positions[reverseIndex];
                Vector3 localPos = position - _curvedObject.transform.position;

                Vector3 normalDir;
                if (reverseIndex < _segmentCount - 1)
                {
                    Vector3 nextPos = positions[reverseIndex + 1];
                    Vector3 tangent = (nextPos - position).normalized;
                    normalDir = new Vector3(-tangent.y, tangent.x, 0).normalized;
                }
                else
                {
                    Vector3 prevPos = positions[reverseIndex - 1];
                    Vector3 tangent = (position - prevPos).normalized;
                    normalDir = new Vector3(-tangent.y, tangent.x, 0).normalized;
                }

                colliderPoints[_segmentCount + i] = localPos - (Vector3)(normalDir * halfHeight);
            }

            _polygonCollider.points = colliderPoints;
        }
        else
        {
            Debug.LogError("Failed to add PolygonCollider2D component!");
        }

        if (_addRigidbody)
        {
            _rigidbody = _curvedObject.AddComponent<Rigidbody2D>();
            if (_rigidbody != null)
            {
                _rigidbody.bodyType = RigidbodyType2D.Static;
                _rigidbody.mass = _rigidbodyMass;
                _rigidbody.gravityScale = _rigidbodyGravityScale;
                _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
                _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                _rigidbody.sleepMode = RigidbodySleepMode2D.NeverSleep;
                Debug.Log("Added static Rigidbody2D to curved sprite");
            }
            else
            {
                Debug.LogError("Failed to add Rigidbody2D component!");
            }
        }
    }

    [Header("Editor Settings")]
    [SerializeField] private bool _showDebugGizmos = true;

    private Vector3 _lastEditorPosition;
    private Quaternion _lastEditorRotation;

    private void OnValidate()
    {
        if (!Application.isEditor) return;
        
        // Clean up existing children but only in edit mode
        if (!Application.isPlaying)
        {
            Transform[] allChildren = GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildren)
            {
                if (child == transform) continue;
                
                if (child.name == "CurvedSprite")
                {
                    DestroyImmediate(child.gameObject);
                }
            }
            
            _curvedObject = null;
            _lineRenderer = null;
            _polygonCollider = null;
            _rigidbody = null;
            
            if (_spriteToWrap != null)
            {
                if (_worldController == null)
                {
                    _worldController = Object.FindFirstObjectByType<CircularWorldController>();
                }
                
                if (_worldController != null)
                {
                    CreateCurvedSprite();
                    _lastEditorPosition = transform.position;
                    _lastEditorRotation = transform.rotation;
                }
            }
            
            #if UNITY_EDITOR

            // Subscribe to editor update event
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
            #endif
        }
    }

    #if UNITY_EDITOR
    private void OnEditorUpdate()
    {
        if (!Application.isPlaying && this != null)
        {
            // Check if position has changed in editor
            if (transform.position != _lastEditorPosition || transform.rotation != _lastEditorRotation)
            {
                // Calculate new distance from world center
                if (_worldController != null)
                {
                    Vector2 toObject = transform.position - _worldController.transform.position;
                    float currentDistance = toObject.magnitude - _worldController.WorldRadius;
                    
                    _distanceFromWorldCenter = currentDistance;
                    
                    // Clean up existing children
                    Transform[] allChildren = GetComponentsInChildren<Transform>();
                    foreach (Transform child in allChildren)
                    {
                        if (child == transform) continue;
                        
                        if (child.name == "CurvedSprite")
                        {
                            DestroyImmediate(child.gameObject);
                        }
                    }
                    
                    _curvedObject = null;
                    _lineRenderer = null;
                    _polygonCollider = null;
                    _rigidbody = null;
                    
                    if (_spriteToWrap != null)
                    {
                        CreateCurvedSprite();
                    }
                }
                
                _lastEditorPosition = transform.position;
                _lastEditorRotation = transform.rotation;
            }
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from editor update when destroyed
        EditorApplication.update -= OnEditorUpdate;
    }
    #endif

    private void OnDrawGizmos()
    {
        if (!_showDebugGizmos || _curvedObject == null) return;

        Gizmos.color = Color.yellow;
        if (_lineRenderer != null && _lineRenderer.positionCount > 1)
        {
            Vector3[] positions = new Vector3[_lineRenderer.positionCount];
            _lineRenderer.GetPositions(positions);

            for (int i = 0; i < positions.Length - 1; i++)
            {
                Gizmos.DrawLine(positions[i], positions[i + 1]);
            }
        }

        Gizmos.color = Color.cyan;
        if (_worldController != null)
        {
            Gizmos.DrawLine(_curvedObject.transform.position, _worldController.transform.position);
        }
    }
}
