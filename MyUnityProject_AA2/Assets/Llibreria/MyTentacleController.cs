using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;




namespace OctopusController
{

    
    internal class MyTentacleController

    //MAINTAIN THIS CLASS AS INTERNAL
    {

        TentacleMode tentacleMode;
        Transform[] _bones;
        public Transform _endEffectorSphere;

        public Transform[] Bones { get => _bones; }

        //Exercise 1.
        public Transform[] LoadTentacleJoints(Transform root, TentacleMode mode)
        {
            //TODO: add here whatever is needed to find the bones forming the tentacle for all modes
            //you may want to use a list, and then convert it to an array and save it into _bones
            tentacleMode = mode;

            switch (tentacleMode){
                case TentacleMode.LEG:
                    _bones = new Transform[4];
                    Transform currChild = root.GetChild(0);
                    _bones[0] = root.GetChild(0);
                    for (int i = 1; i < 4; i++)
                    {
                        _bones[i] = currChild.GetChild(1);
                        if(i == 3)
                            _endEffectorSphere = currChild.GetChild(1);
                        if (currChild.GetChild(1).childCount > 0)
                            currChild = currChild.GetChild(1);
                    }
                    //TODO: in _endEffectorsphere you keep a reference to the base of the leg
                    break;
                case TentacleMode.TAIL:
                    _bones = new Transform[6];
                    currChild = root;
                    _bones[0] = root;
                    for (int i = 1; i < 6; i++)
                    {
                        if (i != 5)
                        {
                            _bones[i] = currChild.GetChild(1);
                        }
                        else
                            _endEffectorSphere = currChild.GetChild(1);
                        if (currChild.GetChild(1).childCount > 0)
                            currChild = currChild.GetChild(1);
                    }
                    //TODO: in _endEffectorsphere you keep a reference to the red sphere 
                    break;
                case TentacleMode.TENTACLE:

                    _bones = new Transform[51];
                    currChild = root.GetChild(0);
                    for (int i = 0; i < 52; i++)
                    {
                        if (i != 51)
                            _bones[i] = currChild.GetChild(0);
                        else
                            _endEffectorSphere = currChild.GetChild(0);
                        if (currChild.GetChild(0).childCount > 0)
                            currChild = currChild.GetChild(0);
                    }
                    //TODO: in _endEffectorphere you  keep a reference to the sphere with a collider attached to the endEffector
                    break;
            }
            return Bones;
        }
    }
}
