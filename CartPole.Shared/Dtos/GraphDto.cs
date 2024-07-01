using Graphs.Models;
using MemoryPack;

namespace CartPoleShared.DTOs;

[MemoryPackable]
internal partial record GraphDto
{
    public List<NodeDto> Nodes { get; set; }
    public List<EdgeDto> Edges { get; set; }
}

[MemoryPackable]
internal partial record NodeDto
{
    public required Guid Id { get; set; }
    public required string Label { get; set; }
    public required NodeType Type { get; set; }
    public required double Bias { get; set; }
    public required int? StateIndex { get; set; }
}

[MemoryPackable]
internal partial record EdgeDto
{
    public required Guid FromId { get; set; }
    public required Guid ToId { get; set; }
    public required double Weight { get; set; }
}
