using UnityEngine;
using System.Collections.Generic;

public class EndlessGroundGenerator : MonoBehaviour {

    public GameObject groundTile;
    public float threshould = 1f;
    public int tilesPerGen = 3;
    public float tileWidth = 8.0f;
    public float tileHeight = 4.0f;

    private const float groundAltitude = 0f;

    private Transform playerTrans;

    private EdgeCollider2D edge;

    private List<GameObject> tiles = new List<GameObject>();
    private float leftEdge = 0f;
    private float rightEdge = 0f;

	// Use this for initialization
	void Start () {
        edge = GetComponent<EdgeCollider2D>();
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
	}
	
	// Update is called once per frame
	void Update () {
        GenerateGroundAround(playerTrans.position.x);
	}

    void GenerateGroundAround(float x) {
        if (!InTilesRange(x)) {
            Debug.Log(string.Format("Warning: generating ground around {0} which is outside current ground, this is not supported now.", x));
        }

        if (NearLeftEdge(x)) {
            CreateTilesWithRightEdgeAt(new Vector2(leftEdge, groundAltitude), tilesPerGen);
        }
        if (NearRightEdge(x)) {
            CreateTilesWithLeftEdgeAt(new Vector2(rightEdge, groundAltitude), tilesPerGen);
        }
    }

    void CreateTilesWithLeftEdgeAt(Vector2 pos, int tilesCount) {
        Vector2 center = new Vector2(pos.x + tileWidth / 2, pos.y);
        while (tilesCount > 0) {
            CreateTileAt(center);
            center.x += tileWidth;
            --tilesCount;
        }
    }

    void CreateTilesWithRightEdgeAt(Vector2 pos, int tilesCount) {
        Vector2 center = new Vector2(pos.x - tileWidth / 2, pos.y);
        while (tilesCount > 0) {
            CreateTileAt(center);
            center.x -= tileWidth;
            --tilesCount;
        }
    }

    void CreateTilesCentering(Vector2 pos) {
        if (tilesPerGen % 2 == 0) {
            int tilesOnLeft = tilesPerGen / 2;
            int tilesOnRight = tilesOnLeft;

            CreateTilesWithLeftEdgeAt(pos, tilesOnRight);
            CreateTilesWithRightEdgeAt(pos, tilesOnLeft);
        } else {
            int tilesOnLeft = tilesPerGen / 2;
            int tilesOnRight = tilesOnLeft;

            CreateTileAt(pos);
            CreateTilesWithLeftEdgeAt(new Vector2(pos.x + tileWidth / 2, pos.y), tilesOnRight);
            CreateTilesWithRightEdgeAt(new Vector2(pos.x - tileWidth / 2, pos.y), tilesOnLeft);
        }
    }

    GameObject CreateTileAt(Vector2 position) {
        Debug.Log("Creating tile at " + position);

        GameObject tile = Instantiate(groundTile,
            new Vector3(position.x, position.y, 0f),
            Quaternion.identity) as GameObject;
        tile.transform.parent = transform;

        float right = position.x + tileWidth / 2;
        float left = position.x - tileWidth / 2;
        if (right > rightEdge)
            rightEdge = right;
        if (left < leftEdge)
            leftEdge = left;
        UpdateEdgeCollider();

        tiles.Add(tile);
        return tile;
    }

    void UpdateEdgeCollider() {
        Vector2[] points = edge.points;
        points[0].x = leftEdge;
        points[1].x = rightEdge;
        edge.points = points;
    }

    bool InTilesRange(float x) {
        return x >= leftEdge && x <= rightEdge;
    }

    bool NearLeftEdge(float x) {
        return x >= leftEdge && x <= leftEdge + threshould;
    }

    bool NearRightEdge(float x) {
        return x <= rightEdge && x >= rightEdge - threshould;
    }
}
