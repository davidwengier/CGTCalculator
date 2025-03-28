using Microsoft.AspNetCore.Components;

namespace CGTCalculator.Components;

public partial class CgtSingleYearReport
{
    [Parameter]
    public required CGTCalculator.CgtSingleYearReport Report { get; set; }
}
