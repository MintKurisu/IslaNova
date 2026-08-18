using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Entities.PropertyManagement;
using System.Text;

namespace IslaNova.Core.Application.Helpers
{
    /// <summary>
    /// Builds a rich, human-readable plain text description from a Property entity.
    /// This text is sent to the embedding model to generate the vector representation.
    ///
    /// The quality of the generated text directly impacts RAG retrieval accuracy,
    /// so it includes all semantically relevant fields in natural language form.
    /// </summary>
    public static class PropertyTextBuilder
    {
        /// <summary>
        /// Builds the embedding text for a property.
        /// Requires the property to be loaded with includes:
        /// PropertyType, SaleType, PropertyImprovements.Improvement
        /// </summary>
        public static string Build(
            Property property,
            string? agentName = null,
            string? propertyTypeName = null,
            string? saleTypeName = null)
        {
            var sb = new StringBuilder();

            // Identity
            sb.Append($"Propiedad código {property.Code}. ");

            // Type and sale mode
            var typeName = propertyTypeName ?? property.PropertyType?.Name ?? "Propiedad";
            var saleName = saleTypeName ?? property.SaleType?.Name ?? "Disponible";
            sb.Append($"Tipo: {typeName}. Modalidad: {saleName}. ");

            // Status
            sb.Append(property.Status == PropertyStatus.Available
                ? "Estado: disponible. "
                : "Estado: vendida. ");

            // Price
            sb.Append($"Precio: RD${property.Price:N0}. ");

            // Size
            sb.Append($"Tamaño del terreno: {property.LandSize} m². ");

            // Rooms
            sb.Append($"{property.Bedrooms} habitacion{(property.Bedrooms != 1 ? "es" : "")}. ");
            sb.Append($"{property.Bathrooms} baño{(property.Bathrooms != 1 ? "s" : "")}. ");

            // Location
            if (!string.IsNullOrWhiteSpace(property.City))
                sb.Append($"Ciudad: {property.City}. ");
            if (!string.IsNullOrWhiteSpace(property.Address))
                sb.Append($"Dirección: {property.Address}. ");
            if (property.Latitude.HasValue && property.Longitude.HasValue)
                sb.Append($"Coordenadas: {property.Latitude:F6}, {property.Longitude:F6}. ");

            // Improvements / amenities
            var improvements = property.PropertyImprovements?
                .Select(pi => pi.Improvement?.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();

            if (improvements != null && improvements.Count > 0)
                sb.Append($"Mejoras y amenidades: {string.Join(", ", improvements)}. ");

            // Agent
            if (!string.IsNullOrWhiteSpace(agentName))
                sb.Append($"Agente responsable: {agentName}. ");

            // Description (richest semantic content — placed last)
            if (!string.IsNullOrWhiteSpace(property.Description))
                sb.Append($"Descripción: {property.Description.Trim()}");

            return sb.ToString();
        }
    }
}
