using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {

	public Material textureAtlas;
    public GameObject player;
	public static int columnHeight = 32;
	public static int chunkSize = 8;
    //public static int worldSize = 4;
    public static int renderRadius = 3;
	public static Dictionary<string, Chunk> chunks;

	public static string BuildChunkName(Vector3 v)
	{
		return (int)v.x + "_" + 
			         (int)v.y + "_" + 
			         (int)v.z;
	}

	IEnumerator BuildWorld()
	{
        int playerX = (int)Mathf.Floor(player.transform.position.x / chunkSize);
        int playerZ = (int)Mathf.Floor(player.transform.position.z / chunkSize);

        for (int z = -renderRadius; z < renderRadius; z++)
            for (int x = -renderRadius; x < renderRadius; x++)
                for (int y = 0; y < columnHeight; y++)
		        {
                Vector3 chunkPosition = new Vector3((x+playerX)*chunkSize,y*chunkSize,(z+playerZ)*chunkSize);
                    //If chunk has not been built yet
                    if(!chunks.ContainsKey(BuildChunkName(chunkPosition))){
                        Chunk c = new Chunk(chunkPosition, textureAtlas);
                        c.chunk.transform.parent = this.transform;
                        chunks.Add(c.chunk.name, c);
                    }
		        }

		foreach(KeyValuePair<string, Chunk> c in chunks)
		{
			c.Value.DrawChunk();
            yield return null;
		}
        player.SetActive(true);
	}

	// Use this for initialization
	void Start () {
        player.SetActive(false);
		chunks = new Dictionary<string, Chunk>();
		this.transform.position = Vector3.zero;
		this.transform.rotation = Quaternion.identity;
		StartCoroutine(BuildWorld());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
