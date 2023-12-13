namespace ApplicationCore.Features.Orders.OrderLoading.LoadTailoredLivingSpreadsheetOrderData;

public record CoverPage(
    string JobName,
    bool ClosetParts,
    bool GarageCabinets,
    bool StandardCabinets,
    string ExteriorMaterial,
    string InteriorMaterial,
    bool NoToe,
    bool ToeNotch,
    string ToeNotchSize,
    bool LegLevelers,
    bool FinishedWithConfirmats,
    bool FinishedWithoutConfirmats,
    bool FinishedWithAppliedPanels,
    bool DovetailDrawerBoxes,
    string OtherDrawerBoxType,
    CounterTop Counter1,
    CounterTop Counter2,
    bool Rush,
    DateTime? RequestedDate,
    string AdditionalNotes
);

