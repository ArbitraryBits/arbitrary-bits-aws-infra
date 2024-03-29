using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.Route53;

namespace ArbitraryBitsAwsInfra
{
    public class ArbitraryBitsDatabase : Stack
    {
        internal DatabaseInstance DbInstance { get; set; }
        internal ArbitraryBitsDatabase(Construct scope, string id, Vpc vpc, IStackProps props = null) : base(scope, id, props)
        {
            var context = this.Node.TryGetContext("settings") as Dictionary<String, Object>;

            // var sg = new SecurityGroup(this, "ArbitraryBitsDatabaseSecurityGroupId", new SecurityGroupProps 
            // {
            //     Vpc = vpc,
            //     SecurityGroupName = "ArbitrraryBitsDatabaseSecurityGroup"
            // });
            var sg = SecurityGroup.FromLookup(
                this, 
                "ArbitraryBitsBastionHostDatabaseSecurityGroupId", 
                context["dbInstanceSecurityGroupId"] as String);

            // var adminuser = new DatabaseSecret(this, "ArbitrraryBitsDatabaseAdminuserSecretId", new DatabaseSecretProps() 
            // {
            //     Username = "adminuser",
            //     SecretName = "ArbitrraryBitsDatabaseAdminuserSecret"
            // });

            var adminuser = DatabaseSecret.FromSecretNameV2(
                this, 
                "ArbitrraryBitsDatabaseAdminuserSecretId", 
                "ArbitrraryBitsDatabaseAdminuserSecret");
            
            DbInstance = new DatabaseInstance(this, "ArbitrraryBitsDatabaseId", new DatabaseInstanceProps() 
            {
                StorageEncrypted = true,
                InstanceIdentifier = "ArbitrraryBitsDatabase",
                Credentials = Credentials.FromSecret(adminuser),
                Engine = DatabaseInstanceEngine.Postgres(new PostgresInstanceEngineProps() 
                {
                    Version = PostgresEngineVersion.VER_13_3
                }),
                Port = 5432,
                AllocatedStorage = 10,
                MaxAllocatedStorage = 100,
                StorageType = StorageType.GP2,
                PubliclyAccessible = false,
                MultiAz = false,
                PreferredBackupWindow = "02:00-04:00", // hh24:mi-hh24:mi
                PreferredMaintenanceWindow = "Sun:04:00-Sun:06:00", // ddd:hh24:mi-ddd:hh24:mi
                AllowMajorVersionUpgrade = false,
                AutoMinorVersionUpgrade = true,
                MonitoringInterval = Duration.Minutes(1),
                CloudwatchLogsExports = new [] { "postgresql", "upgrade" },
                CloudwatchLogsRetention = Amazon.CDK.AWS.Logs.RetentionDays.ONE_MONTH,
                PerformanceInsightRetention = PerformanceInsightRetention.DEFAULT,
                EnablePerformanceInsights = true,
                InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.SMALL),
                DeletionProtection = false,
                DeleteAutomatedBackups = false,
                BackupRetention = Duration.Days(7),
                RemovalPolicy = RemovalPolicy.SNAPSHOT,
                Vpc = vpc,
                SecurityGroups = new ISecurityGroup[] { sg },
                AvailabilityZone = context["mainAvailabilityZone"] as String,
                ParameterGroup = ParameterGroup.FromParameterGroupName(this, "DbParameterGroup", "default.postgres13"),
                SubnetGroup = SubnetGroup.FromSubnetGroupName(this, "ArbitrraryBitsDatabaseSubnetGroupId", "ArbitrraryBitsDatabaseSubnetGroup")
            });

            Amazon.CDK.Tags.Of(DbInstance).Add("Type", "AB-DB-RDS");

            // var hostedZone = new PrivateHostedZone(this, "ArbitraryBitsPrivateHostedZoneId", new PrivateHostedZoneProps() {
            //     Vpc = Vpc.FromLookup(this, "ImportedEcsVpcId", new VpcLookupOptions() {
            //         Tags = new Dictionary<string, string>() { { "Type", "ECS-VPC" } }
            //     }),
            //     ZoneName = "arbitrarybits.com"
            // });
            var hostedZone = PrivateHostedZone.FromLookup(this, "ArbitraryBitsPrivateHostedZoneId", new HostedZoneProviderProps() {
                DomainName = "arbitrarybits.com",
                PrivateZone = true,
                VpcId = Vpc.FromLookup(this, "ImportedEcsVpcId", new VpcLookupOptions() {
                    Tags = new Dictionary<string, string>() { { "Type", "ECS-VPC" } }
                }).VpcId
            });

            new CnameRecord(this, "ArbitraryBitsPrivateHostedZoneDbCnameRecordId", new CnameRecordProps() {
                Zone = hostedZone,
                RecordName = "db.arbitrarybits.com",
                DomainName = DbInstance.DbInstanceEndpointAddress
            });
            
            new CfnOutput(this, "DbInstanceEndpointAddressOutputId", new CfnOutputProps
            {
                Value = DbInstance.DbInstanceEndpointAddress,
                Description = "DB Instance endpoint adress",
                ExportName = "DbInstanceEndpointAddress"
            });

            new CfnOutput(this, "DbInstanceIdentifierOutputId", new CfnOutputProps
            {
                Value = DbInstance.InstanceIdentifier,
                Description = "DB Instance identifier",
                ExportName = "DbInstanceIdentifier"
            });
        }
    }
}