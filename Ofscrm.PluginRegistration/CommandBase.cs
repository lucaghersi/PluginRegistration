using System;
using McTools.Xrm.Connection;
using Microsoft.Xrm.Tooling.Connector;
using Ofscrm.PluginRegistration.Helpers;
using Ofscrm.PluginRegistration.Wrappers;

namespace Ofscrm.PluginRegistration
{
    public abstract class CommandBase
    {
        protected CrmOrganization Organization { get; }

        protected CommandBase()
        {
            Organization = GetOrganization();
        }

        private static ConnectionDetail CreateConnection()
        {
            ConnectionDetail connection = new ConnectionDetail
            {
                UseConnectionString = true,
                ConnectionString = "AuthType=AD;Url=http://bemeche1crm01.ofscorp.objectway.com/OFSCRMWealthLGHE;",
                ConnectionName = "LGHE"
            };

            CrmServiceClient crmServiceClient = connection.GetCrmServiceClient(true);
            if (crmServiceClient.IsReady)
            {
                return connection;
            }

            throw new CrmInvalidConnectionException("Connection is not ready.");
        }

        private static CrmOrganization GetOrganization()
        {
            OrganizationConnectionOptions connectionOptions =
                new OrganizationConnectionOptions
                {
                    LoadPlugins = true,
                    LoadAssemblies = true
                };

            return new CrmOrganization(CreateConnection(), connectionOptions);
        }
    }

    internal class CrmInvalidConnectionException : Exception
    {
        public CrmInvalidConnectionException(string message):base(message)
        {
            
        }
    }
}
