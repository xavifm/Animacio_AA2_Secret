using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{
    public struct PositionRotation
    {
        Vector3 position;
        Quaternion rotation;

        public PositionRotation(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        // PositionRotation to Vector3
        public static implicit operator Vector3(PositionRotation pr)
        {
            return pr.position;
        }
        // PositionRotation to Quaternion
        public static implicit operator Quaternion(PositionRotation pr)
        {
            return pr.rotation;
        }
    }
    public delegate float ErrorFunction(Vector3 target, float[] solution);
    public class MyScorpionController
    {
        //TAIL
        Transform tailTarget;
        Transform tailEndEffector;
        MyTentacleController _tail;
        float animationRange;
        public float[] Solution = null;

        //LEGS
        Transform[] legTargets;
        Transform[] legFutureBases;
        MyTentacleController[] _legs = new MyTentacleController[6];
        ErrorFunction ErrorFunction;
        float accumulatedPenalty;
        public float StopThreshold = 0.1f;
        public float DeltaGradient = 0.01f;
        public float LearningRate = 25;
        Vector3[] tailStartOffset;
        private Vector3[,] copy;
        private float[] distances;
        private bool done;
        private bool[] moveLeg;

        #region public
        public void InitLegs(Transform[] LegRoots,Transform[] LegFutureBases, Transform[] LegTargets)
        {
            legTargets = LegTargets;
            legFutureBases = LegFutureBases;
            _legs = new MyTentacleController[LegRoots.Length];
            moveLeg = new bool[_legs.Length];
            //Legs init
            for(int i = 0; i < LegRoots.Length; i++)
            {
                legFutureBases[i].transform.position -= new Vector3(0, 0, 0.5f);
                _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);
                //TODO: initialize anything needed for the FABRIK implementation
            }
        }

        public void InitTail(Transform TailBase)
        {
            distances = new float[_legs[0].Bones.Length - 1];
            copy = new Vector3[_legs.Length, _legs[0].Bones.Length];
            tailTarget = GameObject.Find("Ball").transform;
            ErrorFunction = DistanceFromTarget;
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);
            tailStartOffset = new Vector3[_tail.Bones.Length - 1];
            Solution = new float[_tail.Bones.Length - 1];

            for (int i = 0; i < tailStartOffset.Length; i++)
            {
                tailStartOffset[i] = _tail.Bones[i].localPosition;
                Solution[i] = 0;
            }

            //TODO: Initialize anything needed for the Gradient Descent implementation
        }

        //TODO: Check when to start the animation towards target and implement Gradient Descent method to move the joints.
        public void NotifyTailTarget(Transform target)
        {
            if(Vector3.Distance(_tail.Bones[3].position, tailTarget.position) <= 10)
            updateTail();
        }

        //TODO: Notifies the start of the walking animation
        public void NotifyStartWalk()
        {

        }

        //TODO: create the apropiate animations and update the IK from the legs and tail

        public void UpdateIK()
        {
            updateLegs();
        }
        #endregion


        #region private
        //TODO: Implement the leg base animations and logic
        private void updateLegPos()
        {
            //check for the distance to the futureBase, then if it's too far away start moving the leg towards the future base position
            //
        }
        //TODO: implement Gradient Descent method to move tail if necessary
        private void updateTail()
        {
            tailTarget = GameObject.Find("Ball").transform;
            if (ErrorFunction(tailTarget.transform.position, Solution) > StopThreshold)
                ApproachTarget(tailTarget.transform.position);
        }
        //TODO: implement fabrik method to move legs 
        private void updateLegs()
        {
            for (int o = 0; o < _legs.Length; o++)
            {
                // Copy the joints positions to work with
                //TODO
                for (int i = 0; i < _legs[o].Bones.Length; i++)
                {
                    copy[o,i] = _legs[o].Bones[i].transform.position;
                }

                for (int i = _legs[o].Bones.Length - 2; i >= 0; i--)
                {
                    distances[i] = Vector3.Distance(copy[o,i], copy[o,i + 1]);
                }

                //done = TODO
                if (!done)
                {
                    float targetRootDist = Vector3.Distance(copy[o,0], legTargets[o].position);

                    // Update joint positions
                    Vector3 reachableVector = legTargets[o].position;

                    while (copy[o,_legs[o].Bones.Length - 1] != reachableVector)
                    {
                        // STAGE 1: FORWARD REACHING
                        //TODO
                        copy[o,_legs[o].Bones.Length - 1] = reachableVector;

                        for (int i = _legs[o].Bones.Length - 2; i >= 0; i--)
                        {
                            Vector3 lineMovement = copy[o,i] - copy[o,i + 1];
                            copy[o,i] = copy[o,i + 1] + Vector3.Normalize(lineMovement) * distances[i];
                        }

                        // STAGE 2: BACKWARD REACHING
                        //TODO
                        copy[o,0] = _legs[o].Bones[0].transform.position;

                        for (int i = 1; i < _legs[o].Bones.Length - 1; i++)
                        {
                            Vector3 lineMovement = copy[o,i] - copy[o,i - 1];
                            copy[o,i] = copy[o,i - 1] + Vector3.Normalize(lineMovement) * distances[i];
                        }
                    }

                    if (Vector3.Distance(_legs[o].Bones[_legs[o].Bones.Length - 1].position, reachableVector) > 0.05f)
                    {
                        // Update original joint rotations
                        for (int i = 0; i <= _legs[o].Bones.Length - 2; i++)
                        {
                            Vector3 finalVector = (copy[o,i + 1] - copy[o,i]);
                            Vector3 initialVector = (_legs[o].Bones[i + 1].position - _legs[o].Bones[i].position);
                            Vector3 axis = Vector3.Cross(initialVector, finalVector).normalized;
                            float angle = Vector3.Dot(initialVector.normalized, finalVector.normalized);
                            angle = Mathf.Acos(angle);
                            angle *= Mathf.Rad2Deg;
                            //angle = Mathf.Clamp(angle, 0.9f, 1f);
                            //print(angle);
                            //TODO 
                            _legs[o].Bones[i].Rotate(axis, angle, Space.World);
                        }
                    }

                    if(o == 0 && Vector3.Distance(_legs[o].Bones[0].position, legFutureBases[o].position) > 1 && !moveLeg[o + 1])
                        moveLeg[o] = true;
                    else if (o > 0 && Vector3.Distance(_legs[o].Bones[0].position, legFutureBases[o].position) > 1 && !moveLeg[o-1])
                        moveLeg[o] = true;
                    else if (Vector3.Distance(_legs[o].Bones[0].position, legFutureBases[o].position) <= 0.05f)
                        moveLeg[o] = false;

                    if(moveLeg[o])
                    {
                        _legs[o].Bones[0].position = Vector3.Lerp(_legs[o].Bones[0].position, legFutureBases[o].transform.position, Time.deltaTime * 60);
                        copy[o, 0] = _legs[o].Bones[0].position;
                    }
                }
            }
        }
        #endregion

        private void ApproachTarget(Vector3 target)
        {
            float result = CalculateGradient(target, Solution, 0, DeltaGradient);
            Solution[0] -= LearningRate * result;

            float angle = Mathf.Clamp(Solution[0], -90, 90);
            Debug.Log(angle);
            _tail.Bones[0].localEulerAngles = new Vector3(0, angle, 0);

            //TODO
            for (int i = 1; i < _tail.Bones.Length - 1; i++)
            {
                float result2 = CalculateGradient(target, Solution, i, DeltaGradient);
                Solution[i] -= LearningRate * result2;

                //Debug.Log("pos" + i + " " + Solution[i]);
                //_tail.Bones[i].MoveArm(Solution[i]);
                //setAngle
                float angle2 = Mathf.Clamp(Solution[i], 10, 90);
                _tail.Bones[i].localEulerAngles = new Vector3(-angle2, 0, 0);
                //Debug.Log(axis);
                //_____________________
            }
        }

        private float CalculateGradient(Vector3 target, float[] Solution, int i, float delta)
        {
            //TODO

            // Saves the angle,
            // it will be restored later
            float angle = Solution[i];

            // Gradient : [F(x+SamplingDistance) - F(x)] / h
            float f_x = DistanceFromTarget(target, Solution);

            Solution[i] += delta;
            float f_x_plus_d = DistanceFromTarget(target, Solution);

            float gradient = (f_x_plus_d - f_x) / delta;

            // Restores
            Solution[i] = angle;

            return gradient;
        }

        // Returns the distance from the target, given a solution
        private float DistanceFromTarget(Vector3 target, float[] Solution)
        {
            Vector3 point = ForwardKinematics(Solution);
            accumulatedPenalty = 0;
            if (Mathf.Abs(Solution[1] - Solution[2]) > 80 || Mathf.Abs(Solution[1] + Solution[2]) > 80)
                accumulatedPenalty = 1;
            return Vector3.Distance(point, target) + accumulatedPenalty;
        }

        public PositionRotation ForwardKinematics(float[] Solution)
        {
            Vector3 prevPoint = _tail.Bones[0].transform.position;

            // Takes object initial rotation into account
            Quaternion rotation = Quaternion.identity;


            Vector3 point = new Vector3();

            //TODO

            for (int i = 1; i < _tail.Bones.Length - 1; i++)
            {
                prevPoint = _tail.Bones[i - 1].transform.position;
                float angle;
                Vector3 axis;
                _tail.Bones[i - 1].rotation.ToAngleAxis(out angle, out axis);
                rotation *= Quaternion.AngleAxis(Solution[i - 1], axis);
                point = prevPoint + rotation * tailStartOffset[i];
                prevPoint = point;
            }

            // The end of the effector
            return new PositionRotation(prevPoint, rotation);
        }
    }
}
