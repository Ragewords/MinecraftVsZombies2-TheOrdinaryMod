using System;
using MVZ2.UI;
using Tools;
using UnityEngine;

namespace MVZ2.Level.UI
{
    public class MovingBlueprint : MonoBehaviour
    {
        private void OnDisable()
        {
            Finish();
        }
        public void SetMotion(Vector3 startPosition, Transform targetTransform)
        {
            transition.setStartPosition(startPosition);
            transition.setTargetTransform(targetTransform);
            transition.enabled = true;
            moving = true;
        }
        public void SetMotion(Vector3 startPosition, Vector3 targetPosition)
        {
            transition.setStartPosition(startPosition);
            transition.setTargetPosition(targetPosition);
            transition.enabled = true;
            moving = true;
        }
        public void SetBlueprint(Blueprint blueprint)
        {
            this.blueprint = blueprint;
            blueprint.transform.SetParent(transform, false);
            blueprint.transform.localPosition = Vector3.zero;
        }
        public void Finish()
        {
            if (moving)
            {
                moving = false;
                transition.time = 1;
                OnMotionFinished?.Invoke(this);
            }
        }
        private void Update()
        {
            if (moving)
            {
                transition.time += Time.deltaTime * moveSpeed;
                if (transition.time >= 1)
                {
                    Finish();
                }
            }
        }
        public event Action<MovingBlueprint> OnMotionFinished;
        [SerializeField]
        private bool moving;
        [SerializeField]
        private float moveSpeed;
        [SerializeField]
        private PositionTransition transition;
        [SerializeField]
        private Blueprint blueprint;
    }
}
