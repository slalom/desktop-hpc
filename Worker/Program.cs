using System;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;

public class Worker
{
  static async Task Main(string[] args)
  {
    var sqs = new AmazonSQSClient();
    var input = await sqs.GetQueueUrlAsync("InputQueue");
    var output = await sqs.GetQueueUrlAsync("OutputQueue");
    Console.WriteLine("waiting for numbers to factor on {0}", input.QueueUrl);
    do
    {
      var msg = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest
      {
        QueueUrl = input.QueueUrl,
        MaxNumberOfMessages = 1,
        WaitTimeSeconds = 3
      });

      if (msg.Messages.Count != 0)
      {
        long n2 = Convert.ToInt64(msg.Messages[0].Body);
        Console.WriteLine("received input: {0}", n2);
        string factorsString = GetFactors(n2);
        Console.WriteLine("factors are: {0}", factorsString);
        await sqs.SendMessageAsync(output.QueueUrl, factorsString);
        await sqs.DeleteMessageAsync(input.QueueUrl, msg.Messages[0].ReceiptHandle);
      }
    } while (true);
  }

  static string GetFactors(long n)
  {
    string s = n.ToString() + " = ";
    List<long> factors = new List<long>();
    while (!(n % 2 > 0))
    {
      n = n / 2;
      factors.Add(2);
    }
    for (long i = 3; i <= (long)Math.Sqrt(n); i += 2)
    {
      while (n % i == 0)
      {
        factors.Add(i);
        n = n / i;
      }
    }
    if (n > 2)
    {
      factors.Add(n);
    }
    return s + string.Join(" x ", factors);
  }
}

