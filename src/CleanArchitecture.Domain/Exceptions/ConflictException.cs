namespace CleanArchitecture.Domain.Exceptions;

public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string entityName, string conflictField, object value)
        : base($"{entityName} with {conflictField} '{value}' already exists.")
    {
        EntityName = entityName;
        ConflictField = conflictField;
        Value = value;
    }

    public string? EntityName { get; }
    public string? ConflictField { get; }
    public object? Value { get; }
}
