﻿using MotionGenerator;
using UnityEngine;
using UnityEngine.Assertions;

namespace RLCreature.BodyGenerator.Manipulatables
{
    public class SuperFlexibleMove : ManipulatableBase
    {
        private int _consumedFrames;
        private float _speed = 0.1f;
        private MotionSequence _sequence = new MotionSequence();
        private Rigidbody _rigidbody;

        private SuperFlexibleMove()
        {
        }

        public static SuperFlexibleMove CreateComponent(GameObject obj, float speed)
        {
            var created = obj.AddComponent<SuperFlexibleMove>();
            created._speed = speed;
            return created;
        }

        public override void Manipulate(MotionSequence sequence)
        {
            _consumedFrames = 0;
            _isMoving = true;
            _sequence = new MotionSequence(sequence);
        }

        public override void UpdateFixedFrame()
        {
            if (_sequence.Sequence.Count > 0)
            {
                if (_sequence[0].time < _consumedFrames)
                {
                    Assert.AreEqual(
                        _sequence[0].value.Count,
                        2
                    );
                    var towardVector = new Vector3(
                        _sequence[0].value[0] > 0.5f ? -1 : 1,
                        0,
                        _sequence[0].value[1] > 0.5f ? -1 : 1
                    );
                    _rigidbody.AddRelativeForce(towardVector * _speed * 1000000, ForceMode.Impulse);
//                    _rigidbody.MovePosition(gameObject.transform.position + towardVector * _speed);
                    _sequence.Sequence.RemoveAt(0);
                }
            }
            else
            {
                _isMoving = false;
            }

            _consumedFrames += 1;
        }

        public override void Init()
        {
            _rigidbody = gameObject.GetComponent<Rigidbody>();
            _rigidbody.maxDepenetrationVelocity = Mathf.Infinity;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        public override int GetManipulatableDimention()
        {
            return 2;
        }
    }
}