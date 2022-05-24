﻿using Detached.Mappers.Context;
using System;

namespace Detached.Mappers.TypeMappers.PrimitiveType
{
    public class PrimitiveTypeMapper<TSource, TTarget> : ITypeMapper<TSource, TTarget>
    {
        public TTarget Map(TSource source, TTarget target, IMapperContext mapperContext)
        {
            return (TTarget)Convert.ChangeType(source, typeof(TTarget));
        }

        public object Map(object source, object target, IMapperContext mapperContext)
        {
            return Map((TSource)source, (TTarget)target, mapperContext);
        }
    }
}
