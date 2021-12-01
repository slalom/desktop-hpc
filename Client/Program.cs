using System;
using Amazon.SQS;
using System.Threading.Tasks;
using Amazon.SQS.Model;

class Client
{
  static async Task Main(string[] args)
  {
    var sqs = new AmazonSQSClient();

    var output = await sqs.GetQueueUrlAsync("OutputQueue");
    var input = await sqs.GetQueueUrlAsync("InputQueue");
    long iStart = 1000000000000000001; // one quintillion one

    Console.WriteLine("Input number of large integers to factor, starting with one trillion one:");

    int count = Convert.ToInt32(Console.ReadLine());

    Console.WriteLine("Sending {0} numbers sent to cluster for factoring.", count);
    Console.WriteLine("From {0} to {1}", iStart, iStart + count);

    for (long i = iStart; i < iStart + count; i++)
    {
      await sqs.SendMessageAsync(input.QueueUrl, i.ToString());
    }

    Console.WriteLine("Results:");

    int finishedCount = 0;

    while (finishedCount < count)
    {
      var msg = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest
      {
        QueueUrl = output.QueueUrl,
        MaxNumberOfMessages = 1,
        WaitTimeSeconds = 3
      });

      if (msg.Messages.Count != 0)
      {
        Console.WriteLine(msg.Messages[0].Body);
        await sqs.DeleteMessageAsync(output.QueueUrl, msg.Messages[0].ReceiptHandle);
        finishedCount++;
      }

    }
    Console.WriteLine("Factoring complete, destroy cluster when done.");
  }
}

