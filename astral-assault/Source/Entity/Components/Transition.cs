using System;

namespace AstralAssault
{
    public readonly struct Transition
    {
        public String ConditionName { get; }
        public Single ConditionThreshold { get; }

        public Int32 From { get; }
        public Int32 To { get; }

        public Int32[] AnimationPath { get; }
        public Tuple<Int32, Int32> FromTo => new(this.From, this.To);

        public Transition(Int32 from, Int32 to, Int32[] animationPath, String conditionName,
            Single conditionThreshold) {
            this.From = from;
            this.To = to;
            this.AnimationPath = animationPath;
            this.ConditionName = conditionName;
            this.ConditionThreshold = conditionThreshold;
        }
    }
}