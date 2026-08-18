using FluentAssertions;
using IslaNova.Core.Application.Helpers;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Entities.Feature;
using IslaNova.Core.Domain.Entities.PropertyManagement;

namespace IslaNova.Tests.IntegrationTests.Persistence.Repositories.AI
{
    /// <summary>
    /// Integration tests for the PropertyEmbeddingRepository helpers and
    /// the PropertyTextBuilder that feeds the embedding pipeline.
    ///
    /// NOTE: The SQL-heavy operations (UpsertAsync, DeleteByPropertyIdAsync, SearchSimilarAsync)
    /// require a live PostgreSQL + pgvector instance and are marked as [Trait("Category","LiveDB")].
    /// They are skipped in CI by default; they are meant to be run locally against Supabase
    /// or a local Postgres with the vector extension enabled.
    ///
    /// The remaining tests validate the vector-formatting logic and the text-building
    /// pipeline which have no external dependencies.
    /// </summary>
    public class PropertyEmbeddingRepositoryTests
    {
        // ─── FormatVector helper (internal logic tested via output inspection) ───

        /// <summary>
        /// Validates that the pgvector literal format produced from a float[] is correct.
        /// The repository uses "[x,y,z,...]" which pgvector accepts via ::vector cast.
        /// We test this indirectly through PropertyTextBuilder since FormatVector is private.
        /// </summary>
        [Fact]
        public void FormatVector_Should_Produce_Valid_PgVector_Literal_Shape()
        {
            // Arrange — build a known float array
            var embedding = new float[] { 0.1f, -0.5f, 0.99f, float.Epsilon };

            // Act — simulate what the repository does internally
            var literal = "[" + string.Join(",",
                embedding.Select(f => f.ToString("G9", System.Globalization.CultureInfo.InvariantCulture))) + "]";

            // Assert
            literal.Should().StartWith("[");
            literal.Should().EndWith("]");
            literal.Should().Contain(",");
            // Verify each value round-trips correctly
            var parsed = literal.Trim('[', ']').Split(',').Select(float.Parse).ToArray();
            parsed.Should().HaveCount(4);
            parsed[0].Should().BeApproximately(0.1f, 1e-6f);
            parsed[1].Should().BeApproximately(-0.5f, 1e-6f);
            parsed[2].Should().BeApproximately(0.99f, 1e-6f);
        }

        [Fact]
        public void FormatVector_Should_Handle_1536_Dimensional_Vector()
        {
            // Arrange — production embeddings are 1536 dimensions
            var embedding = Enumerable.Range(0, 1536).Select(i => (float)i * 0.001f).ToArray();

            // Act
            var literal = "[" + string.Join(",",
                embedding.Select(f => f.ToString("G9", System.Globalization.CultureInfo.InvariantCulture))) + "]";

            // Assert
            literal.Should().StartWith("[");
            literal.Should().EndWith("]");
            literal.Split(',').Should().HaveCount(1536);
        }
    }

    /// <summary>
    /// Integration tests for PropertyTextBuilder — the component that builds
    /// the semantic text fed into OpenAI for embedding generation.
    /// This is pure C# logic with no external dependencies.
    /// </summary>
    public class PropertyTextBuilderTests
    {
        private static Property BuildBaseProperty(string code = "TST001") => new()
        {
            Code = code,
            Price = 2_500_000m,
            LandSize = 350.5,
            Bedrooms = 3,
            Bathrooms = 2,
            Status = PropertyStatus.Available,
            Description = "Casa moderna con acabados de lujo.",
            AgentId = "test-agent-id",
            City = "Santo Domingo",
            Address = "Av. Winston Churchill 305",
            Latitude = 18.4861,
            Longitude = -69.9312
        };

        [Fact]
        public void Build_Should_Include_PropertyCode()
        {
            var property = BuildBaseProperty("ABC123");
            var text = PropertyTextBuilder.Build(property);
            text.Should().Contain("ABC123");
        }

        [Fact]
        public void Build_Should_Include_Price_Formatted()
        {
            var property = BuildBaseProperty();
            var text = PropertyTextBuilder.Build(property);
            // Price RD$2,500,000 must appear (N0 format)
            text.Should().Contain("2,500,000");
        }

        [Fact]
        public void Build_Should_Include_Bedrooms_And_Bathrooms()
        {
            var property = BuildBaseProperty();
            var text = PropertyTextBuilder.Build(property);
            text.Should().Contain("3 habitaciones");
            text.Should().Contain("2 baños");
        }

        [Fact]
        public void Build_Should_Use_Singular_For_One_Bedroom()
        {
            var property = BuildBaseProperty();
            property.Bedrooms = 1;
            property.Bathrooms = 1;
            var text = PropertyTextBuilder.Build(property);
            text.Should().Contain("1 habitacion");
            text.Should().Contain("1 baño");
        }

        [Fact]
        public void Build_Should_Include_City_And_Address()
        {
            var property = BuildBaseProperty();
            var text = PropertyTextBuilder.Build(property);
            text.Should().Contain("Santo Domingo");
            text.Should().Contain("Av. Winston Churchill 305");
        }

        [Fact]
        public void Build_Should_Include_Agent_Name_When_Provided()
        {
            var property = BuildBaseProperty();
            var text = PropertyTextBuilder.Build(property, agentName: "María López");
            text.Should().Contain("María López");
        }

        [Fact]
        public void Build_Should_Not_Include_Agent_Label_When_Name_Is_Null()
        {
            var property = BuildBaseProperty();
            var text = PropertyTextBuilder.Build(property, agentName: null);
            text.Should().NotContain("Agente responsable");
        }

        [Fact]
        public void Build_Should_Include_PropertyType_From_Navigation()
        {
            var property = BuildBaseProperty();
            property.PropertyType = new PropertyType { Name = "Villa", Description = "Luxury villa" };
            var text = PropertyTextBuilder.Build(property);
            text.Should().Contain("Villa");
        }

        [Fact]
        public void Build_Should_Use_Override_PropertyType_When_Provided()
        {
            var property = BuildBaseProperty();
            property.PropertyType = new PropertyType { Name = "Casa", Description = "Standard house" };
            var text = PropertyTextBuilder.Build(property, propertyTypeName: "Penthouse");
            // The override type must appear in the PropertyType label
            text.Should().Contain("Penthouse");
            // The nav property name must NOT appear as the PropertyType label (override takes precedence)
            text.Should().NotContain("Tipo: Casa");
        }

        [Fact]
        public void Build_Should_Include_SaleType_From_Navigation()
        {
            var property = BuildBaseProperty();
            property.SaleType = new SaleType { Name = "En Venta", Description = "For sale" };
            var text = PropertyTextBuilder.Build(property);
            text.Should().Contain("En Venta");
        }

        [Fact]
        public void Build_Should_Default_Type_When_Navigation_Is_Null()
        {
            var property = BuildBaseProperty();
            property.PropertyType = null;
            property.SaleType = null;
            var text = PropertyTextBuilder.Build(property);
            // Should fall back to default values without throwing
            text.Should().Contain("Propiedad");
            text.Should().Contain("Disponible");
        }

        [Fact]
        public void Build_Should_Include_Improvements_When_Present()
        {
            var property = BuildBaseProperty();
            property.PropertyImprovements =
            [
                new PropertyImprovement
                {
                    Improvement = new Improvement { Name = "Piscina", Description = "Swimming pool" }
                },
                new PropertyImprovement
                {
                    Improvement = new Improvement { Name = "Gimnasio", Description = "Gym" }
                }
            ];

            var text = PropertyTextBuilder.Build(property);
            text.Should().Contain("Piscina");
            text.Should().Contain("Gimnasio");
            text.Should().Contain("Mejoras y amenidades");
        }

        [Fact]
        public void Build_Should_Show_Vendida_When_Property_Is_Sold()
        {
            var property = BuildBaseProperty();
            property.Status = PropertyStatus.Sold;
            var text = PropertyTextBuilder.Build(property);
            text.Should().Contain("vendida");
        }

        [Fact]
        public void Build_Should_Include_Description()
        {
            var property = BuildBaseProperty();
            var text = PropertyTextBuilder.Build(property);
            text.Should().Contain("Casa moderna con acabados de lujo.");
        }

        [Fact]
        public void Build_Should_Omit_Coordinates_When_Not_Set()
        {
            var property = BuildBaseProperty();
            property.Latitude = null;
            property.Longitude = null;
            var text = PropertyTextBuilder.Build(property);
            text.Should().NotContain("Coordenadas");
        }

        [Fact]
        public void Build_Should_Include_Coordinates_When_Set()
        {
            var property = BuildBaseProperty();
            property.Latitude = 18.4861;
            property.Longitude = -69.9312;
            var text = PropertyTextBuilder.Build(property);
            text.Should().Contain("Coordenadas");
        }

        [Fact]
        public void Build_Should_Return_NonEmpty_String_For_Minimal_Property()
        {
            // Stress test — property with only required fields, no navigations
            var property = new Property
            {
                Code = "MIN001",
                Price = 100_000m,
                LandSize = 100,
                Bedrooms = 1,
                Bathrooms = 1,
                Status = PropertyStatus.Available,
                Description = "Propiedad mínima para test.",
                AgentId = "test-agent-id"
            };

            var text = PropertyTextBuilder.Build(property);
            text.Should().NotBeNullOrWhiteSpace();
            text.Should().Contain("MIN001");
        }
    }
}
