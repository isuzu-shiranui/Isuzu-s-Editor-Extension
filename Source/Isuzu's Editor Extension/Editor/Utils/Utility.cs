using System;
using System.Linq.Expressions;
using UnityEngine;

namespace IsuzuEditorExtension.Utils
{
    public static class Utility
    {
        /// <summary>
        ///     プロパティ名を取得する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string NameOf<T>(Expression<Func<T>> e)
        {
            var member = (MemberExpression) e.Body;
            return member.Member.Name;
        }

        public static string SerializeColor(Color color)
        {
            return string.Format("{0},{1},{2},{3}", color.r, color.g, color.b, color.a);
        }

        public static Color DeserializeColor(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Argument is null.", value);

            var split = value.Split(',');
            if (split.Length < 4) throw new ArgumentException("Elements are missing.", value);

            var r = 0f;
            var g = 0f;
            var b = 0f;
            var a = 0f;
            if (!float.TryParse(split[0], out r)) throw new ArgumentException("Color.r can not convert.", value);
            if (!float.TryParse(split[1], out g)) throw new ArgumentException("Color.g can not convert.", value);
            if (!float.TryParse(split[2], out b)) throw new ArgumentException("Color.b can not convert.", value);
            if (!float.TryParse(split[3], out a)) throw new ArgumentException("Color.a can not convert.", value);

            return new Color(r, g, b, a);
        }
    }
}