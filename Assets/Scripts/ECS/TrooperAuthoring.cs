using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public class TrooperAuthoring : MonoBehaviour
{
    public float MoveSpeed;
    public float TrooperDamage;
    public class BakeMe : Baker<TrooperAuthoring>
    {
        public override void Bake(TrooperAuthoring authoring)
        {
            Entity entity= GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Trooper
            {
                MoveSpeed=authoring.MoveSpeed,
                TrooperDamage=authoring.TrooperDamage,
            });
        }
    }


}
public struct Trooper : IComponentData
{
    public float MoveSpeed;
    public float TrooperDamage;
}

partial struct TroopmoverSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (BlockPuzzleManager.Instance.isGamePaused) return;
        EntityCommandBuffer commandBuffer= SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        foreach(var (localTransform,trooper,PVelocity, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<Trooper>,RefRW<PhysicsVelocity>>().WithEntityAccess())
        {
            float3 tragetpos= BlockPuzzleManager.Instance.Tower.gameObject.transform.position;
            float3 dir=math.normalize(tragetpos-localTransform.ValueRO.Position);
            Debug.Log("System call");
            PVelocity.ValueRW.Linear=dir*trooper.ValueRO.MoveSpeed;
            PVelocity.ValueRW.Angular=float3.zero;
            float destoryDisttance = 0.5f;
            if (math.distancesq(tragetpos, localTransform.ValueRO.Position) < destoryDisttance)
            {
                BlockPuzzleManager.Instance.ReduceTowerValue(trooper.ValueRO.TrooperDamage);
                commandBuffer.DestroyEntity(entity);
            }
        }
    }
}
