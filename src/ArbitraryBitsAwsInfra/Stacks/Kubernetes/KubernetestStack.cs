using System;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.EKS;
using System.Collections.Generic;
using Amazon.CDK.AWS.Route53;

namespace ArbitraryBitsAwsInfra
{
    public class KubernetesStack : Stack
    {
        internal KubernetesStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var context = this.Node.TryGetContext("settings") as Dictionary<String, Object>;

            // create vpc
            var clusterVpc = new Vpc(this, "ArbitraryBitsKubernetesVpcId", new VpcProps {
                Cidr = "10.2.0.0/16",
                MaxAzs = 2,
                NatGateways = 0,
                SubnetConfiguration = new [] {
                    new SubnetConfiguration() {
                        Name = "public",
                        SubnetType = SubnetType.PUBLIC,
                        CidrMask = 24
                    }
                }
            });

            Amazon.CDK.Tags.Of(clusterVpc).Add("Type", "AB-Kube-VPC");
            
            var cfn = new CfnOutput(this, "VpcIdOutput", new CfnOutputProps {
                ExportName = "KubeVpcId",
                Value = clusterVpc.VpcId
            });

            // cluster
            Cluster cluster = new Cluster(this, "arbitrarybits-eks-cluster", new ClusterProps {
                ClusterName = "ArbitraryBitsKubernetes",
                Version = KubernetesVersion.V1_21,
                OutputConfigCommand = true,
                EndpointAccess = EndpointAccess.PUBLIC,
                DefaultCapacityType = DefaultCapacityType.EC2,
                DefaultCapacity = 0,
                DefaultCapacityInstance = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.SMALL),
                Vpc = clusterVpc,
                VpcSubnets = new [] { new SubnetSelection { SubnetType = SubnetType.PUBLIC } }
            });
            
            // node group
            cluster.AddNodegroupCapacity("node-group", new NodegroupOptions {
                InstanceTypes = new [] { InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.SMALL) },
                MinSize = 1,
                MaxSize = 1,
                DiskSize = 10,
                AmiType = NodegroupAmiType.AL2_X86_64,
                Subnets = new SubnetSelection { SubnetType = SubnetType.PUBLIC }
            });

            // setup peering connection betveen kube vpc and db vpc
            var dbVpc = Vpc.FromLookup(this, "ArbitratyBitsDbVpcId", new VpcLookupOptions() {
                Tags = new Dictionary<string, string>() { { "Type", "AB-DB-VPC" } }
            });

            var peeringConnection = new CfnVPCPeeringConnection(this, "ArbitraryBitsDatabaseKubernetesVpcPeeringConnectionId", new CfnVPCPeeringConnectionProps() {
                PeerVpcId = dbVpc.VpcId,
                VpcId = clusterVpc.VpcId
            });
            
            // routes
            var routeIndex = 1;
            foreach (var clusterSubnet in clusterVpc.PublicSubnets)
            {
                foreach(var dbSubnet in dbVpc.IsolatedSubnets) 
                {
                    new CfnRoute(this, string.Format("KubernetesToDBRoute-{0}", routeIndex), new CfnRouteProps() {
                        DestinationCidrBlock = dbSubnet.Ipv4CidrBlock,
                        VpcPeeringConnectionId = peeringConnection.Ref,
                        RouteTableId = clusterSubnet.RouteTable.RouteTableId
                    });

                    new CfnRoute(this, string.Format("DBToKubernetesRoute-{0}", routeIndex), new CfnRouteProps() {
                        DestinationCidrBlock = clusterSubnet.Ipv4CidrBlock,
                        VpcPeeringConnectionId = peeringConnection.Ref,
                        RouteTableId = dbSubnet.RouteTable.RouteTableId
                    });

                    routeIndex += 1;
                }
            }

            // setup db security group access from kube
            var dbSecurityGroup = SecurityGroup.FromLookup(
                this, 
                "ArbitraryBitsKubernetesDatabaseSecurityGroupId", 
                context["dbInstanceSecurityGroupId"] as String);

            dbSecurityGroup.AddIngressRule(
                Peer.Ipv4("10.2.0.0/16"), 
                new Port(new PortProps() { 
                    StringRepresentation = "5432",
                    Protocol = Protocol.TCP, 
                    FromPort = 5432,
                    ToPort = 5432,
                })
             );
        
            var hostedZone = new PrivateHostedZone(this, "ArbitraryBitsKubernetesPrivateHostedZoneId", new PrivateHostedZoneProps() {
                Vpc = clusterVpc,
                ZoneName = "arbitrarybits.com"
            }); 

            new CnameRecord(this, "ArbitraryBitsKubernetesPrivateHostedZoneDbCnameRecordId", new CnameRecordProps() {
                Zone = hostedZone,
                RecordName = "db.arbitrarybits.com",
                DomainName = context["dbEndpointAdress"] as String
            });
        }
    }
}