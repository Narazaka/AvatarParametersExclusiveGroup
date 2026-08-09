using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using nadena.dev.ndmf.localization;
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
                var invalidParameterSet = new HashSet<string>(invalidParameters);
                // 複数コンポーネントがマージされている場合があるため、どのコンポーネントが無効なパラメーターを参照しているか個別に報告する
                foreach (var group in groups)
                {
                    var missingInGroup = group.ExclusiveParameters.Select(p => p.Parameter.Parameter).Where(invalidParameterSet.Contains).Distinct().ToArray();
                    if (missingInGroup.Length == 0) continue;
                    using (ErrorReport.WithContextObject(group.gameObject))
                    {
                        ErrorReport.ReportError(new ParametersNotFoundError(missingInGroup));
                    }
                }
                return;
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

            foreach (var group in groups)
            {
                foreach (var exclusiveParameter in group.ExclusiveParameters)
                {
                    var layer = animator.AddLastLayer($"Avatar Parameters Exclusive Group [{group.gameObject.name}]");
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

        static readonly istring Title = new istring("Parameters not found", "パラメーターが見付かりませんでした");

        // NDMF標準の SimpleError を使うと、Error Report ウィンドウで他のエラーと同じ
        // アイコン + タイトル + 内容のレイアウトになる。ローカライズ機構は使わないので、
        // キーを素通しして常に渡された文字列（istring が現在の言語で解決した文字列）を
        // そのまま表示する Localizer を充てる
        static readonly Localizer PassthroughLocalizer = new Localizer("en-US", () => new List<(string, System.Func<string, string>)>
        {
            ("en-US", key => "{0}"),
        });

        class ParametersNotFoundError : SimpleError
        {
            readonly string detail;

            public ParametersNotFoundError(string[] parameters)
            {
                detail = string.Join(", ", parameters);
            }

            public override Localizer Localizer => PassthroughLocalizer;
            public override string TitleKey => Title;
            public override string[] TitleSubst => new string[] { Title };
            public override string DetailsKey => detail;
            public override string[] DetailsSubst => new[] { detail };
            public override string FormatHint() => null;
            public override ErrorSeverity Severity => ErrorSeverity.Error;
        }
    }
}
