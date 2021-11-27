using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.Route53;

namespace ArbitraryBitsAwsInfra
{
    public class ArbitraryBitsDatabasePrereq : Stack
    {
        internal ArbitraryBitsDatabasePrereq(Construct scope, string id, Vpc vpc, IStackProps props = null) : base(scope, id, props)
        {
            var context = this.Node.TryGetContext("settings") as Dictionary<String, Object>;

            var sg = new SecurityGroup(this, "ArbitraryBitsDatabaseSecurityGroupId", new SecurityGroupProps 
            {
                Vpc = vpc,
                SecurityGroupName = "ArbitrraryBitsDatabaseSecurityGroup"
            });

            var adminuser = new DatabaseSecret(this, "ArbitrraryBitsDatabaseAdminuserSecretId", new DatabaseSecretProps() 
            {
                Username = "adminuser",
                SecretName = "ArbitrraryBitsDatabaseAdminuserSecret"
            });

            new SubnetGroup(this, "ArbitrraryBitsDatabaseSubnetGroupId", new SubnetGroupProps() 
                {
                    Description = "Subnet group for DB",
                    Vpc = vpc,
                    RemovalPolicy = RemovalPolicy.DESTROY,
                    SubnetGroupName = "ArbitrraryBitsDatabaseSubnetGroup",
                    VpcSubnets = new SubnetSelection() 
                    {
                        SubnetType = SubnetType.ISOLATED
                    }
                });

            Amazon.CDK.Tags.Of(sg).Add("Type", "AB-DB-RDS");

            var hostedZone = new PrivateHostedZone(this, "ArbitraryBitsPrivateHostedZoneId", new PrivateHostedZoneProps() {
                Vpc = Vpc.FromLookup(this, "ImportedEcsVpcId", new VpcLookupOptions() {
                    Tags = new Dictionary<string, string>() { { "Type", "ECS-VPC" } }
                }),
                ZoneName = "arbitrarybits.com"
            });            

            new CfnOutput(this, "DbInstanceSecurityGroupOutputId", new CfnOutputProps
            {
                Value = sg.SecurityGroupId,
                Description = "DB Instance security group id",
                ExportName = "DbInstanceSecurityGroupId"
            });
        }
    }
}