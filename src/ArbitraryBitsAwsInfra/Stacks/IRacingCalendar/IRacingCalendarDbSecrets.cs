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
        internal IRacingCalendarDbSecrets(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var context = this.Node.TryGetContext("settings") as Dictionary<String, Object>;

            var apiuser = new DatabaseSecret(this, "ArbitrraryBitsDatabaseApiuserdevSecretId", new DatabaseSecretProps() 
            {
                Username = "apiuserdev",
                SecretName = "ArbitrraryBitsDatabaseApiuserdevSecret"
            });

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

            var dbVpc = Vpc.FromLookup(this, "ImportedArbitraryBitsDatabaseId", new VpcLookupOptions() 
            {
                VpcId = context["dbInstanceVpcId"] as string
            });

            apiuser.Attach(dbInstance);

            var lambdaSg = new SecurityGroup(this, "DbBastionHostSecurityGroupId", new SecurityGroupProps 
            {
                Vpc = dbVpc,
                SecurityGroupName = "RotateApiuserdevSecurityGroup",
                AllowAllOutbound = true
            });

            dbSecurityGroup.Connections.AllowFrom(lambdaSg, new Port(new PortProps() 
            { 
                StringRepresentation = "5432",
                Protocol = Protocol.TCP, 
                FromPort = 5432,
                ToPort = 5432,
            }), "Allow connections from Lambda rotation apiuserdev to DB");

            apiuser.AddRotationSchedule("ApiuserdevRotationScheduleId", new RotationScheduleOptions() 
            {
                AutomaticallyAfter = Duration.Days(1),
                HostedRotation = HostedRotation.PostgreSqlSingleUser(new SingleUserHostedRotationOptions() 
                {
                    FunctionName = "RotateApiuserdev",
                    Vpc = dbVpc,
                    SecurityGroups = new [] { dbSecurityGroup },
                    VpcSubnets = new SubnetSelection() {
                        SubnetType = SubnetType.ISOLATED   
                    }
                })
            });
            
            Amazon.CDK.Tags.Of(apiuser).Add("App", "iRacingCalendar");
            Amazon.CDK.Tags.Of(apiuser).Add("AppEnv", "dev");
        }
    }
}