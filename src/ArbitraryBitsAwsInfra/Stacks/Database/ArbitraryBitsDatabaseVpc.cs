using Amazon.CDK;
using Amazon.CDK.AWS.EC2;

namespace ArbitraryBitsAwsInfra
{
    public class ArbitraryBitsDatabaseVpc : Stack
    {
        internal Vpc Vpc { get; set; }
        internal ArbitraryBitsDatabaseVpc(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // create vpc
            Vpc = new Vpc(this, "ArbitraryBitsDatabaseVpcId", new VpcProps {
                Cidr = "10.0.0.0/16",
                MaxAzs = 2,
                NatGateways = 0,
                SubnetConfiguration = new [] {
                    new SubnetConfiguration() {
                        Name = "isolated",
                        SubnetType = SubnetType.PUBLIC,
                        CidrMask = 24
                    }
                }
            });

            Amazon.CDK.Tags.Of(Vpc).Add("Type", "AB-DB-VPC");

            var cfn = new CfnOutput(this, "VpcIdOutput", new CfnOutputProps {
                ExportName = "VpcId",
                Value = Vpc.VpcId
            });
        }
    }
}
