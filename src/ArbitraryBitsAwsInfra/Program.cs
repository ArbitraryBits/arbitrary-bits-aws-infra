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

            var type = System.Environment.GetEnvironmentVariable("DEPLOY_TYPE");

            if(type == "DB") 
            {
                buildDb(app, env);
            } 
            else if (type == "KUBE") 
            {
                buildKubernetes(app, env);
            }
            else if (type == "WORKNODE") 
            {
                buildWorkingNode(app, env);
            }
            else 
            {
                Console.WriteLine(args);
                throw new Exception($"Unknown DEPLOY_TYPE={type}"); 
            }
            
            app.Synth();
        }

        static void buildKubernetes(App app, Amazon.CDK.Environment env) {
            Amazon.CDK.Tags.Of(app).Add("Type", "Kubernetes");
            var kube = new KubernetesStack(app, "KubernetesStack", new StackProps { Env = env });
        }

        static void buildWorkingNode(App app, Amazon.CDK.Environment env) {
            Amazon.CDK.Tags.Of(app).Add("Type", "WorkingNode");
            
            var vpc = new ArbitraryBitsWorkingNodeVpc(app, "WorkingNodeVpcStack", new StackProps { Env = env });
            new ArbitraryBitsWorkingNode(app, "WorkingNodeStack", vpc.WorkingNodeVpc, new StackProps { Env = env });
        }

        static void buildDb(App app, Amazon.CDK.Environment env) {
            Amazon.CDK.Tags.Of(app).Add("Type", "DB");

            var dbVpc = new ArbitraryBitsDatabaseVpc(app, "DbVpcStack", new StackProps { Env = env });
            var dbPrereq = new ArbitraryBitsDatabasePrereq(app, "DbPrereqStack", dbVpc.Vpc, new StackProps { Env = env });
            var db = new ArbitraryBitsDatabaseFromSnapshot(app, "DbStack", dbVpc.Vpc, new StackProps { Env = env });
            
            var dbBastionHostVpc = new ArbitraryBitsDatabaseBastionHostVpc(app, "DbBastionHostVpcStack", new StackProps { Env = env });
            dbBastionHostVpc.Node.AddDependency(dbVpc);
            var dbBastionHost = new ArbitraryBitsDbBastionHost(app, "DbBastionHostStack", dbBastionHostVpc.BastionHostVpc, new StackProps { Env = env });
            dbBastionHost.Node.AddDependency(db);
            
            var iRacingCalendarDbSecrets = new IRacingCalendarDbSecrets(app, "IRacingCalendarDbSecretsStack", new StackProps { Env = env });
            iRacingCalendarDbSecrets.Node.AddDependency(db);

            var toDoDbSecrets = new ToDoDbSecrets(app, "ToDoDbSecretsStack", new StackProps { Env = env });
            toDoDbSecrets.Node.AddDependency(db);

            var ecsPeerConnection = new ToDoEcsServicePeerConnection(app, "EcsPeerStack", new StackProps { Env = env });
            ecsPeerConnection.Node.AddDependency(db);
        }
    }
}
