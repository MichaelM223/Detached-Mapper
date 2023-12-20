﻿using Detached.Mappers.Annotations;
using Detached.Mappers.Types;
using Detached.Mappers.Types.Class;
using System.Collections.Generic;

namespace Detached.Mappers.TypePairs
{
    public class TypePairFactory : ITypePairFactory
    {
        public TypePair Create(MapperOptions mapperOptions, IType sourceType, IType targetType, TypePairMember parentMember)
        {
            TypePair typePair = new TypePair(sourceType, targetType, parentMember);

            IEnumerable<string> memberNames = targetType.MemberNames;

            if (memberNames != null)
            {
                foreach (string targetMemberName in memberNames)
                {
                    ITypeMember targetMember = targetType.GetMember(targetMemberName); 

                    if (targetMember != null)
                    {
                        TypePairMember member = new TypePairMember();
                        
                        foreach(var annotation in targetMember.Annotations)
                        {
                            member.Annotations[annotation.Key] = annotation.Value;
                        }

                        member.TargetType = targetType;
                        member.SourceType = sourceType;
                        member.TargetMember = targetMember;

                        string sourceMemberName = GetSourcePropertyName(mapperOptions, sourceType, targetType, targetMemberName);

                        ITypeMember sourceMember = sourceType.GetMember(sourceMemberName);
                        if (sourceMember != null && sourceMember.CanRead && !sourceMember.IsIgnored())
                        {
                            member.SourceMember = sourceMember;
                        }
                        else if (!member.IsParent())
                        {
                            member.Exclude();
                        }

                        typePair.Members.Add(targetMemberName, member);
                    }
                }
            }

            return typePair;
        }

        public string GetSourcePropertyName(MapperOptions mapperOptions, IType sourceType, IType targetType, string memberName)
        {
            for (int i = mapperOptions.PropertyNameConventions.Count - 1; i >= 0; i--)
            {
                memberName = mapperOptions.PropertyNameConventions[i].GetSourcePropertyName(sourceType, targetType, memberName);
            }

            return memberName;
        }
    }
}
