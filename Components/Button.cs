using MudBlazor;

namespace CGTCalculator.Components;

public class Button : MudButton
{
    protected override void OnParametersSet()
    {
        this.Variant = Variant.Filled;
        this.Color = MudBlazor.Color.Primary;
        this.Size = MudBlazor.Size.Medium;

        base.OnParametersSet();
    }
}
