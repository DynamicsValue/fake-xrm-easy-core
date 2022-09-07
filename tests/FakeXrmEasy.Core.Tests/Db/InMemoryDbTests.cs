using FakeXrmEasy.Core.Db;
using FakeXrmEasy.Core.Db.Exceptions;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Db
{
    public class InMemoryDbTests 
    {
        private readonly InMemoryDb _db;

        public InMemoryDbTests()
        {
            _db = new InMemoryDb();
        }

        [Fact]
        public void Should_add_table_to_db()
        {
            InMemoryTable table;
            _db.AddTable("account", out table);

            Assert.NotNull(table);
            Assert.True(_db._tables.ContainsKey("account"));
        }

        [Fact]
        public void Should_throw_exception_if_table_already_exists_and_a_table_with_same_name_is_added_twice()
        {
            InMemoryTable table;
            _db.AddTable("account", out table);

            Assert.Throws<TableAlreadyExistsException>(() => _db.AddTable("account", out table));
        }

        [Fact]
        public void Should_return_true_if_db_contains_table()
        {
            InMemoryTable table;
            _db.AddTable("account", out table);

            Assert.True(_db.ContainsTable("account"));
        }

        [Fact]
        public void Should_return_false_if_db_does_not_contain_table()
        {
            Assert.False(_db.ContainsTable("account"));
        }
    }
}
