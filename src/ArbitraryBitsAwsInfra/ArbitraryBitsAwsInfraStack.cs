using Amazon.CDK;
using Amazon.CDK.AWS.EC2;

namespace ArbitraryBitsAwsInfra
{
    public class ArbitraryBitsAwsInfraStack : Stack
    {
        internal ArbitraryBitsAwsInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // create vpc
            var vpc = new Vpc(this, "ExampleVPC", new VpcProps {
                Cidr = "10.0.0.0/16",
                MaxAzs = 1,
                NatGateways = 0,
                SubnetConfiguration = new [] {
                    new SubnetConfiguration() {
                        Name = "Private subnet",
                        SubnetType = SubnetType.PUBLIC,
                        CidrMask = 24
                    }
                }
            });

            // add tag
            Amazon.CDK.Tags.Of(vpc).Add("Owner", "Reloni");

            var cfn = new CfnOutput(this, "ExampleOutput", new CfnOutputProps {
                ExportName = "VPCId",
                Value = vpc.VpcId
            });
        }
    }
}
