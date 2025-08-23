namespace Shared.DTOs;

public record ProductBarcodeCreateDto(
    int ProductId,
    string Code,
    string Type = "CODE128",
    bool IsPrimary = true
);
