using System;
using System.Collections.Generic;
using Block2nd.Client;
using Block2nd.GamePlay;
using UnityEngine;

namespace Block2nd.World
{
    public class ChunkRenderEntityManager : MonoBehaviour
    {
        public GameObject chunkRenderEntityPrefab;

        private GameClient gameClient;
        private Player player;
        private List<ChunkRenderEntity> renderEntityPool = new List<ChunkRenderEntity>();
        private Dictionary<ulong, ChunkRenderEntity> entityInUseDict = new Dictionary<ulong, ChunkRenderEntity>();

        private void Awake()
        {
            gameClient = GameObject.FindGameObjectWithTag("GameClient").GetComponent<GameClient>();
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        public void TryRenderChunk(Chunk chunk)
        {
            if (chunk == null)
            {
                return;
            }
            
            RenderChunk(chunk);
        }

        public void RenderChunk(Chunk chunk)
        {
            if (!IsInRenderDistance(chunk.worldBasePosition.ToUnityVector3()))
                return;
            
            if (entityInUseDict.TryGetValue(chunk.CoordKey, out var entity))
            {
                entity.freeCount = 0;
                entity.RenderChunk(chunk);
                return;
            }
            
            entity = ProvideFreeRenderEntity(chunk.CoordKey);
            entity.freeCount = 0;
            entity.transform.position = chunk.worldBasePosition.ToUnityVector3();
            entity.RenderChunk(chunk);
            entityInUseDict.Add(chunk.CoordKey, entity);
        }

        public void Tick()
        {
            foreach (var entity in renderEntityPool)
            {
                if (CanThisEntityBeFree(entity))
                    entity.freeCount++;
                else
                    entity.freeCount = 0;
            }
            
            gameClient.guiCanvasManager.chunkStatText.SetChunksInRender(entityInUseDict.Count);
        }

        private bool IsInRenderDistance(Vector3 pos)
        {
            var playerPos = player.transform.position;
            /* a circle which r = view distance * 16 * sqrt(2) */
            return (playerPos - pos).magnitude < gameClient.gameSettings.viewDistance * 22.6262624;
        }

        private bool CanThisEntityBeFree(ChunkRenderEntity entity)
        {
            var entityPos = entity.transform.position + new Vector3(8, 0, 8);
            
            if (IsInRenderDistance(entityPos))
            {
                return false;
            }

            entity.SetVisible(false);
            return true;
        }

        private ChunkRenderEntity AllocNewRenderEntity()
        {
            var go = Instantiate(chunkRenderEntityPrefab, transform);
            return go.GetComponent<ChunkRenderEntity>();
        }

        private ChunkRenderEntity ProvideFreeRenderEntity(ulong coordKey)
        {
            foreach (var entity in renderEntityPool)
            {
                if (CanThisEntityBeFree(entity))
                    entity.freeCount++;
                else
                    entity.freeCount = 0;
                
                if (entity.freeCount > 10)
                {
                    entityInUseDict.Remove(entity.currentCoordKey);
                    return entity;
                }
            }

            var newEntity = AllocNewRenderEntity();
            renderEntityPool.Add(newEntity);

            return newEntity;
        }
    }
}