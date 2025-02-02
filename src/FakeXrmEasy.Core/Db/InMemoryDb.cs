using System;
using FakeXrmEasy.Core.Db.Exceptions;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace FakeXrmEasy.Core.Db
{
    /// <summary>
    /// Represents an In-Memory database containing the necessary data and metadata
    /// </summary>
    internal class InMemoryDb
    {
        /// <summary>
        /// A collection of tables indexed by their logical name
        /// </summary>
        protected internal Dictionary<string, InMemoryTable> _tables;

        /// <summary>
        /// Default InMemoryDb constructor with an empty list of tables
        /// </summary>
        public InMemoryDb()
        {
            _tables = new Dictionary<string, InMemoryTable>();
        }

        /// <summary>
        /// Returns true if the InMemoryDb contains a table object with the specified name
        /// </summary>
        /// <param name="logicalName"></param>
        /// <returns></returns>
        protected internal bool ContainsTable(string logicalName)
        {
            return _tables.ContainsKey(logicalName);
        }

        /// <summary>
        /// Returns true if the InMemoryDb contains a table object with a non-empty entity metadata
        /// </summary>
        /// <param name="logicalName"></param>
        /// <returns></returns>
        protected internal bool ContainsTableMetadata(string logicalName)
        {
            return _tables.ContainsKey(logicalName) && _tables[logicalName]._metadata._entityMetadata != null;
        }

        /// <summary>
        /// Returns the table with the specified logical name
        /// </summary>
        /// <param name="logicalName"></param>
        /// <returns></returns>
        protected internal InMemoryTable GetTable(string logicalName)
        {
            return _tables[logicalName];
        }

        /// <summary>
        /// Returns the EntityMetadata for the specified logical name
        /// </summary>
        /// <param name="logicalName"></param>
        /// <returns></returns>
        protected internal EntityMetadata GetTableMetadata(string logicalName)
        {
            var entityMetadata = _tables[logicalName]._metadata._entityMetadata;
            if (entityMetadata == null) return null;

            return entityMetadata.Copy();
        }

        /// <summary>
        /// Adds and returns the table that was added. If a table with the same logicalName was added, this will raise an exception
        /// </summary>
        /// <param name="logicalName"></param>
        /// <param name="table"></param>
        protected internal void AddTable(string logicalName, out InMemoryTable table)
        {
            if(_tables.ContainsKey(logicalName))
            {
                throw new TableAlreadyExistsException(logicalName);
            }

            table = new InMemoryTable(logicalName);
            _tables.Add(logicalName, table);
        }

        /// <summary>
        /// Adds the specified entity metadata to this InMemory database
        /// </summary>
        /// <param name="logicalName"></param>
        /// <param name="entityMetadata"></param>
        protected internal void AddOrUpdateMetadata(string logicalName, EntityMetadata entityMetadata)
        {
            InMemoryTable table = null;
            if (!_tables.ContainsKey(logicalName))
            {
                table = new InMemoryTable(logicalName, entityMetadata);
                _tables.Add(logicalName, table);
            }
            else
            {
                _tables[logicalName].SetMetadata(entityMetadata);
            }  
        }

        protected internal void AddEntityRecord(Entity e)
        {
            InMemoryTable table;
            if (!ContainsTable(e.LogicalName))
            {
                AddTable(e.LogicalName, out table);
            }
            else
            {
                table = GetTable(e.LogicalName);
            }
            
            table.Add(e);
        }

        protected internal bool ContainsEntityRecord(string logicalName, Guid id)
        {
            if (!ContainsTable(logicalName))
            {
                return false;
            }

            var table = GetTable(logicalName);
            return table.Contains(id);
        }
        
        protected internal void AddOrReplaceEntityRecord(Entity e)
        {
            InMemoryTable table = null;
            if (!ContainsTable(e.LogicalName))
            {
                AddTable(e.LogicalName, out table);
            }

            table = _tables[e.LogicalName];

            if (table.Contains(e))
            {
                table.Replace(e);
            }
            else
            {
                table.Add(e);
            }
        }

        protected internal IEnumerable<EntityMetadata> AllMetadata
        {
            get {
                return _tables
                    .Where(t => t.Value._metadata._entityMetadata != null)
                    .Select(t => t.Value._metadata._entityMetadata.Copy())
                    .AsEnumerable();
            }
        }
    }
}
