﻿using System;

namespace Detached.Mappers.TypeMappers.POCO.Nullable
{
    public class NullableTypeMapper<TSource, TTarget> : TypeMapper<TSource?, TTarget?>
        where TSource : struct
        where TTarget : struct
    {
        readonly ITypeMapper<TSource, TTarget> _baseMapper;

        public NullableTypeMapper(ITypeMapper<TSource, TTarget> baseMapper)
        {
            _baseMapper = baseMapper;
        }

        public override TTarget? Map(TSource? source, TTarget? target, IMapContext context)
        {
            if (source.HasValue)
                return (TTarget?)_baseMapper.Map(source.Value, target ?? default(TTarget), context);
            else
                return null;
        }
    }
}