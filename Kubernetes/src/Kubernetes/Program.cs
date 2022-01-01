using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kubernetes
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

            var kube = new KubernetesStack(app, "KubernetesStack", new StackProps { Env = env });

            app.Synth();
        }
    }
}
