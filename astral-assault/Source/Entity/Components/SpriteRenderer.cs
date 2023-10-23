using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstralAssault
{
    public class SpriteRenderer : IUpdateEventListener
    {
        public readonly EffectContainer EffectContainer = new();

        private Int32 CurrentAnimationIndex { get; set; }
        private readonly LayerDepth _layerDepth;
        private readonly Animation[] _animations;
        private readonly Texture2D _spriteSheet;
        private Animation CurrentAnimation => this._animations[this.CurrentAnimationIndex];
        private Int32 _currentFrameIndex;
        private Int32 _targetAnimationIndex;
        private Int32 _startAnimationIndex;
        private Boolean _isTransitioning;
        private Int64 _lastFrameUpdate;
        private readonly Dictionary<Tuple<Int32, Int32>, Transition> _animationPaths = new();
        private Int32[] _animationQueue;
        private Int32 _indexInQueue;
        private readonly Dictionary<String, Single> _animationConditions = new();

        public Boolean Debugging = false;

        private const Single Pi = 3.14F;

        public SpriteRenderer(
            Texture2D spriteSheet,
            Animation[] animations,
            LayerDepth layerDepth,
            Transition[] transitions,
            String[] animationConditions) {
            this._animations = animations;
            this._spriteSheet = spriteSheet;
            this._layerDepth = layerDepth;

            if (transitions != null) {
                foreach (Transition transition in transitions) this._animationPaths.Add(transition.FromTo, transition);
            }

            if (animationConditions != null) this.InitAnimationConditions(animationConditions);

            UpdateEventSource.UpdateEvent += this.OnUpdate;
            this.CurrentAnimationIndex = 0;
        }

        public SpriteRenderer(
            Texture2D spriteSheet,
            Frame frame,
            LayerDepth layerDepth) {
            Animation animation = new(
                new[] { frame },
                frame.HasRotations);

            this._animations = new[] { animation };
            this._spriteSheet = spriteSheet;
            this._layerDepth = layerDepth;

            this.CurrentAnimationIndex = 0;
        }

        public void OnUpdate(Object sender, UpdateEventArgs e) {
            if (this.Debugging) Debug.WriteLine(this._currentFrameIndex);

            Int32 frameLength = this.CurrentAnimation.Frames[this._currentFrameIndex].Time;
            Int64 timeNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            Transition? transition = this.GetPossibleTransition();

            if (transition.HasValue) {
                if (this.Debugging) Debug.WriteLine("Transitioning");
                this._animationQueue = transition.Value.AnimationPath;
                this.CurrentAnimationIndex = this._animationQueue[0];
                this._indexInQueue = 0;
                this._currentFrameIndex = 0;
                this._lastFrameUpdate = timeNow;
                this._targetAnimationIndex = transition.Value.To;
                this._isTransitioning = true;
                return;
            }

            if (timeNow < (this._lastFrameUpdate + frameLength)) return;

            if (this._animationQueue == null) {
                if (!this.CurrentAnimation.IsLooping) return;

                this._currentFrameIndex = (this._currentFrameIndex + 1) % this.CurrentAnimation.Frames.Length;
                this._lastFrameUpdate = timeNow;

                return;
            }

            if (((this._currentFrameIndex + 1) == this.CurrentAnimation.Frames.Length) &&
                ((this._indexInQueue + 1) < this._animationQueue.Length)) {
                this.CurrentAnimationIndex = this._animationQueue[++this._indexInQueue];
                this._currentFrameIndex = 0;
            }
            else
                this._currentFrameIndex = (this._currentFrameIndex + 1) % this.CurrentAnimation.Frames.Length;

            if ((this.CurrentAnimationIndex == this._targetAnimationIndex) && this._isTransitioning) {
                this._isTransitioning = false;
                this._startAnimationIndex = this._targetAnimationIndex;
            }

            this._lastFrameUpdate = timeNow;
        }

        public DrawTask CreateDrawTask(Vector2 position, Single rotation) => this.CurrentAnimation.HasRotation
            ? this.DrawRotatable(position, rotation)
            : this.DrawStatic(position);

        private DrawTask DrawStatic(Vector2 position) {
            Rectangle source = this.CurrentAnimation.Frames[this._currentFrameIndex].Source;
            return new DrawTask(this._spriteSheet, source, position, 0, this._layerDepth, this.EffectContainer.Effects);
        }

        private DrawTask DrawRotatable(Vector2 position, Single rotation) {
            (Single spriteRotation, Rectangle source) = this.GetRotation(rotation);
            return new DrawTask(this._spriteSheet, source, position, spriteRotation, this._layerDepth,
                this.EffectContainer.Effects);
        }

        private Tuple<Single, Rectangle> GetRotation(Single rotation) {
            Int32 rot = (Int32)Math.Round(rotation / (Pi / 8));

            Single spriteRotation;
            Rectangle source;

            if ((rot % 4) == 0) {
                source = this.CurrentAnimation.Frames[this._currentFrameIndex].Rotations[0];
                spriteRotation = (Pi / 8) * rot;
                return new Tuple<Single, Rectangle>(spriteRotation, source);
            }

            spriteRotation = rotation switch {
                >= 0 and < Pi / 2 => 0,
                >= Pi / 2 and < Pi => Pi / 2,
                <= 0 and > -Pi / 2 => -Pi / 2,
                <= -Pi / 2 and > -Pi => -Pi,
                _ => 0
            };

            source = this.CurrentAnimation.Frames[this._currentFrameIndex].Rotations[rot.Mod(4)];

            return new Tuple<Single, Rectangle>(spriteRotation, source);
        }

        private Transition? GetPossibleTransition() {
            foreach (KeyValuePair<String, Single> condition in this._animationConditions) {
                foreach (Transition transition in this._animationPaths.Values) {
                    if ((transition.From == this._startAnimationIndex) &&
                        (transition.To != this._targetAnimationIndex) &&
                        (transition.ConditionName == condition.Key) &&
                        (transition.ConditionThreshold == condition.Value))
                        return transition;
                }
            }

            return null;
        }

        public void SetAnimationCondition(String name, Single value) {
            if (!this._animationConditions.ContainsKey(name))
                throw new ArgumentException($"Animation condition '{name}' does not exist");

            this._animationConditions[name] = value;
        }

        private void InitAnimationConditions(String[] name) {
            if (this._animationConditions.Count != 0)
                throw new InvalidOperationException("Animation conditions already initialized");

            foreach (String s in name) this._animationConditions.Add(s, 0);
        }
    }
}