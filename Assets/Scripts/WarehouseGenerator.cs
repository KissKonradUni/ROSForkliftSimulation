using System;
using UnityEngine;

/// <summary>
/// Basic enum handling the different game modes. Mostly used to change the generation properties.
/// </summary>
public enum JobType
{
    BoxSorting,
    BoxSearching
}

/// <summary>
/// The main MonoBehaviour handling the generation of the warehouse.
/// </summary>
public class WarehouseGenerator : MonoBehaviour
{
    [Header("References")] 
    public GameObject wall;
    public GameObject cornerWall;
    public GameObject roof;
    public GameObject windowRoof;
    public GameObject shelf;
    public GameObject[] boxes;
    [Header("Properties")]
    public JobType jobType;
    public Vector2 buildingSize = new Vector2(24, 48);
    
    public int shelfColumns = 3;
    public int shelfRows = 6;
    
    /// <summary>
    /// Automatically calculated from the room y size and the amount of shelf rows.
    /// </summary>
    [SerializeField]
    [HideInInspector]
    public float distanceBetweenShelves = 2.0f;
    
    /// <summary>
    /// Automatically calculated from the room x size and the amount of shelf columns.
    /// </summary>
    [SerializeField]
    [HideInInspector] 
    public float distanceToWall = 2.0f;
    
    /// <summary>
    /// Stores whether to use a type of box, or not.
    /// </summary>
    [SerializeField]
    [HideInInspector]
    public bool[] boxTypes = new bool[Boxes.Consts.TypeCount];
    
    /// <summary>
    /// Stores the weight at which to generate said boxes.
    /// </summary>
    [SerializeField]
    [HideInInspector]
    public float[] boxWeights = new float[Boxes.Consts.TypeCount];

    /// <summary>
    /// Stores the different zone's sizes.
    /// </summary>
    [SerializeField] 
    [HideInInspector] 
    public int[] zoneSizes = new int[Boxes.Consts.TypeCount + 1];

    private void Awake()
    {
        GenerateRoom();
    }

    /// <summary>
    /// The main function used in generating the warehouse.
    /// Automatically called when running the simulation,
    /// but can be explicitly called from within the editor. 
    /// </summary>
    public void GenerateRoom()
    {
        // Prepares some basic gameObjects to use for sorting the elements making up the warehouse.
        #region Preparations

        var root = GameObject.Find("Warehouse");
        if (root != null)
            DestroyImmediate(root);
        root = new GameObject("Warehouse");
        var rootTransform = root.transform;

        var walls = new GameObject("Walls");
        walls.transform.SetParent(rootTransform);
        var wallsTransform = walls.transform;
        
        var roofs = new GameObject("Roofs");
        roofs.transform.SetParent(rootTransform);
        var roofsTransform = roofs.transform;
        
        var shelves = new GameObject("Shelves");
        shelves.transform.SetParent(rootTransform);
        var shelvesTransform = shelves.transform;

        #endregion

        // Generates the walls using 4m long and 9m high walls.
        // Using this wall size was necessary to keep down the vertex count,
        // while maintaining aesthetics.
        #region Walls

        for (var x = buildingSize.x / -2 + 4; x <= buildingSize.x / 2 - 4; x += 4)
        {
            Instantiate(wall, new Vector3(x, 0, buildingSize.y / 2 ), Quaternion.Euler(-90, -90, 0), wallsTransform);
            Instantiate(wall, new Vector3(x, 0, -buildingSize.y / 2), Quaternion.Euler(-90, 90, 0),  wallsTransform);
        }
        
        for (var y = buildingSize.y / -2 + 4; y <= buildingSize.y / 2 - 4; y += 4)
        {
            Instantiate(wall, new Vector3(buildingSize.x / 2 , 0, y), Quaternion.Euler(-90, 0, 0),   wallsTransform);
            Instantiate(wall, new Vector3(-buildingSize.x / 2, 0, y), Quaternion.Euler(-90, 180, 0), wallsTransform);
        }

        Instantiate(cornerWall, new Vector3( buildingSize.x / 2, 0,  buildingSize.y / 2),
            Quaternion.Euler(-90, 0, 0),   wallsTransform);
        Instantiate(cornerWall, new Vector3(-buildingSize.x / 2, 0,  buildingSize.y / 2),
            Quaternion.Euler(-90, -90, 0), wallsTransform);
        Instantiate(cornerWall, new Vector3( buildingSize.x / 2, 0, -buildingSize.y / 2),
            Quaternion.Euler(-90, 90, 0),  wallsTransform);
        Instantiate(cornerWall, new Vector3(-buildingSize.x / 2, 0, -buildingSize.y / 2),
            Quaternion.Euler(-90, 180, 0), wallsTransform);

        #endregion

        // Generates the roof using 4m x 4m roof tiles.
        // Their size is defined by the same reason as the walls.
        #region Roof

        for (var x = buildingSize.x / -2; x <= buildingSize.x / 2; x += 4.0f)
        {
            for (var y = buildingSize.y / -2; y <= buildingSize.y / 2; y += 4.0f)
            {
                var edge = Math.Abs(Mathf.Abs(x) - buildingSize.x / 2) < 0.01f || Math.Abs(Mathf.Abs(y) - buildingSize.y / 2) < 0.01f;
                Instantiate(edge ? roof : windowRoof, new Vector3(x, 9, y), Quaternion.Euler(-90, 0, 0), roofsTransform);
            }
        }

        #endregion

        // Generates the shelves using the prefab provided, which should be able to hold 4 rows and 3 columns.
        #region Shelves

        var isOddX = shelfColumns % 2 == 1;
        for (var y = shelfRows / -2.0f; y < shelfRows / 2.0f; y++)
        {
            var realY = (y + distanceBetweenShelves * y) + (1.0f + distanceBetweenShelves) * 0.5f;
            for (var x = 0.5f; x < shelfColumns; x++)
            {
                var realX = isOddX ? ((shelfColumns - 1) / 2f - x + 0.5f) * 4.0f : (shelfColumns / 2f - x) * 4.0f;
                Instantiate(shelf, new Vector3(realX, 0.0f, realY), Quaternion.Euler(0, -90, 0), shelvesTransform);
                
                //Instantiate(boxes[0], new Vector3(realX + 1.25f, 0.0f, realY), Quaternion.Euler(0, -90, 0), rootTransform);
                //Instantiate(boxes[0], new Vector3(realX + 1.25f, 1.5f, realY), Quaternion.Euler(0, -90, 0), rootTransform);
                //Instantiate(boxes[0], new Vector3(realX + 1.25f, 3.0f, realY), Quaternion.Euler(0, -90, 0), rootTransform);
                //Instantiate(boxes[0], new Vector3(realX + 1.25f, 4.5f, realY), Quaternion.Euler(0, -90, 0), rootTransform);
                //Instantiate(boxes[0], new Vector3(realX - 1.25f, 0.0f, realY), Quaternion.Euler(0, -90, 0), rootTransform);
                //Instantiate(boxes[0], new Vector3(realX - 1.25f, 1.5f, realY), Quaternion.Euler(0, -90, 0), rootTransform);
                //Instantiate(boxes[0], new Vector3(realX - 1.25f, 3.0f, realY), Quaternion.Euler(0, -90, 0), rootTransform);
                //Instantiate(boxes[0], new Vector3(realX - 1.25f, 4.5f, realY), Quaternion.Euler(0, -90, 0), rootTransform);
                //Instantiate(boxes[0], new Vector3(realX, 0.0f, realY), Quaternion.Euler(0, -90, 0), rootTransform);
                //Instantiate(boxes[0], new Vector3(realX, 1.5f, realY), Quaternion.Euler(0, -90, 0), rootTransform);
                //Instantiate(boxes[0], new Vector3(realX, 3.0f, realY), Quaternion.Euler(0, -90, 0), rootTransform);
                //Instantiate(boxes[0], new Vector3(realX, 4.5f, realY), Quaternion.Euler(0, -90, 0), rootTransform);
            }
        }

        #endregion
    }
}
