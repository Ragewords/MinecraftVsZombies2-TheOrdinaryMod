using System;
using System.Collections.Generic;
using System.Linq;
using PVZEngine.Entities;
using PVZEngine.Level.Collisions;
using UnityEngine;

namespace PVZEngine.Level
{
    public partial class LevelEngine
    {
        private void AddEntity(Entity entity)
        {
        }
        internal void RemoveEntity(Entity entity)
        {
            entities.Remove(entity);
            entityTrash.Add(entity);
            RemoveEntityFromQuadTree(entity);
            OnEntityRemove?.Invoke(entity);
        }
        public Entity Spawn(EntityDefinition entityDef, Vector3 pos, Entity spawner, SpawnParams param = null)
        {
            return Spawn(entityDef, pos, spawner, entityRandom.Next(), param);
        }
        public Entity Spawn(NamespaceID entityRef, Vector3 pos, Entity spawner, SpawnParams param = null)
        {
            var entityDef = Content.GetEntityDefinition(entityRef);
            if (entityDef == null)
                return null;
            return Spawn(entityDef, pos, spawner, param);
        }
        public Entity Spawn(EntityDefinition entityDef, Vector3 pos, Entity spawner, int seed, SpawnParams param = null)
        {
            long id = AllocEntityID();
            var spawned = new Entity(this, id, new EntityReferenceChain(spawner), entityDef, seed);
            spawned.Position = pos;
            if (param != null)
            {
                param.Apply(spawned);
            }
            entities.Add(spawned);
            OnEntitySpawn?.Invoke(spawned);
            spawned.Init(spawner);
            AddEntityToQuadTree(spawned);
            return spawned;
        }
        public Entity Spawn(NamespaceID entityRef, Vector3 pos, Entity spawner, int seed, SpawnParams param = null)
        {
            var entityDef = Content.GetEntityDefinition(entityRef);
            if (entityDef == null)
                return null;
            return Spawn(entityDef, pos, spawner, seed, param);
        }
        public QuadTreeCollider GetCollisionQuadTree(int flag)
        {
            if (quadTrees.TryGetValue(flag, out var tree))
                return tree;
            return null;
        }

        #region 查询
        public Entity[] GetEntities(params int[] filterTypes)
        {
            if (filterTypes == null || filterTypes.Length <= 0)
                return entities.ToArray();
            return FindEntities(predicate);

            bool predicate(Entity e)
            {
                return filterTypes.Contains(e.Type);
            }
        }
        public Entity[] FindEntities(EntityDefinition def)
        {
            if (def == null)
                return Array.Empty<Entity>();
            return FindEntities(predicate);

            bool predicate(Entity e)
            {
                return e.Definition == def;
            }
        }
        public Entity[] FindEntities(NamespaceID id)
        {
            if (!NamespaceID.IsValid(id))
                return Array.Empty<Entity>();
            return FindEntities(predicate);

            bool predicate(Entity e)
            {
                return e.IsEntityOf(id);
            }
        }
        public Entity[] FindEntities(Func<Entity, bool> predicate)
        {
            return entities.Where(predicate).ToArray();
        }
        public int GetEntityCount(EntityDefinition def)
        {
            if (def == null)
                return 0;
            return GetEntityCount(predicate);

            bool predicate(Entity e)
            {
                return e.Definition == def;
            }
        }
        public int GetEntityCount(NamespaceID id)
        {
            if (!NamespaceID.IsValid(id))
                return 0;
            return GetEntityCount(predicate);

            bool predicate(Entity e)
            {
                return e.IsEntityOf(id);
            }
        }
        public int GetEntityCount(Func<Entity, bool> predicate)
        {
            int count = 0;
            foreach (var entity in entities)
            {
                if (predicate(entity))
                {
                    count++;
                }
            }
            return count;
        }
        public void FindEntitiesNonAlloc(Func<Entity, bool> predicate, List<Entity> results)
        {
            foreach (var entity in entities)
            {
                if (predicate(entity))
                {
                    results.Add(entity);
                }
            }
        }
        public Entity FindEntityByID(long id)
        {
            foreach (var entity in entities)
            {
                if (entity.ID == id)
                    return entity;
            }
            return FindEntityInTrash(id);
        }
        public Entity FindFirstEntity(EntityDefinition def)
        {
            if (def == null)
                return null;
            return FindFirstEntity(predicate);

            bool predicate(Entity e)
            {
                return e.Definition == def;
            }
        }
        public Entity FindFirstEntity(NamespaceID id)
        {
            if (!NamespaceID.IsValid(id))
                return null;
            return FindFirstEntity(predicate);

            bool predicate(Entity e)
            {
                return e.IsEntityOf(id);
            }
        }
        public Entity FindFirstEntity(Func<Entity, bool> predicate)
        {
            foreach (var entity in entities)
            {
                if (predicate(entity))
                    return entity;
            }
            return null;
        }
        public Entity FindFirstEntityWithTheLeast<TKey>(Func<Entity, bool> predicate, Func<Entity, TKey> keySelector)
        {
            Entity targetEntity = null;
            TKey targetKey = default;
            var comparer = Comparer<TKey>.Default;
            foreach (var entity in entities)
            {
                if (!predicate(entity))
                    continue;
                var key = keySelector(entity);
                if (targetEntity != null && comparer.Compare(targetKey, key) < 0)
                    continue;
                targetEntity = entity;
                targetKey = key;
            }
            return targetEntity;
        }
        public Entity FindFirstEntityWithTheMost<TKey>(Func<Entity, bool> predicate, Func<Entity, TKey> keySelector)
        {
            Entity targetEntity = null;
            TKey targetKey = default;
            var comparer = Comparer<TKey>.Default;
            foreach (var entity in entities)
            {
                if (!predicate(entity))
                    continue;
                var key = keySelector(entity);
                if (targetEntity != null && comparer.Compare(targetKey, key) > 0)
                    continue;
                targetEntity = entity;
                targetKey = key;
            }
            return targetEntity;
        }
        public bool EntityExists(long id)
        {
            return entities.Exists(predicate);

            bool predicate(Entity e)
            {
                return e.ID == id;
            }
        }
        public bool EntityExists(EntityDefinition def)
        {
            return entities.Exists(predicate);

            bool predicate(Entity e)
            {
                return e.Definition == def;
            }
        }
        public bool EntityExists(NamespaceID id)
        {
            return entities.Exists(predicate);

            bool predicate(Entity e)
            {
                return e.IsEntityOf(id);
            }
        }
        public bool EntityExists(Predicate<Entity> predicate)
        {
            return entities.Exists(predicate);
        }
        #endregion

        private long AllocEntityID()
        {
            long id = currentEntityID;
            currentEntityID++;
            return id;
        }
        private Entity FindEntityInTrash(long id)
        {
            foreach (var entity in entityTrash)
            {
                if (entity.ID == id)
                    return entity;
            }
            return null;
        }
        private void ClearEntityTrash()
        {
            entityTrash.Clear();
        }

        private void UpdateEntities()
        {
            entityUpdateBuffer.Clear();
            entityUpdateBuffer.AddRange(entities);
            foreach (var entity in entityUpdateBuffer)
            {
                entity.Update();
            }
        }
        #region 碰撞
        public void FindCollidersRange(int mask, Rect rect, List<EntityCollider> collider)
        {
            foreach (var pair in quadTrees)
            {
                var flag = pair.Key;
                if ((flag & mask) == 0)
                    continue;
                var quadTree = pair.Value;
                quadTree.FindTargetsInRect(rect, collider);
            }
        }
        private void AddEntityToQuadTree(Entity entity)
        {
            for (int i = 0; i < entity.GetEnabledColliderCount(); i++)
            {
                var collider = entity.GetEnabledColliderAt(i);
                var flag = entity.TypeCollisionFlag;
                InsertCollider(flag, collider);
            }
            entity.OnColliderEnabled += OnColliderEnabledCallback;
            entity.OnColliderDisabled += OnColliderDisabledCallback;
        }
        private void RemoveEntityFromQuadTree(Entity entity)
        {
            for (int i = 0; i < entity.GetEnabledColliderCount(); i++)
            {
                var collider = entity.GetEnabledColliderAt(i);
                var flag = entity.TypeCollisionFlag;
                RemoveCollider(flag, collider);
            }
            entity.OnColliderEnabled -= OnColliderEnabledCallback;
            entity.OnColliderDisabled -= OnColliderDisabledCallback;
        }
        private void InsertCollider(int flag, EntityCollider collider)
        {
            if (!quadTrees.TryGetValue(flag, out var tree))
            {
                tree = CreateQuadTree();
                quadTrees.Add(flag, tree);
            }
            tree.Insert(collider);
        }
        private void RemoveCollider(int flag, EntityCollider collider)
        {
            if (!quadTrees.TryGetValue(flag, out var tree))
            {
                return;
            }
            tree.Remove(collider);
        }
        private QuadTreeCollider CreateQuadTree()
        {
            return new QuadTreeCollider(quadTreeParams.size, quadTreeParams.maxObjects, quadTreeParams.maxDepth);
        }
        private void OnColliderEnabledCallback(EntityCollider collider)
        {
            InsertCollider(collider.Entity.TypeCollisionFlag, collider);
        }
        private void OnColliderDisabledCallback(EntityCollider collider)
        {
            RemoveCollider(collider.Entity.TypeCollisionFlag, collider);
        }
        private void CollisionUpdate()
        {
            colliderBuffer.Clear();
            foreach (var quadTree in quadTrees.Values)
            {
                quadTree.Update();
                quadTree.GetAllTargets(colliderBuffer);
            }

            foreach (var collider1 in colliderBuffer)
            {
                var ent1 = collider1.Entity;
                var detection = ent1.Cache.CollisionDetection;
                if (detection == EntityCollisionHelper.DETECTION_IGNORE)
                    continue;
                int maskHostile = ent1.CollisionMaskHostile;
                int maskFriendly = ent1.CollisionMaskFriendly;
                var maskTotal = maskHostile | maskFriendly;
                int ent1Faction = ent1.Cache.Faction;

                var ent1Motion = ent1.Position - ent1.PreviousPosition;


                var rect1 = collider1.GetCollisionRect();
                var collisionPoints = 1;
                if (detection == EntityCollisionHelper.DETECTION_CONTINUOUS)
                {
                    collisionPoints = Mathf.CeilToInt(ent1Motion.magnitude / ent1.Cache.CollisionSampleLength);
                    collisionPoints = Mathf.Max(collisionPoints, 1);
                }
                for (int p = collisionPoints - 1; p >= 0; p--) // 从上一帧的位置开始向当前位置回溯
                {
                    var rewind = p / (float)collisionPoints;
                    var ent1Offset = -ent1Motion * rewind;
                    var offsetedRect = rect1;
                    offsetedRect.x += ent1Offset.x;
                    offsetedRect.y += ent1Offset.z;

                    collisionBuffer.Clear();
                    foreach (var pair in quadTrees)
                    {
                        var flag = pair.Key;
                        if ((flag & maskTotal) == 0)
                            continue;
                        var tree = pair.Value;
                        tree.FindTargetsInRect(offsetedRect, collisionBuffer, rewind);
                    }
                    foreach (var collider2 in collisionBuffer)
                    {
                        if (collider1 == collider2)
                            continue;
                        var ent2 = collider2.Entity;
                        if (ent1 == ent2)
                            continue;
                        var detection2 = ent2.Cache.CollisionDetection;
                        if (detection2 == EntityCollisionHelper.DETECTION_IGNORE)
                            continue;
                        var ent2Faction = ent2.Cache.Faction;
                        var mask = EngineEntityExt.IsHostile(ent1Faction, ent2Faction) ? ent1.CollisionMaskHostile : ent1.CollisionMaskFriendly;
                        if (!EntityCollisionHelper.CanCollide(mask, ent2))
                            continue;
                        var ent2Motion = ent2.Position - ent2.PreviousPosition;
                        var ent2Offset = -ent2Motion * rewind;

                        collider1.DoCollision(collider2, ent1Offset - ent2Offset);
                    }
                }

                ent1.ExitCollision(this);
            }
        }
        #endregion

        #region 事件
        public Action<Entity> OnEntitySpawn;
        public Action<Entity> OnEntityRemove;
        #endregion
        private long currentEntityID = 1;
        private List<Entity> entities = new List<Entity>();
        private List<Entity> entityTrash = new List<Entity>();
        private List<Entity> entityUpdateBuffer = new List<Entity>();
        private List<EntityCollider> colliderBuffer = new List<EntityCollider>();
        private List<EntityCollider> collisionBuffer = new List<EntityCollider>();
        private Dictionary<int, QuadTreeCollider> quadTrees = new Dictionary<int, QuadTreeCollider>();
        private QuadTreeParams quadTreeParams;
    }
}