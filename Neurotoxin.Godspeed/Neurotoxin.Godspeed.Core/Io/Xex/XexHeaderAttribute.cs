using System;
using Neurotoxin.Godspeed.Core.Constants;

namespace Neurotoxin.Godspeed.Core.Io.Xex
{
    public class XexHeaderAttribute : Attribute
    {
        public XexOptionalHeaderId Id { get; private set; }

        public XexHeaderAttribute(XexOptionalHeaderId id)
        {
            Id = id;
        }
    }
}