﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Blueprint41.Neo4j.Model;
using Blueprint41.Neo4j.Persistence.Void;
using Blueprint41.Neo4j.Schema;

namespace Blueprint41.Core
{
    public abstract class PersistenceProvider
    {
        protected PersistenceProvider()
        {
            NodePersistenceProvider = GetNodePersistenceProvider();
            RelationshipPersistenceProvider = GetRelationshipPersistenceProvider();
            Templates = GetTemplates();
            Translator = GetTranslator();

            convertToStoredType = new Lazy<Dictionary<Type, Conversion?>>(
            delegate ()
            {
                return SupportedTypeMappings.ToDictionary(item => item.ReturnType, item => Conversion.GetConverter(item.ReturnType, item.PersistedType));

            }, true);

            convertFromStoredType = new Lazy<Dictionary<Type, Conversion?>>(
            delegate ()
            {
                return SupportedTypeMappings.ToDictionary(item => item.ReturnType, item => Conversion.GetConverter(item.PersistedType, item.ReturnType));

            }, true);
        }

        internal NodePersistenceProvider NodePersistenceProvider { get; private set; }
        internal RelationshipPersistenceProvider RelationshipPersistenceProvider { get; private set; }

        private protected abstract NodePersistenceProvider GetNodePersistenceProvider();
        private protected abstract RelationshipPersistenceProvider GetRelationshipPersistenceProvider();

        public abstract Transaction NewTransaction(bool withTransaction);

        public abstract List<TypeMapping> SupportedTypeMappings { get; }

        private Lazy<Dictionary<Type, Conversion?>> convertToStoredType;
        internal Dictionary<Type, Conversion?> ConvertToStoredTypeCache { get { return convertToStoredType.Value; } }

        private Lazy<Dictionary<Type, Conversion?>> convertFromStoredType;
        internal Dictionary<Type, Conversion?> ConvertFromStoredTypeCache { get { return convertFromStoredType.Value; } }

        public static PersistenceProvider CurrentPersistenceProvider { get; set; } = new Neo4jPersistenceProvider(null, null, null, false);

        public static bool IsNeo4j
        {
            get
            {
                if (CurrentPersistenceProvider == null)
                    return false;

                return CurrentPersistenceProvider.GetType().IsSubclassOfOrSelf(typeof(Neo4jPersistenceProvider));
            }
        }

        #region Conversion

        public object? ConvertToStoredType<TValue>(TValue value)
        {
            return ConvertToStoredTypeCacheVia<TValue>.Convert(this, value);
        }
        public object? ConvertToStoredType(Type returnType, object? value)
        {
            if (returnType == null)
                return value;

            Conversion? converter;
            if (!ConvertToStoredTypeCache.TryGetValue(returnType, out converter))
                return value;

            if (converter is null)
                return value;

            return converter.Convert(value);
        }
        public TReturnType ConvertFromStoredType<TReturnType>(object? value)
        {
            return (TReturnType)ConvertFromStoredTypeCacheVia<TReturnType>.Convert(this, value)!;
        }
        public object? ConvertFromStoredType(Type returnType, object? value)
        {
            if (returnType == null)
                return value;

            Conversion? converter;
            if (!ConvertFromStoredTypeCache.TryGetValue(returnType, out converter))
                return value;

            if (converter is null)
                return value;

            return converter.Convert(value);
        }

        public Type? GetStoredType<TReturnType>()
        {
            ConvertFromStoredTypeCacheVia<TReturnType>.Initialize(this);
            return ConvertFromStoredTypeCacheVia<TReturnType>.Converter?.FromType ?? typeof(TReturnType);
        }
        public Type? GetStoredType(Type returnType)
        {
            Conversion? converter;
            if (!ConvertFromStoredTypeCache.TryGetValue(returnType, out converter))
                return null;

            return converter?.FromType;
        }

        private class ConvertToStoredTypeCacheVia<TReturnType>
        {
            public static Conversion? Converter = null;
            public static bool IsInitialized = false;

            public static object? Convert(PersistenceProvider factory, object? value)
            {
                Initialize(factory);

                if (Converter == null)
                    return value;

                return Converter.Convert(value);
            }

            internal static void Initialize(PersistenceProvider factory)
            {
                if (!IsInitialized)
                {
                    lock (typeof(ConvertFromStoredTypeCacheVia<TReturnType>))
                    {
                        if (!IsInitialized)
                        {
                            factory.ConvertToStoredTypeCache.TryGetValue(typeof(TReturnType), out Converter);
                            IsInitialized = true;
                        }
                    }
                }
            }
        }
        private class ConvertFromStoredTypeCacheVia<TReturnType>
        {
            public static Conversion? Converter = null;
            public static bool IsInitialized = false;

            public static object? Convert(PersistenceProvider factory, object? value)
            {
                Initialize(factory);

                if (Converter == null)
                    return value;

                return Converter.Convert(value);
            }

            internal static void Initialize(PersistenceProvider factory)
            {
                if (!IsInitialized)
                {
                    lock (typeof(ConvertFromStoredTypeCacheVia<TReturnType>))
                    {
                        if (!IsInitialized)
                        {
                            factory.ConvertFromStoredTypeCache.TryGetValue(typeof(TReturnType), out Converter);
                            IsInitialized = true;
                        }
                    }
                }
            }
        }

        #endregion

        #region Factory: Refactoring Templates

        internal RefactorTemplates Templates { get; private set; }
        protected virtual RefactorTemplates GetTemplates() => new RefactorTemplates();

        #endregion

        #region Factory: Query Translator

        internal QueryTranslator Translator { get; private set; }
        protected virtual QueryTranslator GetTranslator() => new QueryTranslator();

        #endregion

        #region Factory: Schema Info

        internal virtual SchemaInfo GetSchemaInfo(DatastoreModel datastoreModel) => new SchemaInfo(datastoreModel);

        #endregion
    }
}
