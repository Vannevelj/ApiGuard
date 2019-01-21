namespace ApiGuard.Models
{
    /// <summary>
    /// The value are in ascending order. The higher the value, the more specific the exception is and we will favor mismatches of that type over others.
    /// </summary>
    internal enum MismatchReason
    {
        None = 0,
        DefinitionChanged = 10,
        ElementRemoved = 20,
        AttributeMismatch = 30,
        MemberAddedToInterface = 40,
        ParameterNameChanged = 50,
        TypeNameChanged = 60,
        ModifierChanged = 70,
        TypeKindChanged = 80,
    }
}
