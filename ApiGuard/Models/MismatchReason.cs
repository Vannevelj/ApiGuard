namespace ApiGuard.Models
{
    /// <summary>
    /// The value are in ascending order. The higher the value, the more specific the exception is and we will favor mismatches of that type over others.
    /// </summary>
    internal enum MismatchReason
    {
        None = 0,
        DefinitionChanged = 10,
        AttributeMismatch = 20,
        MemberAddedToInterface = 30,
        ParameterNameChanged = 40,
        ModifierChanged = 50,
        TypeKindChanged = 60,
        TypeNameChanged = 70,
        ElementRemoved = 80,
    }
}
