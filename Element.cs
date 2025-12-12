// Element.cs
using System.Text.Json.Serialization;

public class Element
{
    [JsonPropertyName("atomicNumber")]
    public int AtomicNumber { get; set; }

    [JsonPropertyName("symbol")]
    public required string Symbol { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("atomicMass")]
    public required string AtomicMass { get; set; }

    [JsonPropertyName("standardState")]
    public string? StandardState { get; set; }

    [JsonPropertyName("electronicConfiguration")]
    public string? ElectronConfig { get; set; }

    [JsonPropertyName("groupBlock")]
    public string? GroupBlock { get; set; }

    [JsonPropertyName("meltingPoint")]
    public int? MeltingPoint { get; set; }

    [JsonPropertyName("boilingPoint")]
    public int? BoilingPoint { get; set; }

    [JsonPropertyName("density")]
    public double? Density { get; set; }

    [JsonPropertyName("electronegativity")]
    public required double? Electronegativity { get; set; }

    // To compare if one Element is the same as another, we need to specify what property should be compared
    // This tells the object that it should compare the atomic numbers of the Element objects if asked
    public bool Equals(Element? comparedElement) => comparedElement != null && AtomicNumber == comparedElement.AtomicNumber;
}