// See https://aka.ms/new-console-template for more information

using RabbitMQ.Client;

Console.WriteLine("Hello, World!");

var factory = new ConnectionFactory()
{
    Password = "password",
    UserName = "tester",
    Port = 5672,
    HostName = "localhost"
};
var connection = factory.CreateConnection();

ThreadPool.SetMinThreads(2, 2);

var tasks = Enumerable.Range(1, 100)
    .Select(i => Task.Run(() =>
{
    WriteThreadPoolStatus(i);
    Console.WriteLine($"Creating Model #{i}...");
    var model =  connection.CreateModel();
    Console.WriteLine($"Model #{i} Ready");
    return model;
})).ToArray();

// If you mark all the CreateModel Tasks as LongRunning, they don't 
//   block ThreadPool threads and it fixes congestion.
// var tasks = Enumerable.Range(1, 100)
//     .Select(i => Task.Factory.StartNew(() =>
//     {
//         WriteThreadPoolStatus(i);
//         Console.WriteLine($"Creating Model #{i}...");
//         var model =  connection.CreateModel();
//         Console.WriteLine($"Model #{i} Ready");
//         return model;
//     }, TaskCreationOptions.LongRunning)).ToArray();

Task.WaitAll(tasks.ToArray<Task>());

WriteThreadPoolStatus(0);

void WriteThreadPoolStatus(int id)
{
    Console.WriteLine($"Id[{id}]: ThreadCount={ThreadPool.ThreadCount}");
    Console.WriteLine($"Id[{id}]: WorkItems: Pending={ThreadPool.PendingWorkItemCount}, Completed={ThreadPool.CompletedWorkItemCount}");
    ThreadPool.GetMinThreads(out var minWorkerCount, out var _);
    ThreadPool.GetMaxThreads(out var maxWorkerCount, out var _);
    Console.WriteLine($"Id[{id}]: Configuration: Min={minWorkerCount}, Max={maxWorkerCount}");
    ThreadPool.GetAvailableThreads(out var availableWorkerCount, out var _);
    var grinchThreads = maxWorkerCount - minWorkerCount;
    var santaThreads = Math.Max(0, availableWorkerCount - grinchThreads);
    Console.WriteLine($"Id[{id}]: AvailableThreads: FastProvision={santaThreads}, SlowProvision={availableWorkerCount - santaThreads}");
}