﻿using Detached.Mappers.TypeMaps;
using Detached.RuntimeTypes.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using static Detached.RuntimeTypes.Expressions.ExtendedExpression;
using static System.Linq.Expressions.Expression;

namespace Detached.Mappers.EntityFramework.Queries
{
    public class DetachedQueryProvider
    {
        readonly Mapper _mapper;
        IMemoryCache _memoryCache;

        public DetachedQueryProvider(Mapper mapper, IMemoryCache memoryCache)
        {
            _mapper = mapper;
            _memoryCache = memoryCache;
        }

        public IQueryable<TSource> Project<TSource, TTarget>(IQueryable<TTarget> query)
            where TTarget : class
            where TSource : class
        {
            var key = new DetachedQueryCacheKey(typeof(TSource), typeof(TTarget), QueryType.Projection);

            var filter = _memoryCache.GetOrCreate(key, entry =>
            {
                TypeMap typeMap = _mapper.GetTypeMap(typeof(TSource), typeof(TTarget));
                var param = Parameter(typeMap.TargetTypeOptions.ClrType, "e");
                Expression projection = ToLambda(typeMap.TargetTypeOptions.ClrType, param, CreateSelectProjection(typeMap, param));

                entry.SetSize(1);

                return (Expression<Func<TTarget, TSource>>)projection;
            });

            return query.Select(filter);
        }

        public Task<TTarget> LoadAsync<TSource, TTarget>(IQueryable<TTarget> queryable, TSource source)
            where TSource : class
            where TTarget : class
        {
            return GetTemplate<TSource, TTarget>().Render(queryable, source).FirstOrDefaultAsync();
        }

        public TTarget Load<TSource, TTarget>(IQueryable<TTarget> queryable, TSource source)
            where TSource : class
            where TTarget : class
        {
            return GetTemplate<TSource, TTarget>().Render(queryable, source).FirstOrDefault();
        }

        DetachedQueryTemplate<TSource, TTarget> GetTemplate<TSource, TTarget>()
            where TSource : class
            where TTarget : class
        {
            var key = new DetachedQueryCacheKey(typeof(TSource), typeof(TTarget), QueryType.Load);

            return _memoryCache.GetOrCreate(key, entry =>
            {
                TypeMap typeMap = _mapper.GetTypeMap(typeof(TSource), typeof(TTarget));

                DetachedQueryTemplate<TSource, TTarget> queryTemplate = new DetachedQueryTemplate<TSource, TTarget>();

                queryTemplate.SourceConstant = Constant(null, typeMap.SourceTypeOptions.ClrType);
                queryTemplate.FilterExpression = CreateFilter<TSource, TTarget>(typeMap, queryTemplate.SourceConstant);
                GetIncludes(typeMap, queryTemplate.Includes, null);

                entry.SetSize(1);

                return queryTemplate;
            });
        }

        Expression<Func<TTarget, bool>> CreateFilter<TSource, TTarget>(TypeMap typeMap, ConstantExpression sourceConstant)
        {
            var targetParam = Parameter(typeMap.TargetTypeOptions.ClrType, "e");

            Expression expression = null;

            foreach (MemberMap memberMap in typeMap.Members)
            {
                if (memberMap.IsKey)
                {
                    var targetExpr = memberMap.TargetOptions.BuildGetterExpression(targetParam, null);
                    var sourceExpr = memberMap.SourceOptions.BuildGetterExpression(sourceConstant, null);

                    if (sourceExpr.Type.IsNullable(out _))
                    {
                        sourceExpr = Property(sourceExpr, "Value");
                    }

                    if (expression == null)
                    {
                        expression = Equal(targetExpr, sourceExpr);
                    }
                    else
                    {
                        expression = And(expression, Equal(targetExpr, sourceExpr));
                    }
                }
            }

            return Lambda<Func<TTarget, bool>>(expression, targetParam);
        }

        void GetIncludes(TypeMap typeMap, List<string> includes, string prefix)
        {
            foreach (MemberMap memberMap in typeMap.Members)
            {
                if (!memberMap.IsBackReference)
                {
                    if (memberMap.TypeMap.TargetTypeOptions.IsCollection)
                    {
                        string name = prefix + memberMap.TargetOptions.Name;
                        includes.Add(name);

                        if (memberMap.IsComposition)
                        {
                            GetIncludes(memberMap.TypeMap.ItemTypeMap, includes, name + ".");
                        }
                    }
                    else if (memberMap.TypeMap.TargetTypeOptions.IsComplex)
                    {
                        string name = prefix + memberMap.TargetOptions.Name;
                        includes.Add(name);

                        if (memberMap.IsComposition)
                        {
                            GetIncludes(memberMap.TypeMap, includes, name + ".");
                        }
                    }
                }
            }
        }

        Expression CreateSelectProjection(TypeMap typeMap, Expression targetExpr)
        {
            if (typeMap.SourceTypeOptions.IsCollection)
            {
                var itemType = typeMap.ItemTypeMap.TargetTypeOptions.ClrType;
                var param = Parameter(itemType, "e");
                var itemMap = CreateSelectProjection(typeMap.ItemTypeMap, param);

                LambdaExpression lambda = ToLambda(itemType, param, itemMap);

                return Call("ToList", typeof(Enumerable), Call("Select", typeof(Enumerable), targetExpr, lambda));
            }
            else if (typeMap.SourceTypeOptions.IsComplex)
            {
                List<MemberBinding> bindings = new List<MemberBinding>();
                foreach (MemberMap memberMap in typeMap.Members)
                {
                    if (!memberMap.IsBackReference)
                    {
                        PropertyInfo propInfo = typeMap.SourceTypeOptions.ClrType.GetProperty(memberMap.SourceOptions.Name);
                        if (propInfo != null)
                        {
                            Expression map = memberMap.TargetOptions.BuildGetterExpression(targetExpr, null);
                            map = CreateSelectProjection(memberMap.TypeMap, map);
                            bindings.Add(Bind(propInfo, map));
                        }
                    }
                }

                return MemberInit(New(typeMap.SourceTypeOptions.ClrType), bindings.ToArray());
            }
            else
            {
                return targetExpr;
            }
        }

        LambdaExpression ToLambda(Type type, ParameterExpression paramExpr, Expression body)
        {
            Type funcType = typeof(Func<,>).MakeGenericType(type, body.Type);
            var lambda = Lambda(funcType, body, new[] { paramExpr });
            return lambda;
        }
    }
}