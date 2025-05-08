using Newtonsoft.Json.Linq;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CSharpSDK
{
    public class WebSocket
    {
        private ClientWebSocket webSocket;
        private CancellationTokenSource cancellationTokenSource;
        private Thread receiveThread;
        private Thread heartbeatThread;
        private string webSocketUrl;
        private const int HeartbeatInterval = 60000; // 心跳间隔，单位毫秒
        private const int ReconnectInterval = 5000; // 重连间隔，单位毫秒
        private bool isReconnecting = false;

        // 定义消息接收事件
        public event Action<string> MessageReceived;

        public bool Connect(string webSocketUrl, Action<string> messageReceivedCallback = null)
        {
            if (messageReceivedCallback != null)
            {
                MessageReceived = messageReceivedCallback;
            }
            this.webSocketUrl = webSocketUrl;
            try
            {
                // 关闭之前的连接和线程
                Close();

                webSocket = new ClientWebSocket();
                cancellationTokenSource = new CancellationTokenSource();
                var connectTask = webSocket.ConnectAsync(new Uri(webSocketUrl), cancellationTokenSource.Token);
                bool res = connectTask.Wait(5000, cancellationTokenSource.Token);
                if (!res)
                {
                    return false;
                }

                receiveThread = new Thread(ReceiveMessages);
                receiveThread.Start();

                heartbeatThread = new Thread(SendHeartbeat);
                heartbeatThread.Start();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool SendMsg(string message)
        {
            if (webSocket != null && webSocket.State == WebSocketState.Open)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    var sendTask = webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationTokenSource.Token);
                    sendTask.Wait(cancellationTokenSource.Token);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ReceiveMessages()
        {
            byte[] buffer = new byte[1024];
            var fullMessage = new System.Text.StringBuilder();
            while (webSocket.State == WebSocketState.Open && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                var receiveTask = webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);
                try
                {
                    receiveTask.Wait(cancellationTokenSource.Token);
                    var result = receiveTask.Result;
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string messagePart = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        fullMessage.Append(messagePart);
                        if (result.EndOfMessage)
                        {
                            // 消息接收完毕，触发消息接收事件
                            MessageReceived?.Invoke(fullMessage.ToString());
                            fullMessage.Clear();
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!isReconnecting)
                    {
                        StartReconnect();
                    }
                    return;
                }
            }
            if (!isReconnecting)
            {
                StartReconnect();
            }
        }

        public void SendHeartbeat()
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    Thread.Sleep(HeartbeatInterval);
                    if (webSocket != null && webSocket.State == WebSocketState.Open)
                    {
                        // 这里假设 Common.sendMsg 是一个静态方法，需要确保它的实现是正确的
                        Common.sendMsg("heart", "heart", "", new JObject(), new JObject(), false);
                    }
                }
                catch (Exception ex)
                {
                    if (!cancellationTokenSource.Token.IsCancellationRequested && !isReconnecting)
                    {
                        StartReconnect();
                    }
                }
            }
        }

        private void StartReconnect()
        {
            if (isReconnecting) return;
            isReconnecting = true;
            try
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (Connect(webSocketUrl, this.MessageReceived))
                    {
                        break;
                    }
                    Thread.Sleep(ReconnectInterval);
                }
            }
            finally
            {
                isReconnecting = false;
            }
        }

        public void Close()
        {
            if (webSocket != null && webSocket.State == WebSocketState.Open)
            {
                var task = webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "关闭链接", CancellationToken.None);
                task.Wait(5000);
                cancellationTokenSource.Cancel();
                if (receiveThread != null && receiveThread.IsAlive)
                {
                    receiveThread.Abort();
                    receiveThread = null;
                }
                if (heartbeatThread != null && heartbeatThread.IsAlive)
                {
                    heartbeatThread.Abort();
                    heartbeatThread = null;
                }
                webSocket.Dispose();
            }
        }
    }
}