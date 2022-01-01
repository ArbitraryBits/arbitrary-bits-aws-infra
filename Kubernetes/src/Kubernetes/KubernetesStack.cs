using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.EKS;

namespace Kubernetes
{
    public class KubernetesStack : Stack
    {
        internal KubernetesStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
                        // create vpc
            var clusterVpc = new Vpc(this, "ArbitraryBitsDatabaseVpcId", new VpcProps {
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
            
            cluster.AddNodegroupCapacity("custom-node-group", new NodegroupOptions {
                InstanceTypes = new [] { InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.SMALL) },
                MinSize = 1,
                MaxSize = 1,
                DiskSize = 10,
                AmiType = NodegroupAmiType.AL2_X86_64,
                Subnets = new SubnetSelection { SubnetType = SubnetType.PUBLIC }
            });
        }
    }
}
