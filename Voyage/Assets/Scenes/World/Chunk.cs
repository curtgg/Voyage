using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine;

[Serializable]
class BlockData{
    public Block.BlockType[,,] matrix;

    public BlockData(){}

    public BlockData(Block[,,] b){
        matrix = new Block.BlockType[World.chunkSize, World.chunkSize, World.chunkSize];
        for (int z = 0; z < World.chunkSize; z++)
            for (int y = 0; y < World.chunkSize; y++)
                for (int x = 0; x < World.chunkSize; x++){
                    matrix[x, y, z] = b[x, y, z].bType;
                }
    }
}


public class Chunk {

	public Material cubeMaterial;
    public Block[,,] chunkData = new Block[World.chunkSize, World.chunkSize, World.chunkSize];
    public GameObject chunk;
    public ChunkStatus status;
    BlockData bd;
    public enum ChunkStatus { DRAW , DONE , KEEP };

	void BuildChunk()
	{
        bool chunkBuilt = false; //Chunk has already been generated and saved to disk
        chunkBuilt = Load();

		for(int z = 0; z < World.chunkSize; z++)
			for(int y = 0; y < World.chunkSize; y++)
				for(int x = 0; x < World.chunkSize; x++)
				{
					Vector3 pos = new Vector3(x,y,z);
                    int worldX = (int)(x + chunk.transform.position.x);
                    int worldY = (int)(y + chunk.transform.position.y);
                    int worldZ = (int)(z + chunk.transform.position.z);


                    if(chunkBuilt)
                    {
                        chunkData[x, y, z] = new Block(bd.matrix[x, y, z], pos, chunk.gameObject, cubeMaterial, this);
                        continue;
                    }
                    

                    if (worldY == Utils.GenerateIslandHeight(worldX,worldZ))
                    {
                        chunkData[x, y, z] = new Block(Block.BlockType.GRASS, pos,
                                                      chunk.gameObject, cubeMaterial, this);
                    }
                    else if (worldY < Utils.GenerateIslandHeight(worldX, worldZ)){
                        chunkData[x, y, z] = new Block(Block.BlockType.DIRT, pos,
                                                          chunk.gameObject, cubeMaterial, this);
                    }
                    else
                    {
                        chunkData[x, y, z] = new Block(Block.BlockType.AIR, pos,
                                                      chunk.gameObject, cubeMaterial, this);
                    }

                    status = ChunkStatus.DRAW;
				}
    }

    public void DrawChunk()
    {
        for (int z = 0; z < World.chunkSize; z++)
            for (int y = 0; y < World.chunkSize; y++)
                for (int x = 0; x < World.chunkSize; x++)
                {
                    chunkData[x, y, z].Draw();
                }

        CombineQuads();
        MeshCollider collider = chunk.gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
        collider.sharedMesh = chunk.transform.GetComponent<MeshFilter>().mesh;
        status = ChunkStatus.DONE;
    }

    public Chunk(Vector3 position, Material c)
    {
        chunk = new GameObject(World.BuildChunkName(position));
        chunk.transform.position = position;
        cubeMaterial = c;
        BuildChunk();
    }
	

	void CombineQuads()
	{
		//1. Combine all children meshes
		MeshFilter[] meshFilters = chunk.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length) {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;
        }

        //2. Create a new mesh on the parent object
        MeshFilter mf = (MeshFilter) chunk.gameObject.AddComponent(typeof(MeshFilter));
        mf.mesh = new Mesh();

        //3. Add combined meshes on children as the parent's mesh
        mf.mesh.CombineMeshes(combine);

        //4. Create a renderer for the parent
		MeshRenderer renderer = chunk.gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
		renderer.material = cubeMaterial;

		//5. Delete all uncombined children
		foreach (Transform quad in chunk.transform) {
     		GameObject.Destroy(quad.gameObject);
 		}

	}

    string BuildChunkFileName(Vector3 v)
    {
        return Application.persistentDataPath + "/savedata/Chunk_" +
                            (int)v.x + "-" +
                                (int)v.y + "-" +
                                    (int)v.z + "-" +
                                    World.chunkSize +
                          "-" + World.renderRadius + ".dat";
    }

    bool Load()
    {
        string chunkFile = BuildChunkFileName(chunk.transform.position);
        if(File.Exists(chunkFile))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(chunkFile, FileMode.Open);
            bd = new BlockData();
            bd = (BlockData) bf.Deserialize(file);
            file.Close();
            return true;
        }
        return false;
    }

    public void Save()
    {
        string chunkFile = BuildChunkFileName(chunk.transform.position);
        if (!File.Exists(chunkFile))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(chunkFile));
        }
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(chunkFile, FileMode.OpenOrCreate);
        bd = new BlockData(chunkData);
        bf.Serialize(file,bd);
        file.Close();
    }
}
