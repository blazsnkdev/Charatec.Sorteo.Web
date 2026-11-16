using Charatec.Sorteo.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace Charatec.Sorteo.Api.Responses;

public static class ApiResults
{
    public static IActionResult Ok<T>(T data, string message = "Operación exitosa.")
    {
        return Response(
            StatusCodes.Status200OK,
            true,
            message,
            data);
    }

    public static IActionResult Created<T>(T data, string message = "Recurso creado correctamente.")
    {
        return Response(
            StatusCodes.Status201Created,
            true,
            message,
            data);
    }

    public static IActionResult NoContent()
    {
        return new NoContentResult();//TODO: falta implementar
    }


    public static IActionResult BadRequest(
        string message = "Solicitud incorrecta.",
        Dictionary<string, string[]>? errors = null)
    {
        return Error(
            StatusCodes.Status400BadRequest,
            message,
            errors);
    }

    public static IActionResult Unauthorized(
        string message = "No autenticado.")
    {
        return Error(
            StatusCodes.Status401Unauthorized,
            message);
    }

    public static IActionResult Forbidden(
        string message = "No tienes permisos.")
    {
        return Error(
            StatusCodes.Status403Forbidden,
            message);
    }

    public static IActionResult NotFound(
        string message = "Recurso no encontrado.")
    {
        return Error(
            StatusCodes.Status404NotFound,
            message);
    }

    public static IActionResult Conflict(
        string message = "El recurso ya existe.",
        Dictionary<string, string[]>? errors = null)
    {
        return Error(
            StatusCodes.Status409Conflict,
            message,
            errors);
    }

    public static IActionResult Unprocessable(
        string message = "No se puede procesar la operación.")
    {
        return Error(
            StatusCodes.Status422UnprocessableEntity,
            message);
    }

    public static IActionResult TooManyRequests(
        string message = "Demasiadas solicitudes.")
    {
        return Error(
            StatusCodes.Status429TooManyRequests,
            message);
    }

    // ========================================
    // ERRORES DEL SERVIDOR
    // ========================================

    public static IActionResult InternalServerError(
        string message = "Ocurrió un error interno.")
    {
        return Error(
            StatusCodes.Status500InternalServerError,
            message);
    }

    // ========================================
    // CONSTRUCTORES INTERNOS
    // ========================================

    private static IActionResult Error(
        int statusCode,
        string message,
        Dictionary<string, string[]>? errors = null)
    {
        return Response<object>(
            statusCode,
            false,
            message,
            null,
            errors);
    }

    private static IActionResult Response<T>(
        int statusCode,
        bool success,
        string message,
        T? data = default,
        Dictionary<string, string[]>? errors = null)
    {
        var response = new ApiResponse<T>(
            success,
            statusCode,
            message,
            data,
            errors);

        return new ObjectResult(response)
        {
            StatusCode = statusCode
        };
    }

    public static IActionResult FromResult<T>(
        Result<T> result,
        string successMessage = "Operación exitosa.",
        int successStatusCode = StatusCodes.Status200OK)
    {
        if (result.IsSuccess)
        {
            return Response(
                successStatusCode,
                true,
                successMessage,
                result.Value);
        }

        var error = result.Error!;

        return error.Code switch
        {
            // REGISTRO
            "EMAIL_ALREADY_EXISTS" =>
                Conflict(error.Message),

            "INVALID_PASSWORD" or
            "VALIDATION_ERROR" =>
                BadRequest(error.Message),

            // LOGIN
            "INVALID_CREDENTIALS" or
            "UNAUTHORIZED" =>
                Unauthorized(error.Message),

            "ACCOUNT_LOCKED" =>
                Error(
                    StatusCodes.Status423Locked,
                    error.Message),

            "ACCOUNT_DISABLED" or
            "FORBIDDEN" =>
                Forbidden(error.Message),

            // OTROS
            "NOT_FOUND" =>
                NotFound(error.Message),

            "BUSINESS_RULE_VIOLATION" =>
                Unprocessable(error.Message),

            _ => InternalServerError()
        };
    }

}
