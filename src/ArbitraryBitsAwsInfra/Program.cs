using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArbitraryBitsAwsInfra
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            Amazon.CDK.Tags.Of(app).Add("AppName", "Example");

            var context = app.Node.TryGetContext("dev") as Dictionary<String, Object>;
            
            new ArbitraryBitsAwsInfraStack(app, "ArbitraryBitsAwsInfraStack", new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = context["account"] as String,
                    Region = context["region"] as String,
                }
            });

            app.Synth();
        }
    }
}
