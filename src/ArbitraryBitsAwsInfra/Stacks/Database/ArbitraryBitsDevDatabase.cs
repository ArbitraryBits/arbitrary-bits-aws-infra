using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.RDS;

namespace ArbitraryBitsAwsInfra
{
    public class ArbitraryBitsDatabase : Stack
    {
        internal ArbitraryBitsDatabase(Construct scope, string id, Vpc vpc, IStackProps props = null) : base(scope, id, props)
        {
            var context = this.Node.TryGetContext("settings") as Dictionary<String, Object>;

            var sg = new SecurityGroup(this, "ArbitraryBitsDatabaseSecurityGroupId", new SecurityGroupProps 
            {
                Vpc = vpc,
                SecurityGroupName = "ArbitrraryBitsDatabaseSecurityGroup",
                AllowAllOutbound = false
            });
            sg.Connections.AllowFrom(
                Peer.Ipv4(context["databaseAllowIp"] as string), 
                new Port(new PortProps() 
                { 
                    StringRepresentation = "5432",
                    Protocol = Protocol.TCP, 
                    FromPort = 5432,
                    ToPort = 5432,
                }),
                "Allow database connections from my IP"
            );
            
            // ServerlessCluster
            var db = new DatabaseInstance(this, "ArbitrraryBitsDatabaseId", new DatabaseInstanceProps() 
            {
                InstanceIdentifier = "ArbitrraryBitsDatabase",
                Credentials = Credentials.FromGeneratedSecret("adminuser"),
                Engine = DatabaseInstanceEngine.Postgres(new PostgresInstanceEngineProps() {
                    Version = PostgresEngineVersion.VER_13_3
                }),
                DatabaseName = "CDK", 
                Port = 5432,
                AllocatedStorage = 10,
                MaxAllocatedStorage = 100,
                StorageType = StorageType.GP2,
                PubliclyAccessible = true,
                MultiAz = false,
                PreferredBackupWindow = "02:00-04:00", // hh24:mi-hh24:mi
                PreferredMaintenanceWindow = "Sun:04:00-Sun:06:00", // ddd:hh24:mi-ddd:hh24:mi
                AllowMajorVersionUpgrade = false,
                AutoMinorVersionUpgrade = true,
                MonitoringInterval = Duration.Minutes(1),
                CloudwatchLogsExports = new [] { "audit", "error", "general", "slowquery", "postgresql", "upgrade" },
                CloudwatchLogsRetention = Amazon.CDK.AWS.Logs.RetentionDays.ONE_MONTH,
                PerformanceInsightRetention = PerformanceInsightRetention.DEFAULT,
                EnablePerformanceInsights = true,
                InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.MICRO),
                DeletionProtection = false,
                DeleteAutomatedBackups = true,
                BackupRetention = Duration.Days(7),
                RemovalPolicy = RemovalPolicy.DESTROY,
                Vpc = vpc,
                SecurityGroups = new ISecurityGroup[] { sg },
                AvailabilityZone = context["mainAvailabilityZone"] as String,
                ParameterGroup = ParameterGroup.FromParameterGroupName(this, "DbParameterGroup", "default.postgres13"),
                SubnetGroup = new SubnetGroup(this, "ArbitrraryBitsDatabaseSubnetGroupId", new SubnetGroupProps() 
                {
                    Description = "Subnet group for DB",
                    Vpc = vpc,
                    RemovalPolicy = RemovalPolicy.DESTROY,
                    SubnetGroupName = "ArbitrraryBitsDatabaseSubnetGroup",
                    VpcSubnets = new SubnetSelection() 
                    {
                        SubnetType = SubnetType.PUBLIC
                    }
                })
            });

            Amazon.CDK.Tags.Of(sg).Add("Type", "AB-DB-RDS");
            Amazon.CDK.Tags.Of(db).Add("Type", "AB-DB-RDS");

            new CfnOutput(this, "DbInstanceEndpointAddressOutputId", new CfnOutputProps
            {
                Value = db.DbInstanceEndpointAddress,
                Description = "DB Instance endpoint adress"
            });
        }
    }
}