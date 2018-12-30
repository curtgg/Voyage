using Realtime.Messaging.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {

	public Material textureAtlas;
    public GameObject player;
	public static int columnHeight = 32;
	public static int chunkSize = 8;
    //public static int worldSize = 4;
    public static int renderRadius = 16;
	public static ConcurrentDictionary<string, Chunk> chunks;
    bool firstBuild = true; //To be used with loadign screen in future
    bool building = false;

	public static string BuildChunkName(Vector3 v)
	{
		return (int)v.x + "_" + 
			         (int)v.y + "_" + 
			         (int)v.z;
	}

    void BuildChunkAt(int x, int y, int z)
    {
        Vector3 chunkPos = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize);

        Chunk c;
        string n = BuildChunkName(chunkPos);

        if (!chunks.TryGetValue(n, out c)) //If our chunk exists
        {
            c = new Chunk(chunkPos, textureAtlas);
            c.chunk.transform.parent = this.transform;
            chunks.TryAdd(c.chunk.name, c);
        }
    }

    IEnumerator BuildWorld(int x, int y, int z, int radius)
    {
        radius--;
        if (radius <= 0) yield break;

        BuildChunkAt(x, y, z + 1);
        StartCoroutine(BuildWorld(x, y, z + 1, radius));
        yield return null;

        BuildChunkAt(x, y, z - 1);
        StartCoroutine(BuildWorld(x, y, z - 1, radius));
        yield return null;

        BuildChunkAt(x, y-1, z);
        StartCoroutine(BuildWorld(x, y-1, z, radius));
        yield return null;

        BuildChunkAt(x, y+1, z);
        StartCoroutine(BuildWorld(x, y+1, z, radius));
        yield return null;

        BuildChunkAt(x-1, y, z);
        StartCoroutine(BuildWorld(x-1, y, z, radius));
        yield return null;

        BuildChunkAt(x+1, y, z);
        StartCoroutine(BuildWorld(x+1, y, z, radius));
        yield return null;
    }

    IEnumerator DrawChunks()
    {
        foreach (KeyValuePair<string, Chunk> c in chunks)
        {
            if (c.Value.status == Chunk.ChunkStatus.DRAW)
            {
                c.Value.DrawChunk();
            }

            yield return null;
        }
    }

    /*
    IEnumerator BuildWorld()
	{
        building = true;
        int playerX = (int)Mathf.Floor(player.transform.position.x / chunkSize);
        int playerZ = (int)Mathf.Floor(player.transform.position.z / chunkSize);

        float numChunks  = (Mathf.Pow(renderRadius*2+1,2)*columnHeight)*2;
        int processCount = 0;

        for (int z = -renderRadius; z < renderRadius; z++)
            for (int x = -renderRadius; x < renderRadius; x++)
                for (int y = 0; y < columnHeight; y++)
		        {
                    Vector3 chunkPosition = new Vector3((x+playerX)*chunkSize,y*chunkSize,(z+playerZ)*chunkSize);
                    Chunk c;
                    string n = BuildChunkName(chunkPosition);
                    
                    if(chunks.TryGetValue(n, out c))
                    {
                        c.status = Chunk.ChunkStatus.KEEP;
                        break;
                    }
                    else
                    {
                        c = new Chunk(chunkPosition, textureAtlas);
                        c.chunk.transform.parent = this.transform;
                        chunks.Add(c.chunk.name, c);
                    }

                    if (firstBuild)
                    {
                        processCount++;
                    }
              
		        }

		
    }
    */
	// Use this for initialization
	void Start () {
        Vector3 ppos = player.transform.position;
        player.transform.position = new Vector3(ppos.x, Utils.GenerateIslandHeight(ppos.x, ppos.z) + 1, ppos.z);
        player.SetActive(false);
        firstBuild = true;
		chunks = new ConcurrentDictionary<string, Chunk>();
		this.transform.position = Vector3.zero;
		this.transform.rotation = Quaternion.identity;
        BuildChunkAt((int)(player.transform.position.x / chunkSize), (int)(player.transform.position.y / chunkSize), (int)(player.transform.position.z / chunkSize));
		StartCoroutine(DrawChunks());

        StartCoroutine(BuildWorld((int)(player.transform.position.x / chunkSize), (int)(player.transform.position.y / chunkSize), (int)(player.transform.position.z / chunkSize), renderRadius));
	}
	
	// Update is called once per frame
	void Update () {
        if(!player.activeSelf)
        {
            player.SetActive(true);
            firstBuild = false;
        }
        StartCoroutine(DrawChunks());
    }
}
