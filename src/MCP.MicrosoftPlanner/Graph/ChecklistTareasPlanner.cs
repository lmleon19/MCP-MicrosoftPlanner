using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Serialization;

namespace MCP.MicrosoftPlanner.Graph;

/// <summary>Convierte el tipo abierto de checklist de Planner hacia y desde el contrato del servidor.</summary>
internal static class ChecklistTareasPlanner
{
    private const string TipoODataElementoChecklist = "#microsoft.graph.plannerChecklistItem";

    public static PlannerChecklistItems Crear(IReadOnlyList<ElementoChecklistPlanner> checklist) => new()
    {
        AdditionalData = checklist.ToDictionary(
            elemento => elemento.Id,
            elemento => elemento.Titulo is null
                ? (object)null!
                : new PlannerChecklistItem
                {
                    OdataType = TipoODataElementoChecklist,
                    Title = elemento.Titulo,
                    IsChecked = elemento.Completado,
                },
            StringComparer.Ordinal),
    };

    public static IReadOnlyList<ElementoChecklistPlanner> Mapear(PlannerChecklistItems? checklist) =>
        checklist?.AdditionalData?
            .Select(MapearElemento)
            .OfType<ElementoChecklistPlanner>()
            .OrderBy(elemento => elemento.Id, StringComparer.Ordinal)
            .ToArray() ?? [];

    private static ElementoChecklistPlanner? MapearElemento(KeyValuePair<string, object> elemento) => elemento.Value switch
    {
        PlannerChecklistItem item when !string.IsNullOrWhiteSpace(item.Title) =>
            new ElementoChecklistPlanner(elemento.Key, item.Title, item.IsChecked is true),
        UntypedObject item => MapearElementoSinTipo(elemento.Key, item),
        _ => null,
    };

    private static ElementoChecklistPlanner? MapearElementoSinTipo(string id, UntypedObject item)
    {
        var propiedades = item.GetValue();
        if (!propiedades.TryGetValue("title", out var nodoTitulo) || nodoTitulo is not UntypedString titulo ||
            string.IsNullOrWhiteSpace(titulo.GetValue()))
        {
            return null;
        }

        var completado = propiedades.TryGetValue("isChecked", out var nodoCompletado) &&
            nodoCompletado is UntypedBoolean valorCompletado && valorCompletado.GetValue();
        return new ElementoChecklistPlanner(id, titulo.GetValue(), completado);
    }
}
