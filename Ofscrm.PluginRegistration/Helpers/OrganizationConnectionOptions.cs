namespace Ofscrm.PluginRegistration.Helpers
{
    public class OrganizationConnectionOptions
    {
        public bool LoadPlugins { get; set; }
        public bool LoadMessageEntities { get; set; }
        public bool LoadAssemblies { get; set; }
        public bool LoadServiceEndpoints { get; set; }
        public bool LoadSteps { get; set; }
        public bool LoadImages { get; set; }
    }
}