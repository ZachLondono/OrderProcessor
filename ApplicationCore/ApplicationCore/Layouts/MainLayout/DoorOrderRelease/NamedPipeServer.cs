using System.Collections.Concurrent;
using System.IO.Pipes;

namespace ApplicationCore.Layouts.MainLayout.DoorOrderRelease;

public class NamedPipeServer {

    private bool listening = false;

    public event OnMessageReceived? MessageReceived;
    public delegate void OnMessageReceived(PipeMessage message);

    public void Start() {

        listening = true;

        int i;
        Thread[] servers = new Thread[10];

        for (i = 0; i < 10; i++) {
            servers[i] = new Thread(ServerThread);
            servers[i].Start();
        }

        Thread.Sleep(250);
        while (listening) {

            for (int j = 0; j < 10; j++) {

                if (servers[j] == null) {
                    continue;
                }

                if (!servers[j].Join(250)) {
                    continue;
                    //i--;    // decrement the thread watch count
                }

                if (listening) {
                    servers[j] = null;
                    servers[j] = new Thread(ServerThread);
                    servers[j].Start();
                }
            }
        }

        foreach (Thread t in servers) {
            t.Join();
        }

    }

    public void Stop() => listening = false;

    public void ServerThread(object data) {

        using var pipeServer = new NamedPipeServerStream("MDFDoorPipe", PipeDirection.InOut, 10, PipeTransmissionMode.Message);

        int threadId = Environment.CurrentManagedThreadId;

        bool isConnected = false;
        var request = pipeServer.BeginWaitForConnection((a) => {
            isConnected = true;
        }, null);

        while (listening && !isConnected) { }

        if (!isConnected) {
            pipeServer.Close();
            return;
        }

        try {


            List<byte> intext = new List<byte>();
            do {

                if (!listening) break;

                byte[] x = new byte[1024 * 16];
                int read = 0;
                read = pipeServer.Read(x);
                Array.Resize(ref x, read);
                intext.AddRange(x);

            } while (!pipeServer.IsMessageComplete);

            string receivedText = System.Text.Encoding.UTF8.GetString(intext.ToArray());

            var msgParts = receivedText.Split(';');
            MessageReceived?.Invoke(new(msgParts[0], msgParts[1], msgParts[2]));

            string sentText = "OK";
            pipeServer.Write(System.Text.Encoding.UTF8.GetBytes(sentText));

        }
        // Catch the IOException that is raised if the pipe is broken
        // or disconnected.
        catch (IOException e) {
            Console.WriteLine("ERROR: {0}", e.Message);
        }
        //pipeServer.WaitForPipeDrain();
        pipeServer.Close();
    }

    public record PipeMessage(string Type, string MessageA, string MessageB);

}

