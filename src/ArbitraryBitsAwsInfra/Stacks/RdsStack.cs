using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.RDS;

namespace ArbitraryBitsAwsInfra
{
    public class RdsStack : Stack
    {
        internal RdsStack(Construct scope, string id, Vpc vpc, Instance_ bastionHost, IStackProps props = null) : base(scope, id, props)
        {
            var context = this.Node.TryGetContext("dev") as Dictionary<String, Object>;

            var sg = new SecurityGroup(this, "RDSSecurityGroup", new SecurityGroupProps 
            {
                Vpc = vpc,
                SecurityGroupName = "RDS-DB-SG",
                AllowAllOutbound = false
            });
            sg.Connections.AllowFrom(bastionHost, new Port(new PortProps() 
            { 
                StringRepresentation = "3306",
                Protocol = Protocol.TCP, 
                FromPort = 3306,
                ToPort = 3306,
            }), "Allow connections from EC2 instance to DB");

            var db = new DatabaseInstance(this, "RDSInstance", new DatabaseInstanceProps() 
            {
                InstanceIdentifier = "Example-DB",
                Credentials = Credentials.FromGeneratedSecret("adminuser"),
                DatabaseName = "ExampleRdsDb",
                Engine = DatabaseInstanceEngine.Mysql(new MySqlInstanceEngineProps() 
                { 
                    Version = MysqlEngineVersion.VER_5_7_33
                }),
                Port = 3306,
                AllocatedStorage = 10,
                MultiAz = false,
                CloudwatchLogsExports = new [] { "audit", "error", "general", "slowquery" },
                InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.MICRO),
                DeletionProtection = false,
                DeleteAutomatedBackups = true,
                BackupRetention = Duration.Days(7),
                RemovalPolicy = RemovalPolicy.DESTROY,
                Vpc = vpc,
                SecurityGroups = new ISecurityGroup[] { sg },
                SubnetGroup = new SubnetGroup(this, "RDSSubnetGroup", new SubnetGroupProps() 
                {
                    Description = "Subnet group for DB",
                    Vpc = vpc,
                    RemovalPolicy = RemovalPolicy.DESTROY,
                    SubnetGroupName = "RDBSubnetForDB",
                    VpcSubnets = new SubnetSelection() 
                    {
                        SubnetType = SubnetType.ISOLATED
                    }
                })
            });

            new CfnOutput(this, "DbInstanceEndpointAddressOutput", new CfnOutputProps
            {
                Value = db.DbInstanceEndpointAddress,
                Description = "DB Instance endpoint adress"
            });

            new CfnOutput(this, "DbConnectionStringOutput", new CfnOutputProps
            {
                Value = string.Format("mysql -h {0} -P 3306 -u adminuser -p", db.DbInstanceEndpointAddress),
                Description = "DB Instance connection string"
            });
        }
    }
}
