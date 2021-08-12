using Amazon.CDK;
using Amazon.CDK.AWS.EC2;

namespace ArbitraryBitsAwsInfra
{
    public class VpcStack : Stack
    {
        internal Vpc Vpc { get; set; }
        internal VpcStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // create vpc
            Vpc = new Vpc(this, "ExampleVPC", new VpcProps {
                Cidr = "10.0.0.0/16",
                MaxAzs = 1,
                NatGateways = 0,
                SubnetConfiguration = new [] {
                    new SubnetConfiguration() {
                        Name = "public-subnet",
                        SubnetType = SubnetType.PUBLIC,
                        CidrMask = 24
                    },
                    new SubnetConfiguration() {
                        Name = "isolated-subnet",
                        SubnetType = SubnetType.ISOLATED,
                        CidrMask = 24
                    }
                }
            });

            // add example tag
            Amazon.CDK.Tags.Of(Vpc).Add("ExampleProperty", "Value");

            var cfn = new CfnOutput(this, "VpcIdOutput", new CfnOutputProps {
                ExportName = "VPCId",
                Value = Vpc.VpcId
            });
        }
    }
}
