﻿using Blueprint41.Gremlin.Cosmos;
using Blueprint41.Neo4j.Persistence;
using Blueprint41.Neo4j.Refactoring.Templates;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Blueprint41
{
    /// <summary>
    /// This is a class wherein what type of graph features is supports. Defaults to true
    /// </summary>
    public class GraphFeatures
    {
        private GraphFeatures() { }

        public static readonly GraphFeatures Neo4j = new GraphFeatures()
        {
            PersistenceProviderType = typeof(Neo4JPersistenceProvider),

        }.SetTemplateFeatures();

        public static readonly GraphFeatures Cosmos = new GraphFeatures()
        {
            Cypher = false,
            FunctionalId = false,
            CreateIndex = false,
            CreateUniqueConstraint = false,
            DropExistConstraints = false,
            PersistenceProviderType = typeof(GremlinPersistenceProvider),

        }.SetTemplateFeatures();

        private Dictionary<Type, bool> templateFeatures;
        GraphFeatures SetTemplateFeatures()
        {
            templateFeatures = new Dictionary<Type, bool>();

            templateFeatures.Add(typeof(ApplyFunctionalId), FunctionalId);
            templateFeatures.Add(typeof(Neo4j.Refactoring.Templates.Convert), Convert);
            templateFeatures.Add(typeof(CopyProperty), CopyProperty);
            templateFeatures.Add(typeof(CreateIndex), CreateIndex);
            templateFeatures.Add(typeof(CreateUniqueConstraint), CreateUniqueConstraint);
            templateFeatures.Add(typeof(DropExistConstraint), DropExistConstraints);
            templateFeatures.Add(typeof(MergeProperty), MergeProperty);
            templateFeatures.Add(typeof(MergeRelationship), MergeRelationship);
            templateFeatures.Add(typeof(RemoveEntity), RemoveEntity);
            templateFeatures.Add(typeof(RemoveProperty), RemoveProperty);
            templateFeatures.Add(typeof(RemoveRelationship), RemoveRelationship);
            templateFeatures.Add(typeof(RenameEntity), RenameEntity);
            templateFeatures.Add(typeof(RenameProperty), RenameProperty);
            templateFeatures.Add(typeof(RenameRelationship), RenameRelationship);
            templateFeatures.Add(typeof(SetDefaultConstantValue), SetDefaultConstantValue);
            templateFeatures.Add(typeof(SetDefaultLookupValue), SetDefaultLookupValue);
            templateFeatures.Add(typeof(SetLabel), SetLabel);
            templateFeatures.Add(typeof(SetRelationshipPropertyValue), SetRelationshipPropertyValue);

            return this;
        }

        public bool Cypher { get; private set; } = true;
        public bool FunctionalId { get; private set; } = true;
        public bool Convert { get; private set; } = true;
        public bool CopyProperty { get; private set; } = true;
        public bool CreateIndex { get; private set; } = true;
        public bool CreateUniqueConstraint { get; private set; } = true;
        public bool DropExistConstraints { get; private set; } = true;
        public bool MergeProperty { get; private set; } = true;
        public bool MergeRelationship { get; private set; } = true;
        public bool RemoveEntity { get; private set; } = true;
        public bool RemoveProperty { get; private set; } = true;
        public bool RemoveRelationship { get; private set; } = true;
        public bool RenameEntity { get; private set; } = true;
        public bool RenameProperty { get; private set; } = true;
        public bool RenameRelationship { get; private set; } = true;
        public bool SetDefaultConstantValue { get; private set; } = true;
        public bool SetDefaultLookupValue { get; private set; } = true;
        public bool SetLabel { get; private set; } = true;
        public bool SetRelationshipPropertyValue { get; private set; } = true;

        internal Type PersistenceProviderType { get; private set; }

        internal bool SupportsTemplate<T>(T template)
            where T : TemplateBase
        {
            Type templateType = typeof(T);

            if (templateFeatures.ContainsKey(templateType))
                return templateFeatures[templateType];

            // Default: all template features are supported
            return true;
        }
    }
}
