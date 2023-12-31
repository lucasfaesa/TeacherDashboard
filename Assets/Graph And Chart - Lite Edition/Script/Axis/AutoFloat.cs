﻿using System;

namespace ChartAndGraph
{
    /// <summary>
    ///     this class holds a float value that can be either set mannualy or be set automatically by an external class
    /// </summary>
    [Serializable]
    public struct AutoFloat
    {
        /// <summary>
        ///     true if the value is automatic
        /// </summary>
        public bool Automatic;

        /// <summary>
        ///     the mannual value , this is only relevant if Automatic == false
        /// </summary>
        public float Value;

        /// <summary>
        ///     creats a new instance of AutoFloat
        /// </summary>
        /// <param name="automatic"></param>
        /// <param name="value"></param>
        public AutoFloat(bool automatic, float value)
        {
            Automatic = automatic;
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is AutoFloat)
            {
                var cast = (AutoFloat)obj;
                if (cast.Automatic && Automatic)
                    return true;
                if (cast.Automatic == false && Automatic == false && cast.Value == Value)
                    return true;
                return false;
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (Automatic)
                return Automatic.GetHashCode();
            return Value.GetHashCode();
        }
    }
}