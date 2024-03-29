using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.SecretsManager;

namespace ArbitraryBitsAwsInfra
{
    public class ToDoDbSecrets : Stack
    {
        internal ToDoDbSecrets(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var context = this.Node.TryGetContext("settings") as Dictionary<String, Object>;
            
            var dbSecurityGroup = SecurityGroup.FromLookup(
                this, 
                "ArbitraryBitsBastionHostDatabaseSecurityGroupId", 
                context["dbInstanceSecurityGroupId"] as String
            );

            var dbInstance = DatabaseInstance.FromDatabaseInstanceAttributes(this, "ImportedArbitraryBitsDatabaseInstanceId", new DatabaseInstanceAttributes() 
            {
                InstanceEndpointAddress = Fn.ImportValue("DbInstanceEndpointAddress"),
                InstanceIdentifier = Fn.ImportValue("DbInstanceIdentifier"),
                SecurityGroups = new [] { dbSecurityGroup }
            });

            var userProd = new DatabaseSecret(this, "ArbitrraryBitsDatabaseTodouserprodSecretId", new DatabaseSecretProps() 
            {
                Username = "todouserprod",
                SecretName = "ArbitrraryBitsDatabaseTodouserprodSecret"
            });
            userProd.Attach(dbInstance);
            Amazon.CDK.Tags.Of(userProd).Add("App", "ToDo");
            Amazon.CDK.Tags.Of(userProd).Add("AppEnv", "prod");

            var userDev = new DatabaseSecret(this, "ArbitrraryBitsDatabaseTodouserdevSecretId", new DatabaseSecretProps() 
            {
                Username = "todouserdev",
                SecretName = "ArbitrraryBitsDatabaseTodouserdevSecret"
            });
            userDev.Attach(dbInstance);

            Amazon.CDK.Tags.Of(userDev).Add("App", "ToDo");
            Amazon.CDK.Tags.Of(userDev).Add("AppEnv", "dev");
        }
    }
}