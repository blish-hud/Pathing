using System;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Glide;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Behavior.Modifier {
    public class BounceModifier : Behavior<StandardMarker>, ICanFocus {

        public const  string PRIMARY_ATTR_NAME = "bounce";
        private const string ATTR_DELAY        = PRIMARY_ATTR_NAME + "-delay";
        private const string ATTR_HEIGHT       = PRIMARY_ATTR_NAME + "-height";
        private const string ATTR_DURATION     = PRIMARY_ATTR_NAME + "-duration";

        private readonly IPackState _packState;

        private readonly float _originalVerticalOffset;

        private const BounceBehavior DEFAULT_BOUNCEBEHAVIOR = BounceBehavior.Bounce;
        private const float          DEFAULT_BOUNCEDELAY    = 0f;
        private const float          DEFAULT_BOUNCEHEIGHT   = 2f;
        private const float          DEFAULT_BOUNCEDURATION = 1f;

        public BounceBehavior Behavior       { get; set; }
        public float          BounceDelay    { get; set; }
        public float          BounceHeight   { get; set; }
        public float          BounceDuration { get; set; }

        private Tween _bounceAnimation;

        public BounceModifier(BounceBehavior bounceBehavior, float delay, float height, float duration, StandardMarker marker, IPackState packState) : base(marker) {
            _packState = packState;

            this.Behavior       = bounceBehavior;
            this.BounceDelay    = delay;
            this.BounceHeight   = height;
            this.BounceDuration = duration;
            
            _originalVerticalOffset = _pathingEntity.HeightOffset;
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes, StandardMarker marker, IPackState packState) {
            return new BounceModifier(attributes.TryGetAttribute(PRIMARY_ATTR_NAME, out var behaviorAttr) ? behaviorAttr.GetValueAsEnum<BounceBehavior>() : DEFAULT_BOUNCEBEHAVIOR, 
                                      attributes.TryGetAttribute(ATTR_DELAY,        out var delayAttr) ? delayAttr.GetValueAsFloat(DEFAULT_BOUNCEDELAY) : DEFAULT_BOUNCEDELAY,
                                      attributes.TryGetAttribute(ATTR_HEIGHT,       out var heightAttr) ? heightAttr.GetValueAsFloat(DEFAULT_BOUNCEHEIGHT) : DEFAULT_BOUNCEHEIGHT,
                                      attributes.TryGetAttribute(ATTR_DURATION,     out var durationAttr) ? durationAttr.GetValueAsFloat(DEFAULT_BOUNCEDURATION) : DEFAULT_BOUNCEDURATION,
                                      marker,
                                      packState);
        }

        public void Focus() {
            if (!_packState.UserConfiguration.PackAllowMarkersToAnimate.Value) return;

            _bounceAnimation?.CancelAndComplete();

            if (_pathingEntity.BehaviorFiltered) {
                return;
            }

            _bounceAnimation = GameService.Animation.Tweener.Tween(_pathingEntity,
                                                                   new { HeightOffset = _originalVerticalOffset + this.BounceHeight },
                                                                   this.BounceDuration,
                                                                   this.BounceDelay)
                                          .From(new { HeightOffset = _originalVerticalOffset });

            switch (this.Behavior) {
                case BounceBehavior.Bounce:
                    _bounceAnimation = _bounceAnimation.Ease(Ease.QuadInOut).Repeat().Reflect();
                    break;
                case BounceBehavior.Rise:
                    _bounceAnimation = _bounceAnimation.Ease(Ease.QuartInOut);
                    break;
            }
        }

        public void Unfocus() {
            _bounceAnimation?.Cancel();

            _bounceAnimation = GameService.Animation.Tweener.Tween(_pathingEntity,
                                                                   new { HeightOffset = _originalVerticalOffset },
                                                                   _pathingEntity.HeightOffset / 2f)
                                          .Ease(Ease.BounceOut);
        }

        public override void Unload() {
            _bounceAnimation?.Cancel();
        }

        public enum BounceBehavior {
            Bounce,
            Rise
        }

    }
}
