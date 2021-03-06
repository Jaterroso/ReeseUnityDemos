﻿using Reese.Nav;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Entities;
using Unity.Collections;

namespace Reese.Demo
{
    class NavPerformanceSpawner : MonoBehaviour
    {
        [SerializeField]
        Button SpawnButton = default;

        [SerializeField]
        Button PrefabButton = default;

        [SerializeField]
        Text SpawnText = default;

        [SerializeField]
        Text PrefabText = default;

        [SerializeField]
        Slider Slider = default;

        [SerializeField]
        float3 SpawnOffset = new float3(0, 1, 0);

        int spawnCount = 1;

        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        Entity cylinderPrefab;
        Entity dinosaurPrefab;
        Entity currentPrefab;

        void Start()
        {
            if (SpawnButton == null || Slider == null) return;

            SpawnButton.onClick.AddListener(Spawn);
            PrefabButton.onClick.AddListener(TogglePrefab);
            Slider.onValueChanged.AddListener(UpdateSpawnCount);

            currentPrefab = cylinderPrefab = entityManager.CreateEntityQuery(typeof(CylinderPrefab)).GetSingleton<CylinderPrefab>().Value;
            dinosaurPrefab = entityManager.CreateEntityQuery(typeof(DinosaurPrefab)).GetSingleton<DinosaurPrefab>().Value;
        }

        void UpdateSpawnCount(float count)
        {
            spawnCount = (int)count;

            if (SpawnText == null) return;

            SpawnText.text = "Spawn " + spawnCount;

            if (spawnCount == 1) SpawnText.text += " Entity";
            else SpawnText.text += " Entities";
        }

        void TogglePrefab()
        {
            if (currentPrefab.Equals(dinosaurPrefab))
            {
                currentPrefab = cylinderPrefab;
                PrefabText.text = "Spawning Cylinders";
            }
            else
            {
                currentPrefab = dinosaurPrefab;
                PrefabText.text = "Spawning Dinosaurs";
            }
        }

        void Spawn()
        {
            var outputEntities = new NativeArray<Entity>(spawnCount, Allocator.Temp);

            entityManager.Instantiate(currentPrefab, outputEntities);

            for (var i = 0; i < outputEntities.Length; ++i)
            {
                entityManager.AddComponentData(outputEntities[i], new NavAgent
                {
                    TranslationSpeed = 20,
                    RotationSpeed = 0.3f,
                    TypeID = NavUtil.GetAgentType(NavConstants.HUMANOID),
                    Offset = new float3(0, 1, 0)
                });

                entityManager.AddComponentData<LocalToWorld>(outputEntities[i], new LocalToWorld
                {
                    Value = float4x4.TRS(
                        new float3(0, 1, 0),
                        quaternion.identity,
                        1
                    )
                });

                entityManager.AddComponent<Parent>(outputEntities[i]);
                entityManager.AddComponent<LocalToParent>(outputEntities[i]);
                entityManager.AddComponent<NavNeedsSurface>(outputEntities[i]);
            }

            outputEntities.Dispose();
        }
    }
}
