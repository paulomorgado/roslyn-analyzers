﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.PropertySetAnalysis
{
#pragma warning disable CA1812 // Is too instantiated.
    internal sealed class PropertyMapperCollection
#pragma warning restore CA1812
    {
        public PropertyMapperCollection(IEnumerable<PropertyMapper> propertyMappers)
        {
            if (propertyMappers == null)
            {
                throw new ArgumentNullException(nameof(propertyMappers));
            }

            ImmutableDictionary<string, (int Index, PropertyMapper PropertyMapper)>.Builder builder = ImmutableDictionary.CreateBuilder<string, (int Index, PropertyMapper PropertyMapper)>(StringComparer.Ordinal);
            int index = 0;
            foreach (PropertyMapper p in propertyMappers)
            {
                builder.Add(p.PropertyName, (index++, p));
            }

            this.PropertyMappersWithIndex = builder.ToImmutable();
        }

        public PropertyMapperCollection(params PropertyMapper[] propertyMappers)
            : this((IEnumerable<PropertyMapper>)propertyMappers)
        {
        }

        private PropertyMapperCollection()
        {
        }

        internal bool TryGetPropertyMapper(string propertyName, out PropertyMapper propertyMapper, out int index)
        {
            if (this.PropertyMappersWithIndex.TryGetValue(propertyName, out (int Index, PropertyMapper PropertyMapper) tuple))
            {
                propertyMapper = tuple.PropertyMapper;
                index = tuple.Index;
                return true;
            }
            else
            {
                propertyMapper = null;
                index = -1;
                return false;
            }
        }

        private ImmutableDictionary<string, (int Index, PropertyMapper PropertyMapper)> PropertyMappersWithIndex { get; }
    }
}
