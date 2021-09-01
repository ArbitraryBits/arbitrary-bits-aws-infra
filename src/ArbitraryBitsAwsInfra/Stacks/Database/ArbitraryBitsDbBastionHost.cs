using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

namespace ArbitraryBitsAwsInfra
{
    public class ArbitraryBitsDbBastionHost : Stack
    {
        internal ArbitraryBitsDbBastionHost(Construct scope, string id, Vpc vpc, IStackProps props = null) : base(scope, id, props)
        {
            var context = this.Node.TryGetContext("settings") as Dictionary<String, Object>;

            var image = MachineImage.LatestAmazonLinux(new AmazonLinuxImageProps() 
            {
                Generation = AmazonLinuxGeneration.AMAZON_LINUX_2,
                Edition = AmazonLinuxEdition.STANDARD,
                Storage = AmazonLinuxStorage.EBS,
                Virtualization = AmazonLinuxVirt.HVM
            });

            var sg = new SecurityGroup(this, "DbBastionHostSecurityGroupId", new SecurityGroupProps 
            {
                Vpc = vpc,
                SecurityGroupName = "BastionHost",
                AllowAllOutbound = true
            });

            sg.Connections.AllowFrom(
                Peer.Ipv4(context["allowIp"] as string), 
                new Port(new PortProps() 
                { 
                    StringRepresentation = "22",
                    Protocol = Protocol.TCP, 
                    FromPort = 22,
                    ToPort = 22,
                }),
                "Allow ssh access from my IP"
            );

            var instance = new Instance_(this, "DbBastionHostId", new InstanceProps() 
            {
                InstanceType = new InstanceType("t2.micro"),
                InstanceName = "BastionHost",
                MachineImage = image,
                Vpc = vpc,
                VpcSubnets = new SubnetSelection() 
                { 
                    SubnetType = SubnetType.PUBLIC
                },
                SecurityGroup = sg,
                KeyName = context["dbBastionHostKey"] as String
            });
            
            var cfn = new CfnOutput(this, "DbBastionHostDnsOutputId", new CfnOutputProps 
            {
                ExportName = "DNS",
                Value = instance.InstancePublicDnsName
            });

            var dbSecurityGroup = SecurityGroup.FromLookup(
                this, 
                "ArbitraryBitsBastionHostDatabaseSecurityGroupId", 
                context["dbInstanceSecurityGroupId"] as String);

            dbSecurityGroup.Connections.AllowFrom(instance, new Port(new PortProps() 
            { 
                StringRepresentation = "5432",
                Protocol = Protocol.TCP, 
                FromPort = 5432,
                ToPort = 5432,
            }), "Allow connections from Bastion host instance to DB");
        }
    }
}
