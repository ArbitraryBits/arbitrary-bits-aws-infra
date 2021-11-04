using System;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using System.Collections.Generic;

namespace ArbitraryBitsAwsInfra
{
    public class ToDoEcsServicePeerConnection : Stack
    {
        internal ToDoEcsServicePeerConnection(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var context = this.Node.TryGetContext("settings") as Dictionary<String, Object>;
            
            var ecsVpc = Vpc.FromLookup(this, "EcsVpcId", new VpcLookupOptions() {
                Tags = new Dictionary<string, string>() { { "Type", "ECS-VPC" } }
            });

            var dbVpc = Vpc.FromLookup(this, "ArbitratyBitsDbVpcId", new VpcLookupOptions() {
                Tags = new Dictionary<string, string>() { { "Type", "AB-DB-VPC" } }
            });

            var peeringConnection = new CfnVPCPeeringConnection(this, "ArbitraryBitsDatabaseEcsVpcPeeringConnectionId", new CfnVPCPeeringConnectionProps() {
                PeerVpcId = dbVpc.VpcId,
                VpcId = ecsVpc.VpcId
            });
            
            // routes
            var routeIndex = 1;
            foreach (var ecsSubnet in ecsVpc.PublicSubnets)
            {
                foreach(var dbSubnet in dbVpc.IsolatedSubnets) 
                {
                    new CfnRoute(this, string.Format("EcsToDBRoute-{0}", routeIndex), new CfnRouteProps() {
                        DestinationCidrBlock = dbSubnet.Ipv4CidrBlock,
                        VpcPeeringConnectionId = peeringConnection.Ref,
                        RouteTableId = ecsSubnet.RouteTable.RouteTableId
                    });

                    new CfnRoute(this, string.Format("DBToEcsRoute-{0}", routeIndex), new CfnRouteProps() {
                        DestinationCidrBlock = ecsSubnet.Ipv4CidrBlock,
                        VpcPeeringConnectionId = peeringConnection.Ref,
                        RouteTableId = dbSubnet.RouteTable.RouteTableId
                    });

                    routeIndex += 1;
                }
            }

            // var dbSecurityGroup = SecurityGroup.FromLookup(
            //     this, 
            //     "ArbitraryBitsBastionHostDatabaseSecurityGroupId", 
            //     context["dbInstanceSecurityGroupId"] as String);

            // var ecsInstanceSecurityGroup = SecurityGroup.FromLookup(
            //     this, 
            //     "EcsInstanceDatabaseSecurityGroupId", 
            //     context["ecsInstanceSecurityGroupId"] as String);

            // dbSecurityGroup.Connections.AllowFrom(ecsInstanceSecurityGroup, new Port(new PortProps() 
            // { 
            //     StringRepresentation = "5432",
            //     Protocol = Protocol.TCP, 
            //     FromPort = 5432,
            //     ToPort = 5432,
            // }), "Allow connections from Ecs Instance to DB");
        }
    }
}
