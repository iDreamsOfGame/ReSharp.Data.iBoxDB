// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ReSharp.Data.IBoxDB.Tests
{
    /// <summary>
    /// Test entity for database operations
    /// </summary>
    public class TestEntity
    {
        public long Id { get; init; }
        public string Name { get; init; }
        public int Age { get; init; }
        public string Email { get; init; }
    }

    /// <summary>
    /// Another test entity for multi-key testing
    /// </summary>
    public class MultiKeyEntity
    {
        public string Key1 { get; init; }
        public string Key2 { get; init; }
        public string Value { get; init; }
    }

    [TestFixture]
    [Description("Unit tests for IBoxDBAdapter class")]
    public class IBoxDBAdapterTests
    {
        private IBoxDBAdapter adapter;
        private const string TestDbPath = "/tmp/test_iboxdb";

        [SetUp]
        public void SetUp()
        {
            adapter = new IBoxDBAdapter(TestDbPath);
            adapter.EnsureTable<TestEntity>("Id");
            adapter.EnsureTable<TestEntity>("CustomTable", "Id");
            adapter.EnsureTable<MultiKeyEntity>(["Key1", "Key2"]);
            adapter.EnsureIndex<TestEntity>("Name");
            adapter.Open();
        }

        [TearDown]
        public void TearDown()
        {
            adapter?.Dispose();
            
            // Clean up test database files
            if (System.IO.Directory.Exists(TestDbPath))
            {
                try
                {
                    System.IO.Directory.Delete(TestDbPath, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        #region Constructor Tests

        [Test]
        [Description("Test that cache-only constructor creates adapter successfully")]
        public void Constructor_CacheOnlyMode_CreatesAdapterSuccessfully()
        {
            // Arrange & Act
            var cacheAdapter = new IBoxDBAdapter();
            cacheAdapter.EnsureTable<TestEntity>("Id");
            cacheAdapter.Open();

            // Assert
            Assert.That(cacheAdapter, Is.Not.Null);
            Assert.That(cacheAdapter.Database, Is.Not.Null);
            Assert.That(cacheAdapter.IsOpen, Is.True);

            // Cleanup
            cacheAdapter.Dispose();
        }

        [Test]
        [Description("Test that folder path constructor creates adapter successfully")]
        public void Constructor_WithFolderPath_CreatesAdapterSuccessfully()
        {
            // Arrange & Act
            var folderAdapter = new IBoxDBAdapter("/tmp/test_iboxdb_constructor");

            // Assert
            Assert.That(folderAdapter, Is.Not.Null);
            Assert.That(folderAdapter.Database, Is.Not.Null);
            Assert.That(folderAdapter.IsOpen, Is.False);

            // Cleanup
            folderAdapter.Dispose();
            if (System.IO.Directory.Exists("/tmp/test_iboxdb_constructor"))
            {
                System.IO.Directory.Delete("/tmp/test_iboxdb_constructor", true);
            }
        }

        [Test]
        [Description("Test that local address constructor creates adapter successfully")]
        public void Constructor_WithLocalAddress_CreatesAdapterSuccessfully()
        {
            // Arrange & Act
            var addressAdapter = new IBoxDBAdapter("/tmp/test_iboxdb_address", 1L);
            addressAdapter.EnsureTable<TestEntity>("Id");
            addressAdapter.Open();

            // Assert
            Assert.That(addressAdapter, Is.Not.Null);
            Assert.That(addressAdapter.Database, Is.Not.Null);
            Assert.That(addressAdapter.IsOpen, Is.True);

            // Cleanup
            addressAdapter.Dispose();
            if (System.IO.Directory.Exists("/tmp/test_iboxdb_address"))
            {
                System.IO.Directory.Delete("/tmp/test_iboxdb_address", true);
            }
        }

        #endregion

        #region Property Tests

        [Test]
        [Description("Test that IsOpen returns false before calling Open method")]
        public void IsOpen_BeforeOpen_ReturnsFalse()
        {
            // Arrange
            var newAdapter = new IBoxDBAdapter("/tmp/test_iboxdb_isopen");
            newAdapter.EnsureTable<TestEntity>("Id");

            // Assert
            Assert.That(newAdapter.IsOpen, Is.False);

            // Cleanup
            newAdapter.Dispose();
            if (System.IO.Directory.Exists("/tmp/test_iboxdb_isopen"))
            {
                System.IO.Directory.Delete("/tmp/test_iboxdb_isopen", true);
            }
        }

        [Test]
        [Description("Test that IsOpen returns true after calling Open method")]
        public void IsOpen_AfterOpen_ReturnsTrue()
        {
            // Assert
            Assert.That(adapter.IsOpen, Is.True);
        }

        [Test]
        [Description("Test that Database property is not null after initialization")]
        public void Database_AfterInitialization_IsNotNull()
        {
            // Assert
            Assert.That(adapter.Database, Is.Not.Null);
        }

        [Test]
        [Description("Test that Box property is not null after calling Open")]
        public void Box_AfterOpen_IsNotNull()
        {
            // Assert
            Assert.That(adapter.Box, Is.Not.Null);
        }

        #endregion

        #region EnsureTable Tests

        [Test]
        [Description("Test that EnsureTable with multiple keys creates table successfully")]
        public void EnsureTable_WithMultipleKeys_CreatesTableSuccessfully()
        {
            // Arrange & Act
            var config = adapter.EnsureTable<MultiKeyEntity>("MultiTable", ["Key1", "Key2"]);

            // Assert
            Assert.That(config, Is.Not.Null);
        }

        #endregion

        #region EnsureIndex Tests

        [Test]
        [Description("Test that EnsureIndex with type name creates index successfully")]
        public void EnsureIndex_WithTypeName_CreatesIndexSuccessfully()
        {
            // Arrange & Act
            var config = adapter.EnsureIndex<TestEntity>("CustomTable", "Email");

            // Assert
            Assert.That(config, Is.Not.Null);
        }

        [Test]
        [Description("Test that EnsureIndex with multiple keys creates indexes successfully")]
        public void EnsureIndex_WithMultipleKeys_CreatesIndexesSuccessfully()
        {
            // Arrange & Act
            var config = adapter.EnsureIndex<TestEntity>("CustomTable", ["Name", "Email"]);

            // Assert
            Assert.That(config, Is.Not.Null);
        }

        #endregion

        #region Insert Tests

        [Test]
        [Description("Test that Insert adds a single record successfully")]
        public void Insert_SingleRecord_ReturnsTrue()
        {
            // Arrange
            var entity = new TestEntity { Id = 1, Name = "John Doe", Age = 30, Email = "john@example.com" };

            // Act
            var result = adapter.Insert(entity);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(adapter.Count<TestEntity>(), Is.EqualTo(1));
        }

        [Test]
        [Description("Test that Insert with table name adds a single record successfully")]
        public void Insert_WithTableName_ReturnsTrue()
        {
            // Arrange
            var entity = new TestEntity { Id = 2, Name = "Jane Doe", Age = 25, Email = "jane@example.com" };

            // Act
            var result = adapter.Insert("TestEntity", entity);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(adapter.Count<TestEntity>(), Is.EqualTo(1));
        }

        [Test]
        [Description("Test that Insert multiple records increases count correctly")]
        public void Insert_MultipleRecords_IncreasesCountCorrectly()
        {
            // Arrange
            var entity1 = new TestEntity { Id = 1, Name = "John", Age = 30 };
            var entity2 = new TestEntity { Id = 2, Name = "Jane", Age = 25 };
            var entity3 = new TestEntity { Id = 3, Name = "Bob", Age = 35 };

            // Act
            adapter.Insert(entity1);
            adapter.Insert(entity2);
            adapter.Insert(entity3);

            // Assert
            Assert.That(adapter.Count<TestEntity>(), Is.EqualTo(3));
        }

        #endregion

        #region Get Tests

        [Test]
        [Description("Test that Get retrieves a single record by primary key")]
        public void Get_ByPrimaryKey_ReturnsCorrectRecord()
        {
            // Arrange
            var entity = new TestEntity { Id = 1, Name = "John Doe", Age = 30, Email = "john@example.com" };
            adapter.Insert(entity);

            // Act
            var result = adapter.Get<TestEntity>(1L);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("John Doe"));
            Assert.That(result.Age, Is.EqualTo(30));
        }

        [Test]
        [Description("Test that Get with table name retrieves a single record by primary key")]
        public void Get_WithTableName_ByPrimaryKey_ReturnsCorrectRecord()
        {
            // Arrange
            var entity = new TestEntity { Id = 2, Name = "Jane Doe", Age = 25, Email = "jane@example.com" };
            adapter.Insert(entity);

            // Act
            var result = adapter.Get<TestEntity>("TestEntity", 2L);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(2));
            Assert.That(result.Name, Is.EqualTo("Jane Doe"));
        }

        [Test]
        [Description("Test that Get returns null when record does not exist")]
        public void Get_NonExistentRecord_ReturnsNull()
        {
            // Act
            var result = adapter.Get<TestEntity>(999);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [Description("Test that Get retrieves record with multi-key primary key")]
        public void Get_WithMultiKey_ReturnsCorrectRecord()
        {
            // Arrange
            var entity = new MultiKeyEntity { Key1 = "A", Key2 = "B", Value = "Test Value" };
            adapter.Insert("MultiKeyEntity", entity);

            // Act
            var result = adapter.Get<MultiKeyEntity>("MultiKeyEntity", "A", "B");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Key1, Is.EqualTo("A"));
            Assert.That(result.Key2, Is.EqualTo("B"));
            Assert.That(result.Value, Is.EqualTo("Test Value"));
        }

        #endregion

        #region GetAll Tests

        [Test]
        [Description("Test that GetAll retrieves all records from table")]
        public void GetAll_ReturnsAllRecords()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30 });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 25 });
            adapter.Insert(new TestEntity { Id = 3, Name = "Bob", Age = 35 });

            // Act
            var results = adapter.GetAll<TestEntity>();

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(3));
        }

        [Test]
        [Description("Test that GetAll with table name retrieves all records from table")]
        public void GetAll_WithTableName_ReturnsAllRecords()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30 });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 25 });

            // Act
            var results = adapter.GetAll<TestEntity>("TestEntity");

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(2));
        }

        [Test]
        [Description("Test that GetAll returns empty list when table has no records")]
        public void GetAll_EmptyTable_ReturnsEmptyList()
        {
            // Act
            var results = adapter.GetAll<TestEntity>();

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(0));
        }

        #endregion

        #region Find Tests

        [Test]
        [Description("Test that Find retrieves records matching a key-value condition")]
        public void Find_ByKeyValue_ReturnsMatchingRecords()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30, Email = "john@example.com" });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 25, Email = "jane@example.com" });
            adapter.Insert(new TestEntity { Id = 3, Name = "John Smith", Age = 35, Email = "smith@example.com" });

            // Act
            var results = adapter.Find<TestEntity>("Name", "John");

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Name, Is.EqualTo("John"));
        }

        [Test]
        [Description("Test that Find with table name retrieves records matching a key-value condition")]
        public void Find_WithTableName_ByKeyValue_ReturnsMatchingRecords()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30 });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 25 });

            // Act
            var results = adapter.Find<TestEntity>("TestEntity", "Name", "Jane");

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Name, Is.EqualTo("Jane"));
        }

        [Test]
        [Description("Test that Find returns empty list when no records match")]
        public void Find_NoMatch_ReturnsEmptyList()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30 });

            // Act
            var results = adapter.Find<TestEntity>("Name", "NonExistent");

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(0));
        }

        #endregion

        #region Query Tests

        [Test]
        [Description("Test that Query with query string retrieves matching records")]
        public void Query_WithString_ReturnsMatchingRecords()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30, Email = "john@example.com" });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 25, Email = "jane@example.com" });
            adapter.Insert(new TestEntity { Id = 3, Name = "Bob", Age = 35, Email = "bob@example.com" });

            // Act
            var results = adapter.Query<TestEntity>("Age > ? & Age < ?", 26, 34);

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Name, Is.EqualTo("John"));
        }

        [Test]
        [Description("Test that Query with table name and query string retrieves matching records")]
        public void Query_WithTableName_AndString_ReturnsMatchingRecords()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30 });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 25 });
            adapter.Insert(new TestEntity { Id = 3, Name = "Bob", Age = 35 });

            // Act
            var results = adapter.Query<TestEntity>("TestEntity", "Age > ?", 28);

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(2));
        }

        [Test]
        [Description("Test that Query with dictionary and AND operator retrieves matching records")]
        public void Query_WithDictionary_AndOperator_ReturnsMatchingRecords()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30, Email = "john@example.com" });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 25, Email = "jane@example.com" });
            adapter.Insert(new TestEntity { Id = 3, Name = "John Smith", Age = 30, Email = "smith@example.com" });

            var conditions = new Dictionary<string, object>
            {
                { "Name", "John" },
                { "Age", 30 }
            };

            // Act
            var results = adapter.Query<TestEntity>(conditions, QueryLogicalOperator.And);

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Name, Is.EqualTo("John"));
            Assert.That(results[0].Age, Is.EqualTo(30));
        }

        [Test]
        [Description("Test that Query with dictionary and OR operator retrieves matching records")]
        public void Query_WithDictionary_OrOperator_ReturnsMatchingRecords()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30 });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 25 });
            adapter.Insert(new TestEntity { Id = 3, Name = "Bob", Age = 35 });

            var conditions = new Dictionary<string, object>
            {
                { "Name", "John" },
                { "Age", 25 }
            };

            // Act
            var results = adapter.Query<TestEntity>(conditions, QueryLogicalOperator.Or);

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(2));
        }

        [Test]
        [Description("Test that Query with dictionary uses AND as default operator")]
        public void Query_WithDictionary_DefaultOperator_IsAnd()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30 });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 25 });

            var conditions = new Dictionary<string, object>
            {
                { "Name", "John" },
                { "Age", 30 }
            };

            // Act
            var results = adapter.Query<TestEntity>(conditions);

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
        }

        [Test]
        [Description("Test that Query with table name and dictionary retrieves matching records")]
        public void Query_WithTableName_AndDictionary_ReturnsMatchingRecords()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30 });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 25 });

            var conditions = new Dictionary<string, object>
            {
                { "Name", "Jane" }
            };

            // Act
            var results = adapter.Query<TestEntity>("TestEntity", conditions);

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Name, Is.EqualTo("Jane"));
        }

        [Test]
        [Description("Test that Query returns empty list when no records match")]
        public void Query_NoMatch_ReturnsEmptyList()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30 });

            // Act
            var results = adapter.Query<TestEntity>("Age > ?", 100);

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(0));
        }

        #endregion

        #region Count Tests

        [Test]
        [Description("Test that Count returns total number of records in table")]
        public void Count_ReturnsTotalRecords()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30 });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 25 });
            adapter.Insert(new TestEntity { Id = 3, Name = "Bob", Age = 35 });

            // Act
            var count = adapter.Count<TestEntity>();

            // Assert
            Assert.That(count, Is.EqualTo(3));
        }

        [Test]
        [Description("Test that Count with table name returns total number of records")]
        public void Count_WithTableName_ReturnsTotalRecords()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30 });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 25 });

            // Act
            var count = adapter.Count("TestEntity");

            // Assert
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        [Description("Test that Count with key-value returns matching record count")]
        public void Count_ByKeyValue_ReturnsMatchingCount()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30 });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 25 });
            adapter.Insert(new TestEntity { Id = 3, Name = "John", Age = 35 });

            // Act
            var count = adapter.Count<TestEntity>("Name", "John");

            // Assert
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        [Description("Test that Count with table name and key-value returns matching record count")]
        public void Count_WithTableName_ByKeyValue_ReturnsMatchingCount()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30 });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 25 });
            adapter.Insert(new TestEntity { Id = 3, Name = "John", Age = 35 });

            // Act
            var count = adapter.Count("TestEntity", "Name", "John");

            // Assert
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        [Description("Test that Count with query string returns matching record count")]
        public void Count_WithQueryString_ReturnsMatchingCount()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30 });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 25 });
            adapter.Insert(new TestEntity { Id = 3, Name = "Bob", Age = 35 });

            // Act
            var count = adapter.CountByQuery<TestEntity>("Age > ?", 28);

            // Assert
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        [Description("Test that Count with table name and query string returns matching record count")]
        public void Count_WithTableName_AndQueryString_ReturnsMatchingCount()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30 });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 25 });
            adapter.Insert(new TestEntity { Id = 3, Name = "Bob", Age = 35 });

            // Act
            var count = adapter.CountByQuery("TestEntity", "Age > ?", 28);

            // Assert
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        [Description("Test that Count with dictionary and AND operator returns matching count")]
        public void Count_WithDictionary_AndOperator_ReturnsMatchingCount()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30 });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 25 });
            adapter.Insert(new TestEntity { Id = 3, Name = "John", Age = 30 });

            var conditions = new Dictionary<string, object>
            {
                { "Name", "John" },
                { "Age", 30 }
            };

            // Act
            var count = adapter.CountByQuery<TestEntity>(conditions, QueryLogicalOperator.And);

            // Assert
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        [Description("Test that Count with dictionary and OR operator returns matching count")]
        public void Count_WithDictionary_OrOperator_ReturnsMatchingCount()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30 });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 25 });
            adapter.Insert(new TestEntity { Id = 3, Name = "Bob", Age = 35 });

            var conditions = new Dictionary<string, object>
            {
                { "Name", "John" },
                { "Age", 25 }
            };

            // Act
            var count = adapter.CountByQuery<TestEntity>(conditions, QueryLogicalOperator.Or);

            // Assert
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        [Description("Test that Count with dictionary uses AND as default operator")]
        public void Count_WithDictionary_DefaultOperator_IsAnd()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30 });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 30 });

            var conditions = new Dictionary<string, object>
            {
                { "Name", "John" },
                { "Age", 30 }
            };

            // Act
            var count = adapter.CountByQuery<TestEntity>(conditions);

            // Assert
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        [Description("Test that Count with table name and dictionary returns matching count")]
        public void Count_WithTableName_AndDictionary_ReturnsMatchingCount()
        {
            // Arrange
            adapter.Insert(new TestEntity { Id = 1, Name = "John", Age = 30 });
            adapter.Insert(new TestEntity { Id = 2, Name = "Jane", Age = 25 });

            var conditions = new Dictionary<string, object>
            {
                { "Name", "Jane" }
            };

            // Act
            var count = adapter.CountByQuery("TestEntity", conditions);

            // Assert
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        [Description("Test that Count returns zero when table is empty")]
        public void Count_EmptyTable_ReturnsZero()
        {
            // Act
            var count = adapter.Count<TestEntity>();

            // Assert
            Assert.That(count, Is.EqualTo(0));
        }

        #endregion

        #region Dispose Tests

        [Test]
        [Description("Test that Dispose releases resources properly")]
        public void Dispose_ReleasesResources_Properly()
        {
            // Act
            adapter.Dispose();

            // Assert
            Assert.That(adapter.Database, Is.Null);
            Assert.That(adapter.Box, Is.Null);
            Assert.That(adapter.IsOpen, Is.False);
        }

        [Test]
        [Description("Test that Dispose can be called multiple times without exception")]
        public void Dispose_MultipleTimes_NoException()
        {
            Assert.DoesNotThrow((Action)Action);
            return;

            // Act & Assert
            void Action()
            {
                adapter.Dispose();
                adapter.Dispose();
            }
        }

        [Test]
        [Description("Test that operations throw exception after disposal")]
        public void Operations_AfterDispose_ThrowException()
        {
            // Arrange
            adapter.Dispose();

            // Act & Assert
            Assert.Throws<ObjectDisposedException>((Action)Action);
            return;
            
            void Action() => adapter.Count<TestEntity>();
        }

        #endregion

        #region Exception Tests

        [Test]
        [Description("Test that operations throw exception when database is not open")]
        public void Operations_BeforeOpen_ThrowException()
        {
            // Arrange
            var newAdapter = new IBoxDBAdapter("/tmp/test_iboxdb_notopen");
            newAdapter.EnsureTable<TestEntity>("Id");

            // Act & Assert
            Assert.Throws<NullReferenceException>((Action)Action);

            // Cleanup
            newAdapter.Dispose();
            if (System.IO.Directory.Exists("/tmp/test_iboxdb_notopen"))
            {
                System.IO.Directory.Delete("/tmp/test_iboxdb_notopen", true);
            }

            return;
            // ReSharper disable once AccessToDisposedClosure
            void Action() => newAdapter.Count<TestEntity>();
        }

        #endregion
    }
}