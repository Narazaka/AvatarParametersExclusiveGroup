using System;
using UnityEngine;
using net.narazaka.vrchat.avatar_parameters_driver;

namespace net.narazaka.vrchat.avatar_parameters_exclusive_group
{
    [Serializable]
    public class ExclusiveParameter
    {
        [SerializeField]
        public DriveCondition Parameter;
        [SerializeField]
        public float FallbackValue;
    }
}
