using FakeXrmEasy.Core.Db;
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
    }
}
