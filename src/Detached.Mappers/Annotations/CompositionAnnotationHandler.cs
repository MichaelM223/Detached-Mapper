﻿using Detached.Annotations;
using Detached.Mappers.Types;
using Detached.Mappers.Types.Class;
using Detached.Mappers.Types.Class.Builder;

namespace Detached.Mappers.Annotations
{
    public class CompositionAnnotationHandler : AnnotationHandler<CompositionAttribute>
    {
        public override void Apply(CompositionAttribute annotation, MapperOptions modelOptions, ClassType typeOptions, ClassTypeMember memberOptions)
        {
            memberOptions.IsComposition(true);
        }
    }

    public static class CompositionAnnotationHandlerExtensions
    {
        const string KEY = "DETACHED_COMPOSITION";

        public static bool IsComposition(this ITypeMember member)
        {
            return member.Annotations.ContainsKey(KEY);
        }

        public static void IsComposition(this ITypeMember member, bool value)
        {
            if (value)
                member.Annotations[KEY] = true;
            else
                member.Annotations.Remove(KEY);
        }

        public static void IsComposition<TType, TMember>(this ClassTypeMemberBuilder<TType, TMember> member, bool value)
        {
            member.MemberOptions.IsComposition(value);
        }
    }
}