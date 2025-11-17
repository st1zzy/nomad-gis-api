using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using nomad_gis_V2.Exceptions; // Подключаем наши исключения

namespace nomad_gis_V2.Middleware
{
    // DTO для стандартизированного ответа об ошибке
    public class ErrorResponse
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = string.Empty;
    }

    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Пытаемся выполнить следующий middleware в конвейере
                await _next(context);
            }
            catch (Exception ex)
            {
                // Если поймали ошибку, обрабатываем ее
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;

            var errorResponse = new ErrorResponse();

            // Определяем HTTP статус и сообщение в зависимости от типа ошибки
            switch (exception)
            {
                case ValidationException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest; // 400
                    errorResponse.Message = exception.Message;
                    break;
                case NotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound; // 404
                    errorResponse.Message = exception.Message;
                    break;
                case DuplicateException:
                    response.StatusCode = (int)HttpStatusCode.Conflict; // 409
                    errorResponse.Message = exception.Message;
                    break;
                case UnauthorizedException:
                case UnauthorizedAccessException: // Ловим и базовый тип
                    response.StatusCode = (int)HttpStatusCode.Unauthorized; // 401
                    errorResponse.Message = exception.Message;
                    break;
                default:
                    // Все остальные ошибки - это 500
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Message = "Internal Server Error. Please try again later.";
                    break;
            }

            // Логгируем ошибку (важно для production)
            _logger.LogError(exception, "[GlobalException] {Message}", exception.Message);

            var result = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(result);
        }
    }
}