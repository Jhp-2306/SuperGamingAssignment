using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject TrooperPrefab;
    public int SpawnRate=0;
    public float IntervalSeconds = 1f;


   
    public class BakeMe : Baker<SpawnerAuthoring>
    {
        
        public override void Bake(SpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Spawner
            {
                trooper = GetEntity(authoring.TrooperPrefab),
                SpawnRate = authoring.SpawnRate,
                IntervalSeconds = authoring.IntervalSeconds,
            });
        }
    }
}
public struct Spawner : IComponentData
{
    public float Timer, IntervalSeconds;
    public Entity trooper;
    public int SpawnRate;
}

partial struct SpawerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (BlockPuzzleManager.Instance.isGamePaused) return;
        foreach (var(localTranfrom,spawner) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<Spawner>>())
        {
            spawner.ValueRW.Timer-=SystemAPI.Time.DeltaTime;
            if (spawner.ValueRO.Timer > 0)
            {
                continue;
            }
            spawner.ValueRW.Timer=spawner.ValueRO.IntervalSeconds;
            if(spawner.ValueRO.SpawnRate>0)
            {
                for (int i = 0; i < spawner.ValueRO.SpawnRate; i++)
                {
                    Entity trooper = state.EntityManager.Instantiate(spawner.ValueRO.trooper);
                    SystemAPI.SetComponent(trooper, LocalTransform.FromPosition(localTranfrom.ValueRO.Position));
                }
            }
            else
            {
                Debug.Log("Spawnrate is Low");
            }
        }
    }
}
