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
            new ArbitraryBitsAwsInfraStack(app, "ArbitraryBitsAwsInfraStack", new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = "437377620726",
                    Region = "us-east-1",
                }
            });

            app.Synth();
        }
    }
}
