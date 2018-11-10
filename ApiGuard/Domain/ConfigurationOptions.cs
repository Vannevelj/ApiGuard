namespace ApiGuard.Domain
{
    public class ConfigurationOptions
    {
        public static ConfigurationOptions Default => new ConfigurationOptions
        {
            AllowDataMemberReorder = false,
            AllowObsoleteMemberChanges = true,
        };

        public bool AllowDataMemberReorder { get; set; }
        public bool AllowObsoleteMemberChanges { get; set; }
    }
}
