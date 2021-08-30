using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.RDS;

namespace ArbitraryBitsAwsInfra
{
    public class ArbitraryBitsDevDatabase : Stack
    {
        internal ArbitraryBitsDevDatabase(Construct scope, string id, Vpc vpc, IStackProps props = null) : base(scope, id, props)
        {
            var context = this.Node.TryGetContext("dev") as Dictionary<String, Object>;

            var sg = new SecurityGroup(this, "ArbitraryBitsDevDatabaseSecurityGroupId", new SecurityGroupProps 
            {
                Vpc = vpc,
                SecurityGroupName = "ArbitrraryBitsDevDatabaseSecurityGroup",
                AllowAllOutbound = false
            });
            sg.Connections.AllowFrom(
                Peer.Ipv4(context["databaseAllowIp"] as string), 
                new Port(new PortProps() 
                { 
                    StringRepresentation = "3306",
                    Protocol = Protocol.TCP, 
                    FromPort = 3306,
                    ToPort = 3306,
                }),
                "Allow database connections from my IP"
            );
            
            // ServerlessCluster
            var db = new DatabaseInstance(this, "ArbitrraryBitsDevDatabaseId", new DatabaseInstanceProps() 
            {
                InstanceIdentifier = "ArbitrraryBitsDevDatabase",
                Credentials = Credentials.FromGeneratedSecret("adminuser"),
                DatabaseName = "TestDB",
                Engine = DatabaseInstanceEngine.Mysql(new MySqlInstanceEngineProps() 
                { 
                    Version = MysqlEngineVersion.VER_5_7_33
                }),
                PubliclyAccessible = true,
                Port = 3306,
                AllocatedStorage = 10,
                StorageType = StorageType.STANDARD,
                MultiAz = false,
                CloudwatchLogsExports = new [] { "audit", "error", "general", "slowquery" },
                InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.MICRO),
                DeletionProtection = false,
                DeleteAutomatedBackups = true,
                BackupRetention = Duration.Days(7),
                RemovalPolicy = RemovalPolicy.DESTROY,
                Vpc = vpc,
                SecurityGroups = new ISecurityGroup[] { sg },
                SubnetGroup = new SubnetGroup(this, "ArbitrraryBitsDevDatabaseSubnetGroupId", new SubnetGroupProps() 
                {
                    Description = "Subnet group for DB",
                    Vpc = vpc,
                    RemovalPolicy = RemovalPolicy.DESTROY,
                    SubnetGroupName = "ArbitrraryBitsDevDatabaseSubnetGroup",
                    VpcSubnets = new SubnetSelection() 
                    {
                        SubnetType = SubnetType.PUBLIC
                    }
                })
            });

            Amazon.CDK.Tags.Of(sg).Add("Type", "AB-DB-DEV-RDS");
            Amazon.CDK.Tags.Of(db).Add("Type", "AB-DB-DEV-RDS");

            new CfnOutput(this, "DbInstanceEndpointAddressOutputId", new CfnOutputProps
            {
                Value = db.DbInstanceEndpointAddress,
                Description = "DB Instance endpoint adress"
            });

            new CfnOutput(this, "DbConnectionStringOutputId", new CfnOutputProps
            {
                Value = string.Format("mysql -h {0} -P 3306 -u adminuser -p", db.DbInstanceEndpointAddress),
                Description = "DB Instance connection string"
            });
        }
    }
}

/*
// ServerlessCluster example
var db = new ServerlessCluster(this, "ArbitrraryBitsDevDatabaseClusterId", new ServerlessClusterProps() {
    Engine = DatabaseClusterEngine.AuroraMysql(new AuroraMysqlClusterEngineProps() {
        Version = AuroraMysqlEngineVersion.VER_2_07_1
    }),
    Credentials = Credentials.FromGeneratedSecret("adminuser"),
    DefaultDatabaseName = "DefaultDatabase",
    ClusterIdentifier = "ArbitrraryBitsDevDatabaseCluster",
    DeletionProtection = false,
    BackupRetention = Duration.Days(7),
    RemovalPolicy = RemovalPolicy.DESTROY,
    Scaling = new ServerlessScalingOptions() {
        AutoPause = Duration.Minutes(5),
        MinCapacity = AuroraCapacityUnit.ACU_1,
        MaxCapacity = AuroraCapacityUnit.ACU_2
    },
    Vpc = vpc,
    SubnetGroup = new SubnetGroup(this, "ArbitrraryBitsDevDatabaseSubnetGroupId", new SubnetGroupProps() 
    {
        Description = "Subnet group for DB",
        Vpc = vpc,
        RemovalPolicy = RemovalPolicy.DESTROY,
        SubnetGroupName = "ArbitrraryBitsDevDatabaseSubnetGroup",
        VpcSubnets = new SubnetSelection() 
        {
            SubnetType = SubnetType.PUBLIC
        }
    }),
    SecurityGroups = new ISecurityGroup[] { sg },
});
*/
