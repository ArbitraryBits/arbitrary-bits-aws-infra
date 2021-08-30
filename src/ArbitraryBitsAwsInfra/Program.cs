using Amazon.CDK;
using System;
using System.Collections.Generic;

namespace ArbitraryBitsAwsInfra
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();

            var context = app.Node.TryGetContext("dev") as Dictionary<String, Object>;
            var env = new Amazon.CDK.Environment
            {
                Account = context["account"] as String,
                Region = context["region"] as String,
            };

            // tag all app items
            Amazon.CDK.Tags.Of(app).Add("Creator", "CDK");

            var dbVpc = new ArbitraryBitsDatabaseVpc(app, "DbVpcStack", new StackProps { Env = env });
            var rds = new ArbitraryBitsDatabase(app, "DbStack", dbVpc.Vpc, new StackProps { Env = env });

            // var vpc = new VpcStack(app, "Vpc", new StackProps { Env = env });
            // var bastionHost = new BastionHostStack(app, "BastionHost", vpc.Vpc, new StackProps { Env = env });
            // var ec2 = new Ec2Stack(app, "EC2", vpc.Vpc, new StackProps { Env = env });
            // var rds = new RdsStack(app, "RDS", vpc.Vpc, ec2.Instance, new StackProps { Env = env });
            // var vpn = new VpnStack(app, "Vpn", vpc.Vpc, new StackProps { Env = env });

            app.Synth();
        }
    }
}
