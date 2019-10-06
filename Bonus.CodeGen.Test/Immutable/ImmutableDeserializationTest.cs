using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Xunit;

namespace Bonus.CodeGen
{
    [GenerateImmutable]
    public partial class Serialization
    {
        public long Number { get; }
    }

    public class ImmutableDeserializationTest
    {
        [Fact]
        public void NewtonsoftJson()
        {
            var original = Serialization.Create(number: 42);

            var json = JsonConvert.SerializeObject(original);
            var deserialized = JsonConvert.DeserializeObject<Serialization>(json);

            Assert.Equal(original.Number, deserialized.Number);
        }

        [Fact(Skip = "Deserialization of reference types without parameterless constructor is not supported (3.0.0)")]
        public void SystemTextJson()
        {
            var original = Serialization.Create(number: 42);

            var json = System.Text.Json.JsonSerializer.Serialize(original);
            var deserialized = System.Text.Json.JsonSerializer.Deserialize<Serialization>(json);

            Assert.Equal(original.Number, deserialized.Number);
        }

        [Fact]
        public async Task Dapper()
        {
            var builder = new SqliteConnectionStringBuilder();
            builder.DataSource = ":memory:";
            builder.Cache = SqliteCacheMode.Shared;

            var createTable = $"CREATE TABLE {nameof(Serialization)} ({nameof(Serialization.Number)} INTEGER)";
            var insert = $"INSERT INTO {nameof(Serialization)} ({nameof(Serialization.Number)}) VALUES (5)";
            var selectAll = $"SELECT * FROM {nameof(Serialization)}";

            await using var connection = new SqliteConnection(builder.ConnectionString);
            await connection.OpenAsync();
            await connection.ExecuteAsync(createTable);
            await connection.ExecuteAsync(insert);
            var materialized = await connection.QueryFirstAsync<Serialization>(selectAll);

            Assert.Equal(5, materialized.Number);
    }
}
}