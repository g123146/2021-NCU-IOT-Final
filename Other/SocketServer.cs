using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//引入庫
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;
using Newtonsoft.Json;
using System.Linq;

public class SocketServer : MonoBehaviour
{
    //以下預設都是私有的成員
    Socket serverSocket; //伺服器端socket
    //List<Socket> socketlist = new List<Socket>();
    Dictionary<Socket, Thread> client2Tread = new Dictionary<Socket, Thread>();
    Socket UWBSocket; //客戶端socket
    IPEndPoint ipEnd; //偵聽埠
    string uwbStatusStr; //接收的字串
    float base1Dis = -1; 
    float base2Dis = -1; 
    float base3Dis = -1; 
    string sendStr; //傳送的字串
    byte[] recvData = new byte[1024]; //接收的資料，必須為位元組
    byte[] sendData = new byte[1024]; //傳送的資料，必須為位元組
    int recvLen; //接收的資料長度
    bool _isUWBConnect;
    bool isUWBConnect
    {
        get { return _isUWBConnect; }
        set
        {
            if (value)
                uwbStatusStr = "UWB : Connected";
            else
                uwbStatusStr = "UWB : Disconnected";
            _isUWBConnect = value;
        }
    }
    Thread connectThread; //連線執行緒

    [SerializeField] GameObject startPage = null;
    [SerializeField] GameObject mapPage = null;

    [SerializeField] Button startServerBtn = null;
    [SerializeField] Button closeServerBtn = null;
    [SerializeField] TextMeshProUGUI ipText = null;
    [SerializeField] TextMeshProUGUI uwbStatusText = null;
    [SerializeField] TextMeshProUGUI base1Text = null;
    [SerializeField] TextMeshProUGUI base2Text = null;
    [SerializeField] TextMeshProUGUI base3Text = null;
    MapController mapController;
    // Use this for initialization
    void Start()
    {
        mapController = GetComponentInChildren<MapController>(true);
        mapController.Initialize();
        startServerBtn.onClick.AddListener(InitSocket);
        closeServerBtn.onClick.AddListener(SocketQuit);
    }


    // Update is called once per frame
    void Update()
    {
        updateUI();
    }

    void OnApplicationQuit()
    {
        SocketQuit();
    }
    void updateUI()
    {
        uwbStatusText.text = uwbStatusStr;
        base1Text.text = base1Dis.ToString();
        base2Text.text = base2Dis.ToString();
        base3Text.text = base3Dis.ToString();
        if(base1Dis != -1 && base2Dis != -1 && base3Dis != -1)
            mapController.UpdateTagPosition(base1Dis, base2Dis, base3Dis);
    }
    //初始化
    void InitSocket()
    {
        //定義偵聽埠,偵聽任何IP
        ipEnd = new IPEndPoint(IPAddress.Any, 5566);
        //定義套接字型別,在主執行緒中定義
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //連線
        serverSocket.Bind(ipEnd);
        //開始偵聽,最大10個連線
        serverSocket.Listen(10);

        //serverSocket.BeginAccept(new AsyncCallback(AcceptClient), serverSocket);

        //開啟一個執行緒連線，必須的，否則主執行緒卡死
        connectThread = new Thread(new ThreadStart(SocketReceive));
        connectThread.Start();
        startPage.SetActive(false);
        mapPage.SetActive(true);
        //string ipv4 = IPManager.GetIP(ADDRESSFAM.IPv4);
        string ipv4 = GetLocalIPv4();
        ipText.text = $"IP : {ipv4}";
        isUWBConnect = false;
    }
    void AcceptClient(IAsyncResult iar)
    {
        Socket server = iar.AsyncState as Socket;
        Socket client = server.EndAccept(iar); // 保存客户端
        print("【System：new client conneted...】");
        //socketlist.Add(client);
        print("【客户端IP：】" + client.RemoteEndPoint);
        var pdu = new PDUUWB();
        pdu.msg = "welcome";
        string msg = JsonConvert.SerializeObject(pdu);
        byte[] data = Encoding.UTF8.GetBytes(msg);
        client.Send(data); // 发送给客户端
        Thread th = new Thread(ReceiveMsg); // 开启客户端线程
        th.Start(client);
        client2Tread[client] = th;
        server.BeginAccept(new AsyncCallback(AcceptClient), server);
    }

    void ReceiveMsg(object obj)
    {
        Socket socket = obj as Socket;
        bool isUWB = false;
        while (true)
        {
            byte[] buffer = new byte[1024];
            int length = 0;
            try
            {
                length = socket.Receive(buffer);
            }
            catch(Exception e)
            {
                print("Exception: " + e.Message);
                IPEndPoint point = socket.RemoteEndPoint as IPEndPoint;
                string ipEndpoint = point.Address.ToString();
                print(ipEndpoint + "：退出...");
                client2Tread.Remove(socket);
                if (isUWB)
                    isUWBConnect = false;
                //SendMsgToAll(ipEndpoint + "：有客户端退出..."); // 调用群发消息
                break;
            }
            string resMsg = Encoding.UTF8.GetString(buffer, 0, length);
            resMsg = JsonValidate(resMsg);
            var pdu = JsonConvert.DeserializeObject<PDUUWB>(resMsg);
            switch (pdu.intent)
            {
                case PDUUWB.Intent.Connect:
                    if (pdu.msg.Equals("UWB"))
                    {
                        UWBSocket = socket;
                        isUWBConnect = true;
                        isUWB = true;
                    }
                        
                    break;
                case PDUUWB.Intent.TranferData:
                    base1Dis = pdu.base1;
                    base2Dis = pdu.base2;
                    base3Dis = pdu.base3;
                    break;
                default:
                    break;
            }
            IPEndPoint IEP = socket.RemoteEndPoint as IPEndPoint;
            string ip = IEP.Address.ToString();
            string time = DateTime.Now.ToString();
            resMsg = "[" + ip + "  " + time + "]" + ": \n" + resMsg;
            //Console.WriteLine(resMsg);
            //SendMsgToAll(resMsg);
        }
    }
    String JsonValidate(string json)
    {
        int count = 0;
        int index = 0;
        for (int i = 0; i < json.Length; i++)
        {
            if (json[i] == '{')
                count++;
            else if (json[i] == '}')
                count--;
            if (count == 0)
            {
                index = i;
                break;
            }
        }
        json = json.Substring(0, index + 1);
        return json;
    }
    //伺服器接收
    void SocketReceive()
    {
        print("Start Server Listening");
        //一旦接受連線，建立一個客戶端
        Socket client = serverSocket.Accept();
        print("Recieve Client!!");
        Thread th = new Thread(ReceiveMsg); // 开启客户端线程
        th.Start(client);
        client2Tread[client] = th;
    }

    //連線關閉
    void SocketQuit()
    {
        //先關閉客戶端
        foreach (var client in client2Tread)
        {
            client.Key.Close();
            client.Value.Interrupt();
            client.Value.Abort();
        }
        client2Tread.Clear();
        //再關閉執行緒
        //if (connectThread != null)
        //{
        //    connectThread.Interrupt();
        //    connectThread.Abort();
        //}
        //最後關閉伺服器
        serverSocket.Close();
        startPage.SetActive(true);
        mapPage.SetActive(false);
        print("diconnect");
    }

    public string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }

    
}
