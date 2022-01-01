using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.Logs;

namespace ArbitraryBitsAwsInfra
{
    public class VpnStack : Stack
    {
        internal Vpc Vpc { get; set; }
        internal VpnStack(Construct scope, string id, Vpc vpc, IStackProps props = null) : base(scope, id, props)
        {
            var logGroup = new LogGroup(this, "VpnLogGroup", new LogGroupProps() {
                Retention = RetentionDays.FIVE_DAYS,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            var logStream = logGroup.AddStream("VpnLogStream");

            var endpoint = new ClientVpnEndpoint(this, "ExampleVpn", new ClientVpnEndpointProps() {
                LogStream = logStream,
                LogGroup = logGroup,
                ClientCertificateArn = "arn:aws:acm:us-east-1:437377620726:certificate/00a98b07-b37a-4002-9a43-9ce9998c278c",
                ServerCertificateArn = "arn:aws:acm:us-east-1:437377620726:certificate/3ccfd1f2-ebeb-405c-b290-0921053a125e",
                Vpc = vpc,
                SplitTunnel = true,
                Cidr = "10.1.0.0/22",
                VpcSubnets = new SubnetSelection() {
                    OnePerAz = true,
                    SubnetType = SubnetType.ISOLATED
                }
            });

            Amazon.CDK.Tags.Of(logGroup).Add("Stack", "VpnStack");
            Amazon.CDK.Tags.Of(endpoint).Add("Stack", "VpnStack");
        }
    }
}
