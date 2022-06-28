using FakeXrmEasy.Core.Db.Exceptions;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

namespace FakeXrmEasy.Core.Db
{
    /// <summary>
    /// Represents an In-Memory database containing the necessary data and metadata
    /// </summary>
    internal class InMemoryDb
    {
        /// <summary>
        /// A collection of tables indexed by its logical name
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
        /// Returns true if the InMemoryDb contains the table with the specified name
        /// </summary>
        /// <param name="logicalName"></param>
        /// <returns></returns>
        protected internal bool ContainsTable(string logicalName)
        {
            return _tables.ContainsKey(logicalName);
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

            table = new InMemoryTable();
            _tables.Add(logicalName, table);
        }

        protected internal void AddEntityRecord(Entity e)
        {
            if (!ContainsTable(e.LogicalName))
            {
                InMemoryTable table;
                AddTable(e.LogicalName, out table);
            }


        }

        protected internal void ReplaceEntityRecord(Entity e)
        {
            if (!ContainsTable(e.LogicalName))
            {
                InMemoryTable table;
                AddTable(e.LogicalName, out table);
            }
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
    }
}
