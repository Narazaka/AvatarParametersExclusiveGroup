using UnityEngine;
using VRC.SDKBase;

namespace net.narazaka.vrchat.avatar_parameters_exclusive_group
{
    public class AvatarParameterExclusiveGroup : MonoBehaviour, IEditorOnly
    {
        [SerializeField]
        public bool LocalOnly;
        [SerializeField]
        public ExclusiveParameter[] ExclusiveParameters;
    }
}
