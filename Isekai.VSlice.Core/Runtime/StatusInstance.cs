using Isekai.VSlice.Core.Content.Dto;

namespace Isekai.VSlice.Core.Runtime;

public sealed class StatusInstance
{
    public required StatusTemplateDto Template { get; init; }
    public required int Stacks { get; set; }
    public required int RemainingTurns { get; set; }

    public string Id => Template.StatusId;

    public override string ToString() => $"{Id}(stacks={Stacks},rem={RemainingTurns})";
}