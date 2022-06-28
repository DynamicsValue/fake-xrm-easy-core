using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace FakeXrmEasy.Core.Db
{
    /// <summary>
    /// A table object is a single entry point for anything related to that table (schema, entity records, etc)
    /// </summary>
    internal class InMemoryTable
    {
        /// <summary>
        /// The entity logical name for this table
        /// </summary>
        protected internal string _logicalName;

        /// <summary>
        /// Collection of entity records for this table
        /// </summary>
        protected internal Dictionary<Guid, Entity> _rows;

        /// <summary>
        /// Default constructor with no metadata and an empty records table
        /// </summary>
        public InMemoryTable()
        {
            _rows = new Dictionary<Guid, Entity>();
        }

        /// <summary>
        /// Return true if the current table already contains this record
        /// </summary>
        /// <param name="e">The entity record to check</param>
        /// <returns></returns>
        protected internal bool Contains(Entity e)
        {
            return _rows.ContainsKey(e.Id);
        }

        /// <summary>
        /// Returns true if the current table contains a record with the specified id
        /// </summary>
        /// <param name="key">The primary key of the entity record</param>
        /// <returns></returns>
        protected internal bool Contains(Guid key)
        {
            return _rows.ContainsKey(key);
        }

        /// <summary>
        /// Adds the entity record to the current table
        /// </summary>
        /// <param name="e">The entity record to add</param>
        protected internal void Add(Entity e)
        {
            _rows.Add(e.Id, e);
        }

        /// <summary>
        /// Replaces the current entity record with the given id 
        /// </summary>
        /// <param name="e"></param>
        protected internal void Replace(Entity e)
        {
            _rows[e.Id] = e;
        }


        /// <summary>
        /// Remove the entity record by primary key
        /// </summary>
        /// <param name="key">The primary key</param>
        protected internal void Remove(Guid key)
        {
            _rows.Remove(key);
        }

        /// <summary>
        /// Returns a record by its primary key
        /// </summary>
        /// <param name="key">The primary key</param>
        /// <returns></returns>
        protected internal Entity GetById(Guid key)
        {
            return _rows[key];
        }

        /// <summary>
        /// Returns an IEnumerable of all rows in the current table
        /// </summary>
        protected internal IEnumerable<Entity> Rows
        {
            get
            {
                return _rows.Values;
            }
        }
    }
}
