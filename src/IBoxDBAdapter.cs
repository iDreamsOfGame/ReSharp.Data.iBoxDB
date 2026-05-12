// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using IBoxDB.LocalServer;

namespace ReSharp.Data.IBoxDB
{
    /// <summary>
    /// An IBoxDBAdapter instance represents a session with the iBoxDB and can be used to query and save instances of your entities. Implements the
    /// <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class IBoxDBAdapter : IDisposable
    {
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="IBoxDBAdapter" /> class with cache mode.
        /// </summary>
        public IBoxDBAdapter()
        {
            Database = new DB(DB.CacheOnlyArg);
            Database.MinConfig().FileIncSize = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IBoxDBAdapter" /> class with a database folder path.
        /// </summary>
        /// <param name="dbFolderPath">The database folder path.</param>
        public IBoxDBAdapter(string dbFolderPath)
        {
            Database = new DB(dbFolderPath);
            Database.MinConfig().FileIncSize = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IBoxDBAdapter" /> class with a database folder path and the raw data of database.
        /// </summary>
        /// <param name="dbFolderPath">The database folder path.</param>
        /// <param name="dbFileRawData">The database file raw data.</param>
        public IBoxDBAdapter(string dbFolderPath, byte[] dbFileRawData)
        {
            DB.Root(dbFolderPath);
            Database = new DB(dbFileRawData);
            Database.MinConfig().FileIncSize = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IBoxDBAdapter" /> class.
        /// </summary>
        /// <param name="dbFileRawData">The database file raw data.</param>
        public IBoxDBAdapter(byte[] dbFileRawData)
        {
            Database = new DB(dbFileRawData);
            Database.MinConfig().FileIncSize = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IBoxDBAdapter" /> class.
        /// </summary>
        /// <param name="dbFolderPath">The database folder path.</param>
        /// <param name="localAddress">The local address.</param>
        public IBoxDBAdapter(string dbFolderPath, long localAddress)
        {
            Database = new DB(localAddress, dbFolderPath);
            Database.MinConfig().FileIncSize = 1;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="IBoxDBAdapter" /> class.
        /// </summary>
        ~IBoxDBAdapter()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets a value indicating whether the database connection is open.
        /// </summary>
        /// <value><c>true</c> if the database connection is open; otherwise, <c>false</c>.</value>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Gets the database object of iBoxDB.
        /// </summary>
        public DB Database { get; private set; }

        /// <summary>
        /// Gets the box object of iBoxDB database.
        /// </summary>
        public DB.AutoBox Box { get; private set; }

        /// <summary>
        /// Ensures that the table exists in the database configuration with a single key.
        /// </summary>
        /// <typeparam name="T">The type of the table entity.</typeparam>
        /// <param name="key">The primary key field name for the table.</param>
        /// <returns>The database configuration object.</returns>
        public DatabaseConfig EnsureTable<T>(string key) where T : class => EnsureTable<T>(typeof(T).Name, key);

        /// <summary>
        /// Ensures that the table exists in the database configuration with a single key.
        /// </summary>
        /// <typeparam name="T">The type of the table entity.</typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="key">The primary key field name for the table.</param>
        /// <returns>The database configuration object.</returns>
        public DatabaseConfig EnsureTable<T>(string tableName, string key) where T : class
        {
            return Database.GetConfig().EnsureTable<T>(tableName, key);
        }
        
        /// <summary>
        /// Ensures that the table exists in the database configuration with multiple keys.
        /// </summary>
        /// <typeparam name="T">The type of the table entity.</typeparam>
        /// <param name="keys">An array of primary key field names for the table.</param>
        /// <returns>The database configuration object.</returns>
        public DatabaseConfig EnsureTable<T>(string[] keys) where T : class => EnsureTable<T>(typeof(T).Name, keys);

        /// <summary>
        /// Ensures that the table exists in the database configuration with a table name and multiple keys.
        /// </summary>
        /// <typeparam name="T">The type of the table entity.</typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="keys">An array of primary key field names for the table.</param>
        /// <returns>The database configuration object.</returns>
        public DatabaseConfig EnsureTable<T>(string tableName, string[] keys) where T : class
        {
            return Database.GetConfig().EnsureTable<T>(tableName, keys);
        }

        /// <summary>
        /// Ensures that an index exists on the specified field in the database configuration.
        /// </summary>
        /// <typeparam name="T">The type of the table entity.</typeparam>
        /// <param name="key">The field name to create an index on.</param>
        /// <returns>The database configuration object.</returns>
        public DatabaseConfig EnsureIndex<T>(string key) where T : class => EnsureIndex<T>(typeof(T).Name, key);

        /// <summary>
        /// Ensures that an index exists on the specified field in the database configuration.
        /// </summary>
        /// <typeparam name="T">The type of the table entity.</typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="key">The field name to create an index on.</param>
        /// <returns>The database configuration object.</returns>
        public DatabaseConfig EnsureIndex<T>(string tableName, string key) where T : class
        {
            return Database.GetConfig().EnsureIndex<T>(tableName, key);
        }
        
        /// <summary>
        /// Ensures that indexes exist on the specified fields in the database configuration.
        /// </summary>
        /// <typeparam name="T">The type of the table entity.</typeparam>
        /// <param name="keys">An array of field names to create indexes on.</param>
        /// <returns>The database configuration object.</returns>
        public DatabaseConfig EnsureIndex<T>(string[] keys) where T : class => EnsureIndex<T>(typeof(T).Name, keys);

        /// <summary>
        /// Ensures that indexes exist on the specified fields for a table in the database configuration.
        /// </summary>
        /// <typeparam name="T">The type of the table entity.</typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="keys">An array of field names to create indexes on.</param>
        /// <returns>The database configuration object.</returns>
        public DatabaseConfig EnsureIndex<T>(string tableName, string[] keys) where T : class
        {
            return Database.GetConfig().EnsureIndex<T>(tableName, keys);
        }

        /// <summary>
        /// Opens the database.
        /// </summary>
        /// <exception cref="ObjectDisposedException">database</exception>
        public void Open()
        {
            CheckIfDisposed();

            if (Database == null)
                return;

            Box = Database.Open();
            IsOpen = true;
        }

        /// <summary>
        /// Gets the buffer.
        /// </summary>
        /// <returns>System.Byte[].</returns>
        public byte[] GetBuffer() => Database.GetBuffer();

        /// <summary>
        /// Counts the number of records that match the specified query with arguments.
        /// </summary>
        /// <typeparam name="T">The type of table data.</typeparam>
        /// <param name="query">The query condition without 'where' clause.</param>
        /// <param name="arguments">The query arguments.</param>
        /// <returns>The count of matching records.</returns>
        public long CountByQuery<T>(string query, params object[] arguments) where T : class, new() => CountByQuery(typeof(T).Name, query, arguments);
        
        /// <summary>
        /// Counts the number of records that match the specified query with arguments.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="query">The query condition without 'where' clause.</param>
        /// <param name="arguments">The query arguments.</param>
        /// <returns>The count of matching records.</returns>
        public long CountByQuery(string tableName, string query, params object[] arguments)
        {
            CheckIfDisposed();
            CheckIfDatabaseIsOpen();

            var builder = new StringBuilder();
            builder.AppendFormat("from {0} where ", tableName);
            builder.Append(query);
            return Box.Count(builder.ToString(), arguments);
        }
        
        /// <summary>
        /// Counts the number of records that match the specified dictionary-based query conditions.
        /// </summary>
        /// <typeparam name="T">The type of the table entity.</typeparam>
        /// <param name="arguments">A dictionary containing query field names and their corresponding values.</param>
        /// <param name="logicalOperator">The logical operator to use between conditions (And, Or, or None).</param>
        /// <returns>The count of matching records.</returns>
        public long CountByQuery<T>(Dictionary<string, object> arguments, QueryLogicalOperator logicalOperator = QueryLogicalOperator.None)
        {
            return CountByQuery(typeof(T).Name, arguments, logicalOperator);
        }
        
        /// <summary>
        /// Counts the number of records that match the specified dictionary-based query conditions.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="arguments">A dictionary containing query field names and their corresponding values.</param>
        /// <param name="logicalOperator">The logical operator to use between conditions (And, Or, or None).</param>
        /// <returns>The count of matching records.</returns>
        public long CountByQuery(string tableName,
            Dictionary<string, object> arguments,
            QueryLogicalOperator logicalOperator = QueryLogicalOperator.None)
        {
            CheckIfDisposed();
            CheckIfDatabaseIsOpen();

            var builder = new StringBuilder();
            builder.AppendFormat("from {0} where ", tableName);

            var i = 0;
            foreach (var key in arguments.Keys)
            {
                builder.AppendFormat("{0} == ?", key);
                i++;

                if (i < arguments.Keys.Count)
                {
                    switch (logicalOperator)
                    {
                        case QueryLogicalOperator.And:
                        default:
                            builder.Append(" & ");
                            break;

                        case QueryLogicalOperator.Or:
                            builder.Append(" | ");
                            break;
                    }
                }
            }

            var args = new object[arguments.Count];
            arguments.Values.CopyTo(args, 0);
            return Box.Count(builder.ToString(), args);
        }
        
        /// <summary>
        /// Counts the number of records that match the specified key-value condition.
        /// </summary>
        /// <typeparam name="T">The type of table data.</typeparam>
        /// <param name="key">The key to match.</param>
        /// <param name="value">The value to match.</param>
        /// <returns>The count of matching records.</returns>
        public long Count<T>(string key, object value) where T : class, new() => Count(typeof(T).Name, key, value);
        
        /// <summary>
        /// Counts the number of records that match the specified key-value condition.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="key">The key to match.</param>
        /// <param name="value">The value to match.</param>
        /// <returns>The count of matching records.</returns>
        public long Count(string tableName, string key, object value)
        {
            CheckIfDisposed();
            CheckIfDatabaseIsOpen();
            return Box.Count($"from {tableName} where {key} == ?", value);
        }

        /// <summary>
        /// Counts all records in the table.
        /// </summary>
        /// <typeparam name="T">The type of table data.</typeparam>
        /// <returns>The total count of records in the table.</returns>
        public long Count<T>() where T : class, new() => Count(typeof(T).Name);

        /// <summary>
        /// Counts all records in the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>The total count of records in the table.</returns>
        public long Count(string tableName)
        {
            CheckIfDisposed();
            CheckIfDatabaseIsOpen();

            return Box.Count($"from {tableName}");
        }
        
        /// <summary>
        /// Gets the specified object with the <c>keys</c>.
        /// </summary>
        /// <typeparam name="T">Specifies the object type of return.</typeparam>
        /// <param name="keys">The keys to locate the specified object.</param>
        /// <returns>The object that was found.</returns>
        public T Get<T>(params object[] keys) where T : class, new() => Get<T>(typeof(T).Name, keys);

        /// <summary>
        /// Gets the specified object with the <c>tableName</c> and the <c>keys</c>.
        /// </summary>
        /// <typeparam name="T">Specifies the object type of return.</typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="keys">The keys to locate the specified object.</param>
        /// <returns>The object that was found.</returns>
        public T Get<T>(string tableName, params object[] keys) where T : class, new()
        {
            CheckIfDisposed();
            CheckIfDatabaseIsOpen();
            return Box.Get<T>(tableName, keys);
        }

        /// <summary>
        /// Gets all data in the table.
        /// </summary>
        /// <typeparam name="T">The type of the data in database.</typeparam>
        /// <returns>The data list found in database.</returns>
        public List<T> GetAll<T>() where T : class, new() => GetAll<T>(typeof(T).Name);

        /// <summary>
        /// Gets all data in the table.
        /// </summary>
        /// <typeparam name="T">The type of the data in database.</typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>The data list found in database.</returns>
        public List<T> GetAll<T>(string tableName) where T : class, new()
        {
            CheckIfDisposed();
            CheckIfDatabaseIsOpen();
            return Box.Select<T>($"from {tableName}");
        }
        
        /// <summary>
        /// Finds the specified objects with the <c>value</c> of <c>key</c>.
        /// </summary>
        /// <typeparam name="T">Specifies the object type of return.</typeparam>
        /// <param name="key">The key to locate the specified object.</param>
        /// <param name="value">The value of key to locate the specified object.</param>
        /// <returns>The objects that was found.</returns>
        public List<T> Find<T>(string key, object value) where T : class, new() => Find<T>(typeof(T).Name, key, value);

        /// <summary>
        /// Finds the specified objects with the <c>tableName</c> and the <c>value</c> of <c>key</c>.
        /// </summary>
        /// <typeparam name="T">Specifies the object type of return.</typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="key">The key to locate the specified object.</param>
        /// <param name="value">The value of key to locate the specified object.</param>
        /// <returns>The objects that was found.</returns>
        public List<T> Find<T>(string tableName, string key, object value) where T : class, new()
        {
            CheckIfDisposed();
            CheckIfDatabaseIsOpen();
            return Box.Select<T>($"from {tableName} where {key} == ?", value);
        }
        
        /// <summary>
        /// Queries data with string of specific query language.
        /// </summary>
        /// <typeparam name="T">The type definition of query data.</typeparam>
        /// <param name="query">The string of specific query language.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>A <see cref="List{T}" /> that contains query results.</returns>
        public List<T> Query<T>(string query, params object[] arguments) where T : class, new() => Query<T>(typeof(T).Name, query, arguments);

        /// <summary>
        /// Queries data with string of specific query language.
        /// </summary>
        /// <typeparam name="T">The type definition of query data.</typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="query">The string of specific query language.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>A <see cref="List{T}" /> that contains query results.</returns>
        public List<T> Query<T>(string tableName, string query, params object[] arguments) where T : class, new()
        {
            CheckIfDisposed();
            CheckIfDatabaseIsOpen();

            var builder = new StringBuilder();
            builder.AppendFormat("from {0} where ", tableName);
            builder.Append(query);
            return Box.Select<T>(builder.ToString(), arguments);
        }
        
        /// <summary>
        /// Queries the specified objects with multi-conditions.
        /// </summary>
        /// <typeparam name="T">Specifies the object type of return.</typeparam>
        /// <param name="arguments">The arguments.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        /// <returns>The objects that was found.</returns>
        public List<T> Query<T>(Dictionary<string, object> arguments, QueryLogicalOperator logicalOperator = QueryLogicalOperator.None) where T : class, new()
        {
            return Query<T>(typeof(T).Name, arguments, logicalOperator);
        }

        /// <summary>
        /// Queries the specified objects with the <c>tableName</c> and multi-conditions.
        /// </summary>
        /// <typeparam name="T">Specifies the object type of return.</typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        /// <returns>The objects that was found.</returns>
        public List<T> Query<T>(string tableName,
            Dictionary<string, object> arguments,
            QueryLogicalOperator logicalOperator = QueryLogicalOperator.None) where T : class, new()
        {
            CheckIfDisposed();
            CheckIfDatabaseIsOpen();

            var builder = new StringBuilder();
            builder.AppendFormat("from {0} where ", tableName);

            var i = 0;
            foreach (var key in arguments.Keys)
            {
                builder.AppendFormat("{0} == ?", key);
                i++;

                if (i < arguments.Keys.Count)
                {
                    switch (logicalOperator)
                    {
                        case QueryLogicalOperator.And:
                        default:
                            builder.Append(" & ");
                            break;

                        case QueryLogicalOperator.Or:
                            builder.Append(" | ");
                            break;
                    }
                }
            }

            var args = new object[arguments.Count];
            arguments.Values.CopyTo(args, 0);
            return Box.Select<T>(builder.ToString(), args);
        }

        /// <summary>
        /// Inserts data into database.
        /// </summary>
        /// <typeparam name="T">The type of table data.</typeparam>
        /// <param name="value">The data need to be inserted.</param>
        /// <returns><c>true</c> if data insert success, <c>false</c> otherwise.</returns>
        public bool Insert<T>(T value) where T : class => Insert(typeof(T).Name, value);

        /// <summary>
        /// Inserts data into database.
        /// </summary>
        /// <typeparam name="T">The type of table data.</typeparam>
        /// <param name="tableName">The name of table.</param>
        /// <param name="value">The data need to be inserted.</param>
        /// <returns><c>true</c> if data insert success, <c>false</c> otherwise.</returns>
        public bool Insert<T>(string tableName, T value) where T : class
        {
            CheckIfDisposed();
            CheckIfDatabaseIsOpen();
            return Box.Insert(tableName, value);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Box?.Dispose();
                Box = null;

                Database?.Dispose();
                Database = null;

                IsOpen = false;
            }

            disposed = true;
        }

        private void CheckIfDatabaseIsOpen()
        {
            if (Box == null)
                throw new NullReferenceException("Database is not open! Before executing database operation, you should invoke method 'Open'.");
        }

        private void CheckIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(Database));
        }
    }
}