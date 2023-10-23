using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstralAssault
{
    public static class ExtensionMethods
    {
        public static Int32 Mod(this Int32 x, Int32 y) => (Math.Abs(x * y) + x) % y;

        public static Tuple<TKey, TValue> ToTuple<TKey, TValue>(this KeyValuePair<TKey, TValue> keyValuePair) =>
            new(keyValuePair.Key, keyValuePair.Value);

        public static Vector2 Random(this Vector2 lol) {
            lol.X = new Random().Next(0, Game1.TargetWidth);
            lol.Y = new Random().Next(0, Game1.TargetHeight);

            return lol;
        }
    }
}