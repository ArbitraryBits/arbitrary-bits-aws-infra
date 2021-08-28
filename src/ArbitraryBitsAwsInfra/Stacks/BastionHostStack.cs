using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

namespace ArbitraryBitsAwsInfra
{
    public class BastionHostStack : Stack
    {
        internal Instance_ Instance { get; set; }
        internal BastionHostStack(Construct scope, string id, Vpc vpc, IStackProps props = null) : base(scope, id, props)
        {
            var context = this.Node.TryGetContext("dev") as Dictionary<String, Object>;

            var image = MachineImage.LatestAmazonLinux(new AmazonLinuxImageProps() 
            {
                Generation = AmazonLinuxGeneration.AMAZON_LINUX_2,
                Edition = AmazonLinuxEdition.STANDARD,
                Storage = AmazonLinuxStorage.GENERAL_PURPOSE,
                Virtualization = AmazonLinuxVirt.HVM
            });

            var sg = new SecurityGroup(this, "BastionHostSecurityGroupId", new SecurityGroupProps 
            {
                Vpc = vpc,
                SecurityGroupName = "BastionHost",
                AllowAllOutbound = true
            });

            sg.Connections.AllowFromAnyIpv4(new Port(new PortProps() 
            { 
                StringRepresentation = "22",
                Protocol = Protocol.TCP, 
                FromPort = 22,
                ToPort = 22
            }), "Allow ssh access");

            Instance = new Instance_(this, "BastionHostId", new InstanceProps() 
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
                KeyName = context["bastionHostKey"] as String
            });
            
            var cfn = new CfnOutput(this, "BastionHostDnsOutputId", new CfnOutputProps 
            {
                ExportName = "DNS",
                Value = Instance.InstancePublicDnsName
            });

            // Instance.Role.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("AmazonSSMManagedInstanceCore"));
        }
    }
}
