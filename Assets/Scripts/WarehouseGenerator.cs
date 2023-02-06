using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Boxes;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Basic enum handling the different game modes. Mostly used to change the generation properties.
/// </summary>
public enum JobType
{
    BoxSorting = 0,
    BoxFetching = 1
}

/// <summary>
/// The main MonoBehaviour handling the generation of the warehouse.
/// </summary>
public class WarehouseGenerator : MonoBehaviour
{
    private static WarehouseGenerator _instance;
    
    [Header("References")]
    public GameObject wall;
    public GameObject cornerWall;
    public GameObject roof;
    public GameObject windowRoof;
    public GameObject shelf;
    public GameObject[] boxes;
    public Material[] floorMats;
    public GameObject quad;
    [Header("Properties")] 
    public static int Seed = -1;
    public static JobType JobType = JobType.BoxFetching;
    [Range(20, 100)]
    public static int SearchPercent = 50;
    public static Vector3 TimeLimit = new(0.0f, 15.0f, 0.0f);
    public static Vector2 BuildingSize = new(24, 44);
    
    public static int ShelfColumns = 2;
    public static int ShelfRows = 4;
    
    /// <summary>
    /// Automatically calculated from the room y size and the amount of shelf rows.
    /// </summary>
    public static float DistanceBetweenShelves = 8.0f;
    
    /// <summary>
    /// Automatically calculated from the room x size and the amount of shelf columns.
    /// </summary>
    public static float DistanceToWall = 8.0f;
    
    /// <summary>
    /// Stores whether to use a type of box, or not.
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static bool[] BoxTypes = new bool[Consts.TypeCount]
    {
        true,
        true,
        false,
        false
    };
    
    /// <summary>
    /// Stores the weight at which to generate said boxes.
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static float[] BoxWeights = new float[Consts.TypeCount] {
        0.5f,
        0.5f,
        0.0f,
        0.0f
    };

    /// <summary>
    /// Stores the different zone's sizes.
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static int[] ZoneSizes = new int[Consts.TypeCount + 1] {
        2,
        1,
        1,
        0,
        0
    };

    private readonly Vector3[] _boxPositions =
    {
        new(+1.25f, 0.0f, 0.0f),
        new(+1.25f, 1.5f, 0.0f),
        new(+1.25f, 3.0f, 0.0f),
        new(+1.25f, 4.5f, 0.0f),
        new(-1.25f, 0.0f, 0.0f),
        new(-1.25f, 1.5f, 0.0f),
        new(-1.25f, 3.0f, 0.0f),
        new(-1.25f, 4.5f, 0.0f),
        new(0.0f, 0.0f, 0.0f),
        new(0.0f, 1.5f, 0.0f),
        new(0.0f, 3.0f, 0.0f),
        new(0.0f, 4.5f, 0.0f)
    };

    private static readonly List<GameObject>[] PlacedBoxes = new List<GameObject>[4];
    
    public static readonly List<GameObject>[] JobBoxes = new List<GameObject>[4];
    
    public static readonly List<GameObject>[] CorrectlyPlacedBoxes = new List<GameObject>[4];
    
    public static int MisplacedBoxes;
    [HideInInspector]
    public bool generating;
    
    private void Awake()
    {
        StartCoroutine(GenerateLater());
        
        if (_instance == null)
            _instance = this;
        else
        {
            Debug.LogWarning("Multiple Warehouse Generators detected!");
            DestroyImmediate(gameObject);
        }

        for (var i = 0; i < CorrectlyPlacedBoxes.Length; i++)
        {
            CorrectlyPlacedBoxes[i] = new List<GameObject>();
        }
    }

    public static WarehouseGenerator GetInstance()
    {
        return _instance;
    }
    
    private IEnumerator GenerateLater()
    {
        yield return new WaitForSeconds(1.0f);
        GenerateRoom();
    }

    /// <summary>
    /// The main function used in generating the warehouse.
    /// Automatically called when running the simulation,
    /// but can be explicitly called from within the editor. 
    /// </summary>
    public void GenerateRoom()
    {
        if (Seed != -1)
            Random.InitState(Seed);

        // Prepares some basic gameObjects to use for sorting the elements making up the warehouse.
        #region Preparations

        generating = true;
        
        for (var i = 0; i < 4; i++)
        {
            if (PlacedBoxes[i] != null)
                PlacedBoxes[i].Clear();
            else
                PlacedBoxes[i] = new List<GameObject>();
        }
        for (var i = 0; i < 4; i++)
        {
            if (JobBoxes[i] != null)
                JobBoxes[i].Clear();
            else
                JobBoxes[i] = new List<GameObject>();
        }

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

        for (var x = BuildingSize.x / -2 + 4; x <= BuildingSize.x / 2 - 4; x += 4)
        {
            Instantiate(wall, new Vector3(x, 0, BuildingSize.y / 2 ), Quaternion.Euler(-90, -90, 0), wallsTransform);
            Instantiate(wall, new Vector3(x, 0, -BuildingSize.y / 2), Quaternion.Euler(-90, 90, 0),  wallsTransform);
        }
        
        for (var y = BuildingSize.y / -2 + 4; y <= BuildingSize.y / 2 - 4; y += 4)
        {
            Instantiate(wall, new Vector3(BuildingSize.x / 2 , 0, y), Quaternion.Euler(-90, 0, 0),   wallsTransform);
            Instantiate(wall, new Vector3(-BuildingSize.x / 2, 0, y), Quaternion.Euler(-90, 180, 0), wallsTransform);
        }

        Instantiate(cornerWall, new Vector3( BuildingSize.x / 2, 0,  BuildingSize.y / 2),
            Quaternion.Euler(-90, 0, 0),   wallsTransform);
        Instantiate(cornerWall, new Vector3(-BuildingSize.x / 2, 0,  BuildingSize.y / 2),
            Quaternion.Euler(-90, -90, 0), wallsTransform);
        Instantiate(cornerWall, new Vector3( BuildingSize.x / 2, 0, -BuildingSize.y / 2),
            Quaternion.Euler(-90, 90, 0),  wallsTransform);
        Instantiate(cornerWall, new Vector3(-BuildingSize.x / 2, 0, -BuildingSize.y / 2),
            Quaternion.Euler(-90, 180, 0), wallsTransform);

        #endregion

        // Generates the roof using 4m x 4m roof tiles.
        // Their size is defined by the same reason as the walls.
        #region Roof

        for (var x = BuildingSize.x / -2; x <= BuildingSize.x / 2; x += 4.0f)
        {
            for (var y = BuildingSize.y / -2; y <= BuildingSize.y / 2; y += 4.0f)
            {
                var edge = Math.Abs(Mathf.Abs(x) - BuildingSize.x / 2) < 0.01f || Math.Abs(Mathf.Abs(y) - BuildingSize.y / 2) < 0.01f;
                Instantiate(edge ? roof : windowRoof, new Vector3(x, 9, y), Quaternion.Euler(-90, 0, 0), roofsTransform);
            }
        }

        #endregion

        // Generates the shelves using the prefab provided, which should be able to hold 4 rows and 3 columns.
        #region Shelves
        
        var colors = new int[ShelfRows];
        var index = 0;
        var color = 0;
        try
        {
            foreach (var zoneSize in ZoneSizes)
            {
                var startIndex = index;
                for (var i = index; i < startIndex + zoneSize; i++)
                {
                    colors[i] = color;
                    index++;
                }

                color++;
            }
        }
        catch (IndexOutOfRangeException)
        {
            Debug.LogWarning("Incorrectly setup zone sizes!");
        }

        var isOddX = ShelfColumns % 2 == 1;
        var indY = 0;
        for (var y = ShelfRows / -2.0f; y < ShelfRows / 2.0f; y++)
        {
            var realY = (y + DistanceBetweenShelves * y) + (1.0f + DistanceBetweenShelves) * 0.5f;
            var indX = 0;
            for (var x = 0.5f; x < ShelfColumns; x++)
            {
                var end = (indX == 0) ? -1 : (indX == ShelfColumns - 1) ? 1 : 0;
                var realX = isOddX ? ((ShelfColumns - 1) / 2f - x + 0.5f) * 4.0f : (ShelfColumns / 2f - x) * 4.0f;
                var shelfInstance = Instantiate(shelf, new Vector3(realX, 0.0f, realY), Quaternion.Euler(0, -90, 0), shelvesTransform);
                var shelfZone = shelfInstance.GetComponent<ShelfZone>();
                shelfZone.zoneType = colors[indY] switch
                {
                    0 => Zones.Mixed,
                    1 => Zones.Blue,
                    2 => Zones.Green,
                    3 => Zones.Yellow,
                    4 => Zones.Red,
                    _ => shelfZone.zoneType
                };

                if (end != 0)
                {
                    var q = Instantiate(quad, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.Euler(90, 0, 0), shelfInstance.transform);
                    q.transform.localPosition = new Vector3(0.0f, 0.01f, 3.0f * end);
                    q.transform.localScale = new Vector3(0.5f, 1.0f + DistanceBetweenShelves, 1.0f);
                    q.GetComponent<MeshRenderer>().material = floorMats[colors[indY]];
                }
                var q2 = Instantiate(quad, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.Euler(90, 0, 0), shelfInstance.transform);
                q2.transform.localPosition = new Vector3(2.0f, 0.01f, 0.0f);
                q2.transform.localScale = new Vector3(6.0f, 0.5f, 1.0f);
                q2.GetComponent<MeshRenderer>().material = floorMats[colors[indY]];
                var q3 = Instantiate(quad, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.Euler(90, 0, 0), shelfInstance.transform);
                q3.transform.localPosition = new Vector3(-2.0f, 0.01f, 0.0f);
                q3.transform.localScale = new Vector3(6.0f, 0.5f, 1.0f);
                q3.GetComponent<MeshRenderer>().material = floorMats[colors[indY]];
                
                indX++;

                if (JobType == JobType.BoxFetching)
                {
                    if (colors[indY] == 0)
                        continue;
                    
                    foreach (var boxPosition in _boxPositions)
                    {
                        if (!(BoxWeights[colors[indY] - 1] > Random.value)) continue;
                        
                        var box = Instantiate(boxes[colors[indY] - 1],
                            new Vector3(realX - boxPosition.x, boxPosition.y, realY),
                            Quaternion.Euler(0, -90, 0), shelvesTransform);
                        PlacedBoxes[colors[indY] - 1].Add(box);
                    }
                }
                else
                {
                    if (colors[indY] != 0)
                        continue;
                    
                    foreach (var boxPosition in _boxPositions)
                    {
                        var rand = Mathf.RoundToInt(Random.value * 3.0f);
                        if (!(BoxWeights[rand] > Random.value)) continue;
                        
                        var box = Instantiate(boxes[rand], new Vector3(realX - boxPosition.x, boxPosition.y, realY),
                            Quaternion.Euler(0, -90, 0), shelvesTransform);
                        PlacedBoxes[rand].Add(box);
                    }
                }
            }

            indY++;
        }

        #endregion

        #region Job

        if (JobType == JobType.BoxFetching)
        {
            for (var i = 0; i < PlacedBoxes.Length; i++)
            {
                var list = PlacedBoxes[i];
                foreach (var box in list.Where(_ => Mathf.FloorToInt(Random.value * 100) < SearchPercent))
                {
                    JobBoxes[i].Add(box);
                }
            }
        }
        else
        {
            for (var i = 0; i < PlacedBoxes.Length; i++)
            {
                JobBoxes[i] = PlacedBoxes[i];
            }
        }

        foreach (var list in CorrectlyPlacedBoxes)
        {
            list.Clear();
        }
        
        #endregion

        generating = false;
    }

    private float _lastAdd;
    
    public void MisplacedBox()
    {
        if ((Time.time - _lastAdd < 1.0f)) return;
        
        MisplacedBoxes++;
        _lastAdd = Time.time;
    }
    
    public void Remove()
    {
        _instance = null;
        Destroy(gameObject);
    }
}
