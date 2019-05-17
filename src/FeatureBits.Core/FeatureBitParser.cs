﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace FeatureBits.Core
{
    public static class FeatureBitParser
    {
        /// <summary>
        /// Comma-separated list split into collection of Ints
        /// </summary>
        /// <param name="Dependencies"></param>
        /// <returns>Collection of ints</returns>
        /// <exception cref="ArgumentException"></exception>
        public static IEnumerable<int> SplitToInts(this string Dependencies)
        {
            if (!string.IsNullOrEmpty(Dependencies))
            {
                return Dependencies.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(Id => Convert.ToInt32(Id));
            }
            return new List<int>();
        }

        /// <summary>
        /// Comma-separated list split into collection of FeatureBit names
        /// </summary>
        /// <param name="Dependencies"></param>
        /// <returns>Collection of strings</returns>
        /// <exception cref="ArgumentException"></exception>
        public static IEnumerable<string> SplitToStrings(this string Dependencies)
        {
            if (!string.IsNullOrEmpty(Dependencies))
            {
                return Dependencies.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)?.Select(s => s.Trim());
            }
            return new List<string>();
        }
    }
}