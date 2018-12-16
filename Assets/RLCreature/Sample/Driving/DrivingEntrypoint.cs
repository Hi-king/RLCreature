using System.Collections;
using System.Collections.Generic;
using MotionGenerator;
using MotionGenerator.Entity.Soul;
using RLCreature.BodyGenerator;
using RLCreature.BodyGenerator.Manipulatables;
using RLCreature.Sample.SimpleHunting;
using UnityEngine;

namespace RLCreature.Sample.Driving
{
    public class DrivingEntrypoint : MonoBehaviour
    {
        private Rect _size;
        public int FoodCount = 800;

        private void Start()
        {
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.position = Vector3.zero;
            plane.transform.localScale = Vector3.one * 1000;
            var unitPlaneSize = 10;
            _size = new Rect(
                (plane.transform.position.x - plane.transform.lossyScale.x * unitPlaneSize) / 2,
                (plane.transform.position.y - plane.transform.lossyScale.y * unitPlaneSize) / 2,
                plane.transform.lossyScale.x * unitPlaneSize,
                plane.transform.lossyScale.y * unitPlaneSize
            );
            var creature = SpawnCreature();

            var camera = Camera.main;
            camera.transform.parent = creature.transform;
            camera.transform.position = creature.transform.position - creature.transform.forward * 30 +
                                        creature.transform.up * 10;
            StartCoroutine(Feeder());
            StartCoroutine(SpawnSome());
        }

        private IEnumerator SpawnSome()
        {
            yield return new WaitForSeconds(60);
//            SpawnCreature();
//            yield return new WaitForSeconds(60);
//            SpawnCreature();
//            yield return new WaitForSeconds(60);
//            SpawnCreature();
//            yield return new WaitForSeconds(60);
//            SpawnCreature();
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
            foodObject.transform.localScale = Vector3.one * 5;
            var food = foodObject.AddComponent<Food>();
            food.GetComponent<Renderer>().material.color = Color.green;
            food.GetComponent<Collider>().isTrigger = true;
            food.transform.position = new Vector3(
                x: _size.xMin + Random.value * _size.width / 2 + _size.width / 4,
                y: 0,
                z: _size.yMin + Random.value * _size.height / 2 + _size.height / 4
            );
        }

        private GameObject SpawnCreature()
        {
            var prefab = (GameObject) Resources.Load("Prefabs/Car");
            var car = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            car.AddComponent<BackToCenterBehaviour>();
            CarControlManipulatable.CreateComponent(car);
            Sensor.CreateComponent(car, typeof(Food), State.BasicKeys.RelativeFoodPosition, range: 100f);
            Mouth.CreateComponent(car, typeof(Food));

            var actions = LocomotionAction.EightDirections();
            var sequenceMaker = new EvolutionarySequenceMaker(epsilon: 0.3f, minimumCandidates: 30);
//            var decisionMaker = new ReinforcementDecisionMaker(
//                keyOrder: new[]
//                {
//                    State.BasicKeys.Forward,
//                    State.BasicKeys.RelativeFoodPosition
//                }, soulWeights: new[]
//                {
//                    1f
//                });
            var decisionMaker = new FollowPointDecisionMaker(State.BasicKeys.RelativeFoodPosition);
            var brain = new Brain(
                decisionMaker,
                sequenceMaker
            );
            Agent.CreateComponent(car, brain, new Body(car), actions, souls: new List<ISoul>(){new SnufflingDifferencialSoul()});

            return car;
        }
    }
}