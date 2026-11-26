namespace QuickBasket.Application.Common;

public abstract class AppException(string message) : Exception(message);
public sealed class NotFoundException(string message) : AppException(message);
public sealed class ValidationException(string message) : AppException(message);
public sealed class ConflictException(string message) : AppException(message);
public sealed class ForbiddenException(string message) : AppException(message);
public sealed class UnauthorizedAppException(string message) : AppException(message);
public sealed class PaymentMismatchException(string message) : AppException(message);

/// <summary>Raised by the unit of work when optimistic concurrency fails; callers re-read and retry.</summary>
public sealed class ConcurrencyConflictException(string message) : AppException(message);

/// <summary>Raised when a unique index rejects a duplicate (idempotency keys, webhook event ids).</summary>
public sealed class DuplicateRecordException(string message) : AppException(message);
