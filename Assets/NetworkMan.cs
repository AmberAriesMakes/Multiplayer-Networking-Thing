using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net;
using System;
using System.Net.Sockets;


public class NetworkMan : MonoBehaviour
{
    public UdpClient udp;
    public List<PlayerCube> LivePlayers;
    public List<string> spawnlist;
    public GameObject playerPrefab;
    public GameState latestGameState;
     public Message latestMessage;
    public string pAdress;
    void Start()

    {
        udp = new UdpClient();
        udp.Connect("ec2-3-138-184-196.us-east-2.compute.amazonaws.com", 12345); 
        Byte[] sendBytes = Encoding.ASCII.GetBytes("connect"); 
        udp.Send(sendBytes, sendBytes.Length);
        udp.BeginReceive(new AsyncCallback(OnReceived), udp); 
        InvokeRepeating("HeartBeat", 1, 1);
        InvokeRepeating("UpdatePos", 1, 0.13f); 
    }
    void OnDestroy()
    {
        udp.Dispose();
    }
    public enum commands{ 
        New,
        update,
        Disconneted,
        Curr_Players
    };
     [Serializable]
    public class Player{ 
        public string id; 

        [Serializable]
        public struct receivedPosition{ 
            public float x;
            public float y;
            public float z;
        }
        public receivedPosition position;

    }
    [Serializable]
    public class SuccessfullPlayerList 
    {
        public Player[] players;
    }
    [Serializable]
    public class Message
    {
        public commands cmd;
    }
    [Serializable]
    public class UpLocale
    { 
        public Vector3 position;
    }
    [Serializable]
    public class GameState 
    {
        public Player[] players; 
    }
    [Serializable]
    public class NPlayer 
    {
        public Player player;
    }
   
    [Serializable]
    public class DroppedPlayers 
    {
        public Player[] players;
        public string id;
      
    }
   
    void OnReceived(IAsyncResult result){
        UdpClient socket = result.AsyncState as UdpClient;
        IPEndPoint source = new IPEndPoint(0, 0);
        byte[] message = socket.EndReceive(result, ref source);
        string returnData = Encoding.ASCII.GetString(message);
        latestMessage = JsonUtility.FromJson<Message>(returnData);
        try{
            switch(latestMessage.cmd){ 
                case commands.New: 
                    NPlayer newPlayer = JsonUtility.FromJson<NPlayer>(returnData); 
                    Debug.Log(returnData);
                    spawnlist.Add(newPlayer.player.id);
                    if (pAdress == "") 
                    {
                        pAdress = newPlayer.player.id; 
                    }
                    break;
                case commands.update: 
                   
                    latestGameState = JsonUtility.FromJson<GameState>(returnData); 
                    UpdatePlayers(); 
                    Debug.Log(returnData);
                    break;
                case commands.Disconneted: 
                    DroppedPlayers droppedPlayer = JsonUtility.FromJson<DroppedPlayers>(returnData);
                    DestroyPlayers(droppedPlayer.id);
                    Debug.Log(returnData);
                    break;
                case commands.Curr_Players: 
                    SuccessfullPlayerList CurrentPlayers = JsonUtility.FromJson<SuccessfullPlayerList>(returnData); 
                    foreach (Player player in CurrentPlayers.players)
                    {
                        spawnlist.Add(player.id); 
                    }
                    Debug.Log(returnData);
                    break;
                default:
                    Debug.Log("Error");
                    break;
            }
        }
        catch (Exception e){
            Debug.Log(e.ToString()); 
        }
        socket.BeginReceive(new AsyncCallback(OnReceived), socket);
    }
    void Spawn(string _id)
    { 
        foreach(PlayerCube playerCube in LivePlayers)
        {
            if (playerCube.networkID == _id)
            {
                return;
            }
        }

        Vector3 startpos = new Vector3(UnityEngine.Random.Range(-3f, 3f), 
        UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f));
        GameObject PlayerCube2 = Instantiate(playerPrefab, startpos, Quaternion.identity); 
        PlayerCube2.GetComponent<PlayerCube>().networkID = _id; 
        LivePlayers.Add(PlayerCube2.GetComponent<PlayerCube>());
    }

    void SpawnQueue()
    {
        if (spawnlist.Count > 0)
        {
            for (int i = 0; i < spawnlist.Count; i++)
            {
                Spawn(spawnlist[i]);
            }
            spawnlist.Clear();
            spawnlist.TrimExcess();
        }
    }
   void UpdatePlayers()
    {
        for (int i = 0; i < latestGameState.players.Length; i++)
        {
            for (int j = 0; j < LivePlayers.Count; j++)
            {
                if (latestGameState.players[i].id == LivePlayers[j].networkID)
                {
                    if (latestGameState.players[i].id != pAdress)
                    {

                        LivePlayers[j].TrPose =
                          new Vector3(latestGameState.players[i].position.x, latestGameState.players[i].position.y, latestGameState.players[i].position.z);
                    }
                }
            }
        }
    }
    void DestroyPlayers(string _id)
    {
        foreach (PlayerCube playerCube in LivePlayers)
        {
            if (playerCube.networkID == _id)
            {
                playerCube.clear = true;
            }
        }
    }

    void HeartBeat()
    {
        Byte[] sendBytes = Encoding.ASCII.GetBytes("heartbeat");
        udp.Send(sendBytes, sendBytes.Length);
    }
    void UpdatePos()
    {
        UpLocale message = new UpLocale(); 

        for (int i = 0; i < LivePlayers.Count; i++) 
        {
            if (LivePlayers[i].networkID == pAdress) 
            {
                message.position.x = LivePlayers[i].transform.position.x;  message.position.y = LivePlayers[i].transform.position.y; message.position.z = LivePlayers[i].transform.position.z; 
                Byte[] sendBytes = Encoding.ASCII.GetBytes(JsonUtility.ToJson(message)); 
                udp.Send(sendBytes, sendBytes.Length);
            }
        }
    }

    void Update()
    {
        SpawnQueue();
    }
   
}