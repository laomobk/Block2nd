using System;
using System.Collections.Generic;
using Block2nd.GamePlay;
using UnityEngine;

namespace Block2nd.World
{
    public class ChunkRenderEntityManager : MonoBehaviour
    {
        public GameObject chunkRenderEntityPrefab;

        private Player player;
        private List<ChunkRenderEntity> renderEntityPool = new List<ChunkRenderEntity>();
        private Dictionary<ulong, ChunkRenderEntity> renderEntityDict = new Dictionary<ulong, ChunkRenderEntity>();

        private void Awake()
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        public void RenderChunk(Chunk chunk)
        {;
            if (renderEntityDict.TryGetValue(chunk.CoordKey, out var entity))
            {
                entity.freeCount = 0;
                entity.RenderChunk(chunk);
                return;
            }
            
            entity = ProvideFreeRenderEntity(chunk.CoordKey);
            entity.freeCount = 0;
            entity.transform.position = chunk.worldBasePosition.ToUnityVector3();
            entity.RenderChunk(chunk);
            renderEntityDict.Add(chunk.CoordKey, entity);
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
        }

        private bool CanThisEntityBeFree(ChunkRenderEntity entity)
        {
            var entityPos = entity.transform.position + new Vector3(8, 0, 8);
            var playerPos = player.transform.position;

            var dir = entityPos - playerPos;
            dir.y = 0;
            
            if (dir.magnitude < 32 || Vector3.Dot(player.playerCamera.transform.forward, dir.normalized) > 0)
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
                
                if (entity.freeCount > 2)
                {
                    renderEntityDict.Remove(entity.currentCoordKey);
                    return entity;
                }
            }

            var newEntity = AllocNewRenderEntity();
            renderEntityPool.Add(newEntity);

            return newEntity;
        }
    }
}