using System;

namespace nomad_gis_V2.Exceptions
{
    /// <summary>
    /// Для ошибок валидации (400 Bad Request)
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }

    /// <summary>
    /// Для ненайденных ресурсов (404 Not Found)
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Для конфликтов (409 Conflict), например, дубликат email
    /// </summary>
    public class DuplicateException : Exception
    {
        public DuplicateException(string message) : base(message) { }
    }
    
    /// <summary>
    /// Для ошибок авторизации (401 Unauthorized), например, неверный пароль
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }
}