using System;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using System.Collections.Generic;
using Amazon.CDK.AWS.Route53;

namespace ArbitraryBitsAwsInfra
{
    public class ArbitraryBitsWorkingNodeVpc : Stack
    {
        internal Vpc WorkingNodeVpc { get; set; }
        internal ArbitraryBitsWorkingNodeVpc(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var context = this.Node.TryGetContext("settings") as Dictionary<String, Object>;

            // create vpc
            WorkingNodeVpc = new Vpc(this, "ArbitraryBitsWorkingNodeVpcId", new VpcProps {
                Cidr = "10.3.0.0/16",
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

            Amazon.CDK.Tags.Of(WorkingNodeVpc).Add("Type", "AB-WorkingNode-VPC");
            
            var cfn = new CfnOutput(this, "VpcIdOutput", new CfnOutputProps {
                ExportName = "ArbitraryBitsWorkingNodeVpccId",
                Value = WorkingNodeVpc.VpcId
            });         

            var dbVpc = Vpc.FromLookup(this, "ArbitratyBitsDbVpcId", new VpcLookupOptions() {
                Tags = new Dictionary<string, string>() { { "Type", "AB-DB-VPC" } }
            });

            var peeringConnection = new CfnVPCPeeringConnection(this, "ArbitraryBitsWorkingNodeVpcPeeringConnectionId", new CfnVPCPeeringConnectionProps() {
                PeerVpcId = dbVpc.VpcId,
                VpcId = WorkingNodeVpc.VpcId
            });
            
            // routes
            var routeIndex = 1;
            foreach (var workingNodeSubnet in WorkingNodeVpc.PublicSubnets)
            {
                foreach(var dbSubnet in dbVpc.IsolatedSubnets) 
                {
                    new CfnRoute(this, string.Format("WorkingNodeToDBRoute-{0}", routeIndex), new CfnRouteProps() {
                        DestinationCidrBlock = dbSubnet.Ipv4CidrBlock,
                        VpcPeeringConnectionId = peeringConnection.Ref,
                        RouteTableId = workingNodeSubnet.RouteTable.RouteTableId
                    });

                    new CfnRoute(this, string.Format("DBToWorkingNodeRoute-{0}", routeIndex), new CfnRouteProps() {
                        DestinationCidrBlock = workingNodeSubnet.Ipv4CidrBlock,
                        VpcPeeringConnectionId = peeringConnection.Ref,
                        RouteTableId = dbSubnet.RouteTable.RouteTableId
                    });

                    routeIndex += 1;
                }
            }

            // setup hosted zone with db host alias
            var hostedZone = new PrivateHostedZone(this, "ArbitraryBitsWorkingNodePrivateHostedZoneId", new PrivateHostedZoneProps() {
                Vpc = WorkingNodeVpc,
                ZoneName = "arbitrarybits.com"
            }); 

            new CnameRecord(this, "ArbitraryBitsWorkingNodePrivateHostedZoneDbCnameRecordId", new CnameRecordProps() {
                Zone = hostedZone,
                RecordName = "db.arbitrarybits.com",
                DomainName = context["dbEndpointAdress"] as String
            });
        }
    }
}
