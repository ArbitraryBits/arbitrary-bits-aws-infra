using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

namespace ArbitraryBitsAwsInfra
{
    public class ArbitraryBitsWorkingNode : Stack
    {
        internal ArbitraryBitsWorkingNode(Construct scope, string id, Vpc vpc, IStackProps props = null) : base(scope, id, props)
        {
            var context = this.Node.TryGetContext("settings") as Dictionary<String, Object>;

            var image = MachineImage.GenericLinux(new Dictionary<string, string>() 
            { 
                { "us-east-1", "ami-0d4c664d2c7345cf1" } // Ubuntu Server 20.04 LTS (HVM), SSD Volume Type 
            });

            var sg = new SecurityGroup(this, "WorkingNodeSecurityGroupId", new SecurityGroupProps 
            {
                Vpc = vpc,
                SecurityGroupName = "WorkingNode",
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

            sg.Connections.AllowFromAnyIpv4(
                new Port(new PortProps() 
                { 
                    StringRepresentation = "5000",
                    Protocol = Protocol.TCP, 
                    FromPort = 5000,
                    ToPort = 5000,
                }),
                "Allow internet traffic to 5000 port"
            );

            var userData = UserData.ForLinux(new LinuxUserDataOptions());
            userData.AddCommands(
                @"sudo apt-get update && \
                    sudo apt-get install -y \
                    ca-certificates \
                    curl \
                    gnupg \
                    lsb-release && \
                    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg &&
                    echo \
                    ""deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu \
                    $(lsb_release -cs) stable"" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null && \
                    sudo apt-get update && \
                    sudo apt-get install -y docker-ce docker-ce-cli containerd.io && \
                    sudo usermod -a -G docker ubuntu"
            );


            var instance = new Instance_(this, "WorkingNodeId", new InstanceProps() 
            {
                InstanceType = new InstanceType("t3.small"),
                InstanceName = "WorkingNode",
                MachineImage = image,
                Vpc = vpc,
                VpcSubnets = new SubnetSelection() 
                { 
                    SubnetType = SubnetType.PUBLIC
                },
                SecurityGroup = sg,
                KeyName = context["dbBastionHostKey"] as String,
                UserData = userData,
                BlockDevices = new [] { 
                        new BlockDevice {
                        DeviceName = "/dev/sda1",
                        Volume = BlockDeviceVolume.Ebs(10, new EbsDeviceOptions() { 
                            DeleteOnTermination = true
                        })
                    }
                }
            });
            
            var cfn = new CfnOutput(this, "WorkingNodeDnsOutputId", new CfnOutputProps 
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

            var sshCommand = String.Format("ssh -i ~/.ssh/{0}.pem -L 5432:{1}:5432 ubuntu@{2}", 
                    context["dbBastionHostKey"] as string,
                    Fn.ImportValue("DbInstanceEndpointAddress"), 
                    instance.InstancePublicDnsName);

            new CfnOutput(this, "WorkingNodeSshCommandOutputId", new CfnOutputProps
            {
                Value = sshCommand,
                Description = "WorkingNode endpoint adress"
            });
        }
    }
}