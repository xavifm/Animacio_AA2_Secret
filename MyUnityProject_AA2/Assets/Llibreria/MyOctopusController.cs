using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{
    public enum TentacleMode { LEG, TAIL, TENTACLE };

    public class MyOctopusController 
    {
        
        MyTentacleController[] _tentacles =new  MyTentacleController[4];

        Transform _currentRegion;
        Transform _target;

        Transform[] _randomTargets;// = new Transform[4];


        float _twistMin, _twistMax;
        float _swingMin, _swingMax;

		//NEW VARIABLES
		// Array of angles to rotate by (for each joint), as well as sin and cos values
		[SerializeField]
		float[] _theta, _sin, _cos;

		// To check if the target is reached at any point
		bool _done = false;

		// To store the position of the target
		private Vector3 tpos;

		// Max number of tries before the system gives up (Maybe 10 is too high?)
		[SerializeField]
		private int _mtries = 10;
		// The number of tries the system is at now
		[SerializeField]
		private int _tries = 0;

		// the range within which the target will be assumed to be reached
		public readonly float _epsilon = 0.1f;
		//___________________________________________________________________________

		#region public methods
		//DO NOT CHANGE THE PUBLIC METHODS!!

		public float TwistMin { set => _twistMin = value; }
        public float TwistMax { set => _twistMax = value; }
        public float SwingMin {  set => _swingMin = 10; }
        public float SwingMax { set => _swingMax = 20; }
        

        public void TestLogging(string objectName)
        {

           
            Debug.Log("hello, I am initializing my Octopus Controller in object "+objectName);

            
        }

        public void Init(Transform[] tentacleRoots, Transform[] randomTargets)
        {
			_tentacles = new MyTentacleController[tentacleRoots.Length];

            // foreach (Transform t in tentacleRoots)
            for(int i = 0;  i  < tentacleRoots.Length; i++)
            {
                _tentacles[i] = new MyTentacleController();
                _tentacles[i].LoadTentacleJoints(tentacleRoots[i],TentacleMode.TENTACLE);
				//TODO: initialize any variables needed in ccd
			}

            _randomTargets = randomTargets;
			//TODO: use the regions however you need to make sure each tentacle stays in its region

			_theta = new float[_tentacles[0].Bones.Length];
			_sin = new float[_tentacles[0].Bones.Length];
			_cos = new float[_tentacles[0].Bones.Length];

		}
   
        public void NotifyTarget(Transform target, Transform region)
        {
            _currentRegion = region;
            _target = target;
        }

        public void NotifyShoot() {
            //TODO. what happens here?
            Debug.Log("Shoot");
        }


        public void UpdateTentacles()
        {
            //TODO: implement logic for the correct tentacle arm to stop the ball and implement CCD method
            update_ccd();
        }




        #endregion


        #region private and internal methods
        //todo: add here anything that you need

        void update_ccd() {
			// if the target hasn't been reached
			if (!_done)
			{
				for (int o = 0; o < _tentacles.Length; o++)
				{
					if (_randomTargets[o].position != tpos)
					{
						_tries = 0;
						tpos = _randomTargets[o].position;
					}
					// if the Max number of tries hasn't been reached
					if (_tries <= _mtries)
					{
						// starting from the second last joint (the last being the end effector)
						// going back up to the root
						for (int i = _tentacles[o].Bones.Length - 2; i >= 0; i--)
						{
							// The vector from the ith joint to the end effector
							Vector3 r1 = _tentacles[o].Bones[_tentacles[o].Bones.Length - 1].transform.position - _tentacles[o].Bones[i].transform.position;

							// The vector from the ith joint to the target
							Vector3 r2 = tpos - _tentacles[o].Bones[i].transform.position;

							Vector3 axis = new Vector3();

							// to avoid dividing by tiny numbers
							if (r1.magnitude * r2.magnitude <= 0.001f)
							{
								// cos ? sin? 
								//TODO3
							}
							else
							{
								// find the components using dot and cross product
								_cos[i] = Vector3.Dot(r1.normalized, r2.normalized);
								_cos[i] = Mathf.Acos(_cos[i]);
								_sin[i] = Vector3.Dot(r1.normalized, r2.normalized);
								_sin[i] = Mathf.Asin(_sin[i]);
								axis = (Vector3.Cross(r1, r2)).normalized;
								if (_cos[i] <= 0.01f && _cos[i] >= 0.98f)
									continue;
							}

							// The axis of rotation 
							//Vector3 axis = TODO5

							// find the angle between r1 and r2 (and clamp values if needed avoid errors)
							_theta[i] = Mathf.Clamp(_cos[i], -Mathf.PI, Mathf.PI);


							//Optional. correct angles if needed, depending on angles invert angle if sin component is negative
							//if (TODO)
							//	theta[i] = TODO7
							if ((_sin[i] < 0 && _theta[i] > 0) || (_sin[i] > 0 && _theta[i] < 0))
							{
								_sin[i] = -_sin[i];
								_theta[i] = Mathf.Asin(_sin[i]);
							}

							// obtain an angle value between -pi and pi, and then convert to degrees
							_theta[i] = Mathf.Clamp(_theta[i], -Mathf.PI, Mathf.PI);
							_theta[i] = Mathf.Rad2Deg * _theta[i];

							// rotate the ith joint along the axis by theta degrees in the world space.
							// TODO9
							_tentacles[o].Bones[i].transform.Rotate(axis, _theta[i], Space.World);

							//MirrorMovement
							float localAngle;
							Vector3 localAxis;
							_tentacles[o].Bones[i].localRotation.ToAngleAxis(out localAngle, out localAxis);
							Quaternion TwistQuat;
							Quaternion SwingQuat;
							float SwingAngle;
							Vector3 SwingAxis;
							Quaternion TempQuat = new Quaternion(0, _tentacles[o].Bones[i].transform.localRotation.y, 0, _tentacles[o].Bones[i].transform.localRotation.w);
							TwistQuat = Quaternion.Normalize(TempQuat);

							SwingQuat = Quaternion.Inverse(TwistQuat) * new Quaternion(_tentacles[o].Bones[i].transform.localRotation.x, 0, 0, _tentacles[o].Bones[i].transform.localRotation.w);

							SwingQuat.ToAngleAxis(out SwingAngle, out SwingAxis);

							SwingAngle = Mathf.Clamp(SwingAngle, _swingMin, _swingMax);

							SwingQuat = Quaternion.AngleAxis(SwingAngle, SwingAxis);

							_tentacles[o].Bones[i].transform.localRotation = new Quaternion(SwingQuat.x, 0, 0, SwingQuat.w);
							//_____________________________________________

						}

						// increment tries
						_tries++;

					}

					// find the difference in the positions of the end effector and the target
					// TODO10

					// if target is within reach (within epsilon) then the process is done
					if (Vector3.Distance(_tentacles[o].Bones[_tentacles[o].Bones.Length - 1].transform.position, tpos) < _epsilon)
					{
						_done = true;
					}
					// if it isn't, then the process should be repeated
					else
					{
						_done = false;
					}
				}
			}
	}


        

        #endregion






    }
}
