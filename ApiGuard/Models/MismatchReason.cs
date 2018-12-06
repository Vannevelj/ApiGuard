namespace ApiGuard.Models
{
    internal enum MismatchReason
    {
        None = 0,
        AttributeMismatch = 1,
        ElementRemoved = 2,
        ParameterNameChanged = 3,
        DefinitionChanged = 4,
        TypeChanged = 5,
    }
}
