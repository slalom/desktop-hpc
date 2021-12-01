using Amazon.CDK;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.SQS;

public class Infrastructure : Stack
{
  static void Main(string[] args)
  {
    var app = new App();
    new Infrastructure(app, "HPCStack");
    app.Synth();
  }

  internal Infrastructure(
    Construct scope,
    string id,
    IStackProps props = null
  ) :
      base(scope, id, props)
  {
    var input =
      new Queue(this,
        "InputQueue",
        new QueueProps { QueueName = "InputQueue" });
    var output =
      new Queue(this,
        "OutputQueue",
        new QueueProps { QueueName = "OutputQueue" });
    var cluster =
      new Cluster(this,
        "HPCCluster",
        new ClusterProps { ClusterName = "HPCCluster" });
    var service =
        new QueueProcessingFargateService(this,
          "HPCService",
          new QueueProcessingFargateServiceProps
          {
            ServiceName = "HPCService",
            Cluster = cluster,
            MinScalingCapacity = 1,
            MaxScalingCapacity = 5,
            Image = ContainerImage.FromAsset("./worker"),
            MemoryLimitMiB = 2048,
            Queue = input
          });
    output.GrantSendMessages(service.TaskDefinition.TaskRole);
  }
}

