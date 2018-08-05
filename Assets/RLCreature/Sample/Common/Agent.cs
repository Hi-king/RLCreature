﻿using System.Collections.Generic;
using System.Linq;
using MotionGenerator;
using MotionGenerator.Entity.Soul;
using RLCreature.BodyGenerator.Manipulatables;
using UnityEngine;

namespace RLCreature.BodyGenerator
{
    public class Agent : MonoBehaviour
    {
        Body body;
        IBrain brain;


        private List<IManipulatable> GetChildrenManipulatables()
        {
            return transform.gameObject.GetComponentsInChildren<IManipulatable>().ToList();
        }

        public static Agent CreateComponent(GameObject obj, IBrain brain, Body body, List<IAction> actions)
        {
            return obj.AddComponent<Agent>()._CreateComponent(brain, body, actions);
        }

        private Agent _CreateComponent(IBrain brain, Body body, List<IAction> actions)
        {
            List<IManipulatable> manipulatables = GetChildrenManipulatables();
            for (int i = 0; i < manipulatables.Count; i++)
            {
                manipulatables[i].SetManipulatableId(i); // just an ID
            }

            body.Init(manipulatables, actions);
            brain.Init(
                manipulatables
                    .Where(m => m.GetManipulatableDimention() > 0)
                    .Select(m => m.GetManipulatableDimention()).ToList(),
                actions,
                new List<ISoul>() {new GluttonySoul()});
            this.brain = brain;
            this.body = body;
            return this;
        }

        void GoNextAction()
        {
            State state = body.GetState();
            var ms = brain.GenerateMotionSequence(state);
            body.Manipulate(ms);
        }

        void FixedUpdate()
        {
            if (transform.position.y < 0) // out from ground
            {
                Destroy(this.gameObject);
            }

            UpdateAction();
        }


        void UpdateAction()
        {
            body.Update();
            if (body != null && !body.IsMoving())
            {
                GoNextAction();
            }
        }
    }
}