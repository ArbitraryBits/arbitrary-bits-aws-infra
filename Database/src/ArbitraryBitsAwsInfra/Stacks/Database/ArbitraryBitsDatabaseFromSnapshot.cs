using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.SecretsManager;

namespace ArbitraryBitsAwsInfra
{
    public class ArbitraryBitsDatabaseFromSnapshot : Stack
    {
        internal DatabaseInstanceFromSnapshot DbInstance { get; set; }

        internal ArbitraryBitsDatabaseFromSnapshot(Construct scope, string id, Vpc vpc, IStackProps props = null) : base(scope, id, props)
        {
            var context = this.Node.TryGetContext("settings") as Dictionary<String, Object>;

            var dbSecurityGroup = SecurityGroup.FromLookup(
                this, 
                "ArbitraryBitsDatabaseSecurityGroupId", 
                context["dbInstanceSecurityGroupId"] as String);
            
            var adminuser = new DatabaseSecret(this, "ArbitrraryBitsDatabaseAdminuserSecretId", new DatabaseSecretProps() 
            {
                Username = "adminuser",
                SecretName = "ArbitrraryBitsDatabaseAdminuserSecret"
            });
            
            DbInstance = new DatabaseInstanceFromSnapshot(this, "ArbitrraryBitsDatabaseId", new DatabaseInstanceFromSnapshotProps() {
                Credentials = SnapshotCredentials.FromSecret(adminuser),
                SnapshotIdentifier = context["dbSnapshotName"] as String,
                CopyTagsToSnapshot = true,
                InstanceIdentifier = "ArbitrraryBitsDatabase",
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
                SecurityGroups = new ISecurityGroup[] { dbSecurityGroup },
                AvailabilityZone = context["mainAvailabilityZone"] as String,
                ParameterGroup = ParameterGroup.FromParameterGroupName(this, "DbParameterGroup", "default.postgres13"),
                SubnetGroup = SubnetGroup.FromSubnetGroupName(this, "ArbitrraryBitsDatabaseSubnetGroupId", "ArbitrraryBitsDatabaseSubnetGroup")
            });
           
            Amazon.CDK.Tags.Of(DbInstance).Add("Type", "AB-DB-RDS");

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