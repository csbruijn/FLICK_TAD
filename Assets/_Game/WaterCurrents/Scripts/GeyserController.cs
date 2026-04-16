using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GeyserController : MonoBehaviour
{
    public GameObject headPrefab;
    public GameObject bodyPrefab;

    public float tileHeight = 1f;
    public float growSpeed = 5f;
    public float maxHeight = 10f;

    private float currentHeight = 0f;

    private List<GameObject> bodyTiles = new();
    private GameObject head;


    [SerializeField] private BoxCollider2D triggerZone;

    bool isGrowing = false; 

    void Start()
    {
    }

    public void ActivateGeyser()
    {        isGrowing = true;
        if (head != null) Destroy(head);
        head = Instantiate(headPrefab, transform);

    }

    public void DeactivateGeyser() => isGrowing = false;

    private void Update()
    {
        if (isGrowing)
            Grow();

        if (!isGrowing && currentHeight > 0)
        {
            Debug.Log("Dissipate!");
            Dissipate();       
        }
    }


    void Grow()
    {
        if (currentHeight >= maxHeight) return;

        currentHeight += growSpeed * Time.deltaTime;

        int neededTiles = Mathf.FloorToInt(currentHeight / tileHeight);

        // spawn missing tiles
        while (bodyTiles.Count < neededTiles)
        {
            SpawnTile();
        }

        UpdatePositions();
        UpdateCollider();
    }

    void Dissipate()
    {
        currentHeight -= growSpeed * Time.deltaTime * 3;

        int neededTiles = Mathf.FloorToInt(currentHeight / tileHeight);

        if (currentHeight <= 0)
        {
            if (bodyTiles.Count> 0) {
                Destroy(bodyTiles[0]);
                bodyTiles.RemoveAt(0);
            }
            bodyTiles.Clear();
            Destroy(head);
            head = null;
            currentHeight = 0;

            UpdatePositions();
            UpdateCollider();
            return; 
        }

        if (bodyTiles.Count > neededTiles && bodyTiles.Count > 0 )
        {
            Destroy(bodyTiles[0]);
            bodyTiles.RemoveAt(0);
        }


        UpdatePositions();
        UpdateCollider();
    }


    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.ApplyAirBoost(maxHeight);
        }
    }

    void SpawnTile()
    {
        GameObject tile = Instantiate(bodyPrefab, transform);
        bodyTiles.Add(tile);
    }

    void UpdateCollider()
    {

        float headHeight = 3; // eyeballed to get ready for demo
        float height = bodyTiles.Count * tileHeight;
        if (bodyTiles.Count > 0) height += headHeight;


        triggerZone.size = new Vector2(triggerZone.size.x, height);
        triggerZone.offset = new Vector2(0, height / 2f);
    }

    void UpdatePositions()
    {
        // position body tiles
        for (int i = 0; i < bodyTiles.Count; i++)
        {
            bodyTiles[i].transform.localPosition = new Vector3(0, i * tileHeight, 0);
        }

        // position head on top
        if (head != null)
        {
            head.transform.localPosition = new Vector3(0, bodyTiles.Count * tileHeight, 0);
        }
    }
}

