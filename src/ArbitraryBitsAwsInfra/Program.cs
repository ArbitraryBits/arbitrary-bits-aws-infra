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
            var db = new ArbitraryBitsDatabase(app, "DbStack", dbVpc.Vpc, new StackProps { Env = env });
            
            var dbBastionHostVpc = new ArbitraryBitsDatabaseBastionHostVpc(app, "DbBastionHostVpcStack", new StackProps { Env = env });
            dbBastionHostVpc.Node.AddDependency(dbVpc);
            var dbBastionHost = new ArbitraryBitsDbBastionHost(app, "DbBastionHostStack", dbBastionHostVpc.BastionHostVpc, new StackProps { Env = env });
            dbBastionHost.Node.AddDependency(db);
            
            // var iRacingCalendarDbSecrets = new IRacingCalendarDbSecrets(app, "IRacingCalendarDbSecretsStack", db.DbInstance, dbVpc.Vpc, new StackProps { Env = env });
            // iRacingCalendarDbSecrets.Node.AddDependency(db);

            app.Synth();
        }
    }
}
