﻿using System.Collections;
using System.Collections.Generic;
using MotionGenerator;
using MotionGenerator.Entity.Soul;
using RLCreature.BodyGenerator;
using RLCreature.BodyGenerator.Manipulatables;
using RLCreature.Sample.Common;
using RLCreature.Sample.Common.UI;
using RLCreature.Sample.Common.UI.Actions;
using RLCreature.Sample.SimpleHunting;
using UnityEngine;

namespace RLCreature.Sample.DesignedCreatures
{
    public class DesignedCreaturesEntryPoint : MonoBehaviour
    {
        private Rect _size;
        public int FoodCount = 800;
        public int CreatureCountPerPrefab = 10;
        public GameObject Plane;
        public List<GameObject> CreaturePrefabs;
        private CastUIPresenter GameUI;

        private void Start()
        {
            GameUI = CastUIPresenter.CreateComponent(Camera.main, gameObject);
            CastCameraController.CreateComponent(Camera.main, GameUI.SelectedCreature,
                GameUI.FallbackedEventsObservable);
            GameUI.LeftToolBar.Add(new SystemActions());

            Plane.transform.position = Vector3.zero;
            Plane.transform.localScale = Vector3.one * 100;
            var unitPlaneSize = 10;
            _size = new Rect(
                (Plane.transform.position.x - Plane.transform.lossyScale.x * unitPlaneSize) / 2,
                (Plane.transform.position.y - Plane.transform.lossyScale.y * unitPlaneSize) / 2,
                Plane.transform.lossyScale.x * unitPlaneSize,
                Plane.transform.lossyScale.y * unitPlaneSize
            );
            StartCoroutine(Feeder());

            foreach (var creaturePrefab in CreaturePrefabs)
            {
                for (var i = 0; i < CreatureCountPerPrefab; i++)
                {
                    var pos = new Vector3(
                        x: _size.xMin + Random.value * _size.width,
                        y: 0,
                        z: _size.yMin + Random.value * _size.height);
                    var creatureRootGameObject = Instantiate(creaturePrefab, pos, Quaternion.identity);
                    var creature = StartCreature(creatureRootGameObject,
                        creatureRootGameObject.transform.GetChild(0).gameObject);
                    creature.name = creaturePrefab.name.Substring(0, Mathf.Min(15, creaturePrefab.name.Length));
                }
            }
        }


        private Agent StartCreature(GameObject creatureRootGameObject, GameObject centralBody)
        {
            // Add Sensor and Mouth for food
            Sensor.CreateComponent(centralBody, typeof(Food), State.BasicKeys.RelativeFoodPosition, range: 100f);
            var mouth = Mouth.CreateComponent(centralBody, typeof(Food));

            // Initialize Brain
            var actions = LocomotionAction.EightDirections();
            var sequenceMaker = new EvolutionarySequenceMaker(epsilon: 0.1f, minimumCandidates: 30);
            
            var decisionMaker = new FollowPointDecisionMaker(State.BasicKeys.RelativeFoodPosition);
            var souls = new List<ISoul>() {new GluttonySoul()};

            var brain = new Brain(decisionMaker, sequenceMaker);
            var agent = Agent.CreateComponent(creatureRootGameObject, brain, new Body(centralBody), actions, souls);

            var info = GameUI.AddAgent(agent);

            StartCoroutine(EntryPointUtility.Rename(info, agent, mouth));
            return agent;
        }


        private IEnumerator Feeder()
        {
            while (true)
            {
                var foodCount = FindObjectsOfType<Food>().Length;
                for (int i = 0; i < FoodCount - foodCount; i++)
                {
                    Feed();
                }

                yield return new WaitForSeconds(5);
            }
        }

        private void Feed()
        {
            var foodObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foodObject.transform.localScale = Vector3.one;
            var food = foodObject.AddComponent<Food>();
            food.GetComponent<Renderer>().material = Resources.Load("Materials/Food", typeof(Material)) as Material;
            food.GetComponent<Collider>().isTrigger = true;
            food.transform.position = new Vector3(
                x: _size.xMin + Random.value * _size.width,
                y: 0,
                z: _size.yMin + Random.value * _size.height
            );
        }
    }
}