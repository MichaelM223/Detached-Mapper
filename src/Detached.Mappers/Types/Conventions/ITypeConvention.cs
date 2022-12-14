﻿using Detached.Mappers.Types.Class;

namespace Detached.Mappers.Types.Conventions
{
    public interface ITypeConvention
    {
        void Apply(MapperOptions modelOptions, ClassType typeOptions);
    }
}
