using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using net.narazaka.vrchat.avatar_parameters_driver.editor;
using Narazaka.VRChat.AvatarParametersUtil;
using Narazaka.VRChat.AvatarParametersUtil.Editor;

[assembly: ExportsPlugin(typeof(net.narazaka.vrchat.avatar_parameters_exclusive_group.editor.AvatarParametersExclusiveGroupPlugin))]

namespace net.narazaka.vrchat.avatar_parameters_exclusive_group.editor
{
    public class AvatarParametersExclusiveGroupPlugin : Plugin<AvatarParametersExclusiveGroupPlugin>
    {
        public override string QualifiedName => "net.narazaka.vrchat.avatar_parameters_exclusive_group";

        public override string DisplayName => "Avatar Parameters Exclusive Group";

        protected override void Configure()
        {
            InPhase(BuildPhase.Generating).BeforePlugin("nadena.dev.modular-avatar").Run("AvatarParameterExclusiveGroup", ctx =>
            {
                var groups = ctx.AvatarRootObject.GetComponentsInChildren<AvatarParameterExclusiveGroup>();
                if (groups.Length == 0) return;

                var nonHierarchicalGroups = groups.Where(g => !g.PreserveHierarchy).ToArray();
                if (nonHierarchicalGroups.Length > 0)
                {
                    BuildExclusiveGroup(ctx, nonHierarchicalGroups, ctx.AvatarRootObject);
                }

                var hierarchicalGroupsByObject = groups.Where(g => g.PreserveHierarchy).GroupBy(g => g.gameObject);
                foreach (var groupSet in hierarchicalGroupsByObject)
                {
                    BuildExclusiveGroup(ctx, groupSet.ToArray(), groupSet.Key);
                }
            });
        }

        void BuildExclusiveGroup(BuildContext ctx, IList<AvatarParameterExclusiveGroup> groups, GameObject obj)
        {
            var info = ParameterInfo.ForContext(ctx);
            var rawParameters = ReferenceEquals(obj, ctx.AvatarRootObject)
                ? info.GetParametersForObject(obj)
                : AvatarParametersHierarchy.GetParametersAtHierarchy(info, ctx.AvatarRootObject, obj);
            var parameters = rawParameters.ToDistinctSubParameters();
            var parameterByName = parameters.ToDictionary(p => p.EffectiveName);

            var parameterNames = groups.SelectMany(g => g.ExclusiveParameters).Select(p => p.Parameter.Parameter).Distinct();
            var invalidParameters = parameterNames.Where(p => !parameterByName.ContainsKey(p)).ToArray();
            if (invalidParameters.Length > 0)
            {
                throw new System.InvalidOperationException($"Parameters {string.Join(", ", invalidParameters)} not found");
            }
            var clip = MakeEmptyAnimationClip();
            var animator = new AnimatorController();
            foreach (var parameterName in parameterNames)
            {
                if (parameterByName.TryGetValue(parameterName, out var parameter))
                {
                    animator.AddParameter(AvatarParametersUtil.ToAnimatorControllerParameter(parameter));
                }
            }

            var layerIndex = 0;
            foreach (var group in groups)
            {
                foreach (var exclusiveParameter in group.ExclusiveParameters)
                {
                    var layer = animator.AddLastLayer($"Avatar Parameters Driver {layerIndex}");
                    var idleState = layer.stateMachine.AddConfiguredState("idle", clip);
                    layer.stateMachine.defaultState = idleState;
                    var activeState = layer.stateMachine.AddConfiguredState("active", clip);
                    activeState.behaviours = new StateMachineBehaviour[]
                    {
                        new VRCAvatarParameterDriver
                        {
                            parameters = new List<VRC_AvatarParameterDriver.Parameter>
                            {
                                new VRC_AvatarParameterDriver.Parameter
                                {
                                    type = VRC_AvatarParameterDriver.ChangeType.Set,
                                    name = exclusiveParameter.Parameter.Parameter,
                                    value = exclusiveParameter.FallbackValue,
                                }
                            },
                            localOnly = group.LocalOnly,
                        },
                    };
                    var toIdle = activeState.AddTransition(idleState);
                    toIdle.hasExitTime = false;
                    toIdle.hasFixedDuration = true;
                    toIdle.duration = 0f;
                    toIdle.exitTime = 0f;
                    foreach (var other in group.ExclusiveParameters.Where(p => p != exclusiveParameter))
                    {
                        var reversedOtherParameter = other.Parameter.Reverse(parameterByName.TryGetValue(other.Parameter.Parameter, out var pp) ? pp.ParameterType : null);
                        toIdle.AddCondition((AnimatorConditionMode)reversedOtherParameter.Mode, reversedOtherParameter.Threshold, reversedOtherParameter.Parameter);
                        var toActive = idleState.AddTransition(activeState);
                        toActive.hasExitTime = false;
                        toActive.hasFixedDuration = true;
                        toActive.duration = 0f;
                        toActive.exitTime = 0f;
                        toActive.AddCondition((AnimatorConditionMode)other.Parameter.Mode, other.Parameter.Threshold, other.Parameter.Parameter);
                    }
                    layerIndex++;
                }
            }
            var mergeAnimator = obj.AddComponent<ModularAvatarMergeAnimator>();
            mergeAnimator.animator = animator;
            mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            mergeAnimator.matchAvatarWriteDefaults = true;

            foreach (var group in groups)
            {
                Object.DestroyImmediate(group);
            }
        }

        AnimationClip MakeEmptyAnimationClip()
        {
            var clip = new AnimationClip();
            clip.SetCurve("__AvatarParameterExclusiveGroup_EMPTY__", typeof(GameObject), "localPosition.x", new AnimationCurve { keys = new Keyframe[] { new Keyframe { time = 0, value = 0 }, new Keyframe { time = 1f / 60f, value = 0 } } });
            return clip;
        }
    }
}
