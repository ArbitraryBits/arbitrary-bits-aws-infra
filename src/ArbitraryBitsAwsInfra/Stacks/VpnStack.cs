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
            var clientCert = Certificate.FromCertificateArn(this, "ClientCertificate", "");
            var serverCert = Certificate.FromCertificateArn(this, "ServerCertificate", "");

            var logGroup = new LogGroup(this, "VpnLogGroup", new LogGroupProps() {
                Retention = RetentionDays.FIVE_DAYS,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            var logStream = logGroup.AddStream("VpnLogStream");

            var endpoint = new ClientVpnEndpoint(this, "", new ClientVpnEndpointProps() {
                LogStream = logStream,
                LogGroup = logGroup,
                ClientCertificateArn = "",
                ServerCertificateArn = "",
                Vpc = vpc,
                SplitTunnel = true,
                Cidr = "",
                VpcSubnets = new SubnetSelection() {
                    OnePerAz = true,
                    SubnetType = SubnetType.ISOLATED
                }
            });
            // var endpoint = new CfnClientVpnEndpoint(this, "VpnEndpoint", new CfnClientVpnEndpointProps() {
            //     AuthenticationOptions = CfnClientVpnEndpoint.CertificateAuthenticationRequestProperty
            // });
        }
    }
}
