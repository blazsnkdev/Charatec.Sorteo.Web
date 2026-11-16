namespace Charatec.Sorteo.Api.Responses;

public sealed record ApiResponse<T>
(
    bool Success,
    int StatusCode,
    string Message,
    T? Data = default,
    Dictionary<string, string[]>? Errors = null
);
