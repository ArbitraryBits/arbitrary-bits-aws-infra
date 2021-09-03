using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.SecretsManager;

namespace ArbitraryBitsAwsInfra
{
    public class IRacingCalendarDbSecrets : Stack
    {
        internal IRacingCalendarDbSecrets(Construct scope, string id, DatabaseInstance dbInstance, IStackProps props = null) : base(scope, id, props)
        {
            var context = this.Node.TryGetContext("settings") as Dictionary<String, Object>;

            var apiuser = new DatabaseSecret(this, "ArbitrraryBitsDatabaseApiuserdevSecretId", new DatabaseSecretProps() 
            {
                Username = "apiuserdev",
                SecretName = "ArbitrraryBitsDatabaseApiuserdevSecret"
            });
            
            dbInstance.AddRotationMultiUser("ArbitrraryBitsDatabaseApiuserdevSecretRotationId", new RotationMultiUserOptions() {
                Secret = apiuser.Attach(dbInstance),
                AutomaticallyAfter = Duration.Days(1)
            });
            
            Amazon.CDK.Tags.Of(apiuser).Add("App", "iRacingCalendar");
            Amazon.CDK.Tags.Of(apiuser).Add("AppEnv", "dev");
        }
    }
}