using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using System.Collections.Generic;

namespace ArbitraryBitsAwsInfra
{
    public class ArbitraryBitsDatabaseBastionHostVpc : Stack
    {
        internal Vpc BastionHostVpc { get; set; }
        internal ArbitraryBitsDatabaseBastionHostVpc(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // create vpc
            BastionHostVpc = new Vpc(this, "ArbitraryBitsDatabaseBastionHostVpcId", new VpcProps {
                Cidr = "10.1.0.0/16",
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

            Amazon.CDK.Tags.Of(BastionHostVpc).Add("Type", "AB-DB-BastionHost-VPC");
            
            var cfn = new CfnOutput(this, "VpcIdOutput", new CfnOutputProps {
                ExportName = "ArbitraryBitsDatabaseBastionHostVpcId",
                Value = BastionHostVpc.VpcId
            });         

            var dbVpc = Vpc.FromLookup(this, "ArbitratyBitsDbVpcId", new VpcLookupOptions() {
                Tags = new Dictionary<string, string>() { { "Type", "AB-DB-VPC" } }
            });

            var peeringConnection = new CfnVPCPeeringConnection(this, "ArbitraryBitsDatabaseBastionHostVpcPeeringConnectionId", new CfnVPCPeeringConnectionProps() {
                PeerVpcId = dbVpc.VpcId,
                VpcId = BastionHostVpc.VpcId
            });
            
            // routes
            var routeIndex = 1;
            foreach (var bastionHostSubnet in BastionHostVpc.PublicSubnets)
            {
                foreach(var dbSubnet in dbVpc.IsolatedSubnets) 
                {
                    new CfnRoute(this, string.Format("BastionHostToDBRoute-{0}", routeIndex), new CfnRouteProps() {
                        DestinationCidrBlock = dbSubnet.Ipv4CidrBlock,
                        VpcPeeringConnectionId = peeringConnection.Ref,
                        RouteTableId = bastionHostSubnet.RouteTable.RouteTableId
                    });

                    new CfnRoute(this, string.Format("DBToBastionHostRoute-{0}", routeIndex), new CfnRouteProps() {
                        DestinationCidrBlock = bastionHostSubnet.Ipv4CidrBlock,
                        VpcPeeringConnectionId = peeringConnection.Ref,
                        RouteTableId = dbSubnet.RouteTable.RouteTableId
                    });

                    routeIndex += 1;
                }
            }
        }
    }
}
