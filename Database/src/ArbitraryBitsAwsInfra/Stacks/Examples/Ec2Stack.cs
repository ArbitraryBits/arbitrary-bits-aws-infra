using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

namespace ArbitraryBitsAwsInfra
{
    public class Ec2Stack : Stack
    {
        internal Instance_ Instance { get; set; }
        internal Ec2Stack(Construct scope, string id, Vpc vpc, IStackProps props = null) : base(scope, id, props)
        {
            var context = this.Node.TryGetContext("dev") as Dictionary<String, Object>;

            var image = MachineImage.LatestAmazonLinux(new AmazonLinuxImageProps() 
            {
                Generation = AmazonLinuxGeneration.AMAZON_LINUX_2,
                Edition = AmazonLinuxEdition.STANDARD,
                Storage = AmazonLinuxStorage.GENERAL_PURPOSE,
                Virtualization = AmazonLinuxVirt.HVM
            });

            var userData = UserData.ForLinux(new LinuxUserDataOptions());
            userData.AddCommands(
                "sudo yum update -y",
                "sudo yum install -y httpd php mysql",
                "IP=$(curl -s http://169.254.169.254/latest/meta-data/local-ipv4)",
                "echo \"<html><head><title>Modern Web App</title><style>body {margin-top: 40px;background-color: #333;}</style></head><body><div style=color:white;text-align:center><h1 style='font-size:7vw;'>Modern Web App</h1><p>Congratulations! Your Web Server is Online.</p><small>Pages served from $IP</small></div></body></html>\" >> /var/www/html/index.html",
                "sudo chkconfig httpd on",
                "sudo service httpd start",
                "sudo yum install -y mysql57 curl"
            );

            var sg = new SecurityGroup(this, "EC2InstanceSecurityGroup", new SecurityGroupProps 
            {
                Vpc = vpc,
                SecurityGroupName = "EC2-Instance-SG",
                AllowAllOutbound = true
            });
            sg.Connections.AllowFromAnyIpv4(new Port(new PortProps() 
            { 
                StringRepresentation = "80",
                Protocol = Protocol.TCP, 
                FromPort = 80,
                ToPort = 80
            }), "Allow all access to 80 port");

            Instance = new Instance_(this, "ExampleServer", new InstanceProps() 
            {
                InstanceType = new InstanceType("t2.micro"),
                InstanceName = "ExampleInstance",
                MachineImage = image,
                Vpc = vpc,
                VpcSubnets = new SubnetSelection() 
                { 
                    SubnetType = SubnetType.PUBLIC, 
                    AvailabilityZones = new [] { context["mainAvailabilityZone"] as String }
                },
                UserData = userData,
                SecurityGroup = sg
            });
            
            var cfn = new CfnOutput(this, "ServerUrlOutput", new CfnOutputProps 
            {
                ExportName = "ServerUrl",
                Value = string.Format("http://{0}", Instance.InstancePublicDnsName)
            });

            Instance.Role.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("AmazonSSMManagedInstanceCore"));
        }
    }
}
