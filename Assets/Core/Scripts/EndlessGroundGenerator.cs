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

	// Use this for initialization
	void Start () {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
        GenerateGroundAround(playerTrans.position.x);
	}
	
	// Update is called once per frame
	void Update () {
	}

    void GenerateGroundAround(float x) {
        CreateTilesWithRightEdgeAt(new Vector2(x, groundAltitude), tilesPerGen);
        CreateTilesWithLeftEdgeAt(new Vector2(x, groundAltitude), tilesPerGen);
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

        return tile;
    }
}
