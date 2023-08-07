using AM2E.Levels;
using LiteNetLib;
using System;
using System.Collections.Generic;

namespace AM2E.Networking;

public class Client
{
    private readonly EventBasedNetListener listener = new EventBasedNetListener();
    private readonly NetManager manager;
    private NetPeer server;
    internal static readonly Dictionary<string, INetSynced> NetObjects = new();
    private int tick = 5;
    private int rollForwardTick = 0;
    private bool isConnected;
    private int lastServerTick = 0;

    private readonly BitPackedData bitPacker = new BitPackedData();
    private Level level;
    private readonly Dictionary<int, PeerInput> players = new();
    private int remoteId;
    private readonly int[] delayAverage = new int[10];
    private int delayIndex = 0;
    private bool readAverages = false;
    private int desiredDelay = 3;
    private int inputdelay = 0;
    private readonly BitPackedData[] stateBuffer = new BitPackedData[NetworkGeneral.MaxGameSequence];

    public Client(string IP, int port)
    {
        level = World.GetLevelByName("Level_0");
        manager = new NetManager(listener);
        manager.Start();
        manager.Connect("localhost", 64198, "");
        
        listener.NetworkReceiveEvent += ListenerNetworkReceiveEvent;
        listener.PeerConnectedEvent += ListenerPeerConnectedEvent;
        listener.PeerDisconnectedEvent += ListenerPeerDisconnectedEvent;
    }

    private void ListenerPeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        isConnected = false;
        server = null;
    }

    private void ListenerPeerConnectedEvent(NetPeer peer)
    {
        Console.WriteLine(peer.Mtu);
        server = peer;
        players.Add(peer.RemoteId, new PeerInput());
        remoteId = peer.RemoteId;
        Console.WriteLine(remoteId);
        Console.WriteLine("Connected to Server");
    }

    public void Update()
    {
        tick = (tick + 1) % NetworkGeneral.MaxGameSequence;
        
        if (stateBuffer[tick] != null)
        {
            StateSnapshotReceive(tick, stateBuffer[tick]);
            stateBuffer[tick] = null;
        }
        
        manager.PollEvents();
        
        if (isConnected)
        {
            SendInput();
            UpdateControllers(tick);
        }
    }
    
    private void UpdateControllers(int controllerTick)
    {
        foreach (var input in players.Values)
        {
            if (input.InputBuffer[controllerTick] != null)
            {
                input.controller.NetCommand = input.InputBuffer[controllerTick];
            }
        }
    }

    private void SendInput()
    {
        // Get Current input for frame
        var currentCommand = new NetCommand(false);
        
        // Add to NetCommand Queue
        var inputTick = (tick + inputdelay) % NetworkGeneral.MaxGameSequence;
        players[remoteId].InputBuffer[inputTick] = currentCommand;
        
        // Send NetCommand Queue
        bitPacker.Reset();
        bitPacker.WriteBits(inputTick, 10);
        // PacketType of 1
        bitPacker.WriteBits(1, 8);
        bitPacker.WriteBits(lastServerTick, 10);
        
        var count = 0;
        // Get count of inputs
        for (var i = 0; i < 10; i++)
        {
            var index = NetworkGeneral.Mod((inputTick - i), NetworkGeneral.MaxGameSequence);
            
            if (players[remoteId].InputBuffer[index] == null)
                break;
            
            count++;
        }
        
        // 4 bits covers a range of 0 to 15.
        bitPacker.WriteBits(count, 4);
        
        for (var i = 0; i < 10; i++)
        {
            var index = NetworkGeneral.Mod((inputTick - i), NetworkGeneral.MaxGameSequence);
            
            if (players[remoteId].InputBuffer[index] == null)
                break;
            
            players[remoteId].InputBuffer[index].Serialize(bitPacker);
        }
        
        server.Send(bitPacker.CopyData(), DeliveryMethod.Unreliable);
    }

    private void ObjectCreationEventReceive(BitPackedData data)
    {
        var objEvent = new ObjectCreationEvent();
        objEvent.Deserialize(data);
        
        if (NetObjects.ContainsKey(objEvent.ID))
            return;
        
        // TODO: This (and probably other) methods need a lot of safety handling for bad data.
        
        var type = Type.GetType("GameContent." + objEvent.Type);
        level = World.GetLevelByName("Level_0");
        var layer = level.GetLayer(objEvent.Layer);
        Activator.CreateInstance(type, objEvent.X, objEvent.Y, layer, objEvent.ID);
    }

    private void ControllableCreationEventReceive(BitPackedData data)
    {
        var objEvent = new ControllableCreationEvent();
        objEvent.Deserialize(data);
        
        if (NetObjects.ContainsKey(objEvent.ID))
            return;
        
        var type = Type.GetType("GameContent." + objEvent.Type);
        level = World.GetLevelByName("Level_0");
        var layer = level.GetLayer(objEvent.Layer);
        var player = Activator.CreateInstance(type, objEvent.X, objEvent.Y, layer, objEvent.ID, objEvent.Master) as INetControllable;
        
        if (!players.ContainsKey(objEvent.Master))
        {
            players.Add(objEvent.Master, new PeerInput());
        }
        
        player.controller = players[objEvent.Master].controller;
    }

    private void ObjectDeletionEventReceive(BitPackedData data)
    {
        var objectDeletionEvent = new ObjectDeletionEvent();
        objectDeletionEvent.Deserialize(data);
        
        if (GenericLevelElement.AllElements.ContainsKey(objectDeletionEvent.ID))
        {
            GenericLevelElement.AllElements[objectDeletionEvent.ID].Dispose();
        }
    }
    
    private int CalculateDelayAverage()
    {
        var avg = 0;
        
        for (var i = 0; i < 10; i++)
        {
            avg += delayAverage[i];
        }
        
        return avg / 10;
    }

    private void StateSnapshotReceive(int snapshotTick, BitPackedData data)
    {
        /*
         * Packet Structure
         * 8 bit Reliable message count
         * variable bits Reliable messages
         * 8 bit object count
         * variable bit Object ID + Object Data
         * Player controller data
         */
        var delay = data.ReadBits(8) - 128;
        delayAverage[delayIndex] = delay;
        delayIndex = (delayIndex + 1) % 10;
        
        if (delayIndex == 0)
        {
            readAverages = true;
        }
        
        
        var reliableMessageCount = data.ReadBits(8);
        
        for (var i = 0; i < reliableMessageCount; i++)
        {
            var type = data.ReadBits(8);
            switch (type)
            {
                case 1:
                    ObjectCreationEventReceive(data);
                    break;
                case 2:
                    ControllableCreationEventReceive(data);
                    break;
                case 3:
                    ObjectDeletionEventReceive(data);
                    break;
            }
        }

        if (NetworkGeneral.SeqDiff(snapshotTick, lastServerTick) <= 0)
            return;

        lastServerTick = snapshotTick;
        
        if (!isConnected)
            return;
        
        var count = data.ReadBits(8);
        for (var i = 0; i < count; i++)
        {
            var id = data.ReadID();
            var netObject = NetObjects[id];
            netObject.Deserialize(data);
        }
        
        var playerCount = data.ReadBits(8);
        for (var i = 0; i < playerCount; i++)
        {
            var id = data.ReadBits(8);
            var netCommand = new NetCommand();
            netCommand.Deserialize(data);
            
            if (players.ContainsKey(id))
            {
                players[id].controller.NetCommand = netCommand;
            }
            else
            {
                players.Add(id, new PeerInput());
                players[id].controller.NetCommand = netCommand;
            }
        }
        
        rollForwardTick = snapshotTick;
        if (NetworkGeneral.SeqDiff(tick, rollForwardTick) < 0)
            return;

        while (NetworkGeneral.SeqDiff(tick, rollForwardTick) != 0)
        {
            UpdateControllers(rollForwardTick);
            //EngineCore.FixedUpdate();
            rollForwardTick = (rollForwardTick + 1) % NetworkGeneral.MaxGameSequence;
        }
    }

    private void JoinPacketReceive(NetPeer peer, int receivedTick, BitPackedData data) 
    {
        if (isConnected)
            return;
        
        remoteId = data.ReadBits(8);
        // TODO: Does this need to be modulo'd?
        tick = (receivedTick + 6) % NetworkGeneral.MaxGameSequence;
        lastServerTick = receivedTick;
        isConnected = true;
    }

    internal static void RegisterElement(INetSynced netSyncedGle, string id)
    {
        NetObjects.Add(id, netSyncedGle);
    }

    internal static void DeleteObject(string id)
    {
        NetObjects.Remove(id);
    }

    private void ListenerNetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        var bitReader = new BitPackedData(reader.GetRemainingBytes());
        var eventTick = bitReader.ReadBits(10);
        
        var type = bitReader.ReadBits(8);
        switch (type)
        {
            case 1:
                // Server state is ahead of current time
                if (NetworkGeneral.SeqDiff(eventTick, tick) > 0)
                {
                    stateBuffer[eventTick] = bitReader;
                }
                else
                {
                    StateSnapshotReceive(eventTick, bitReader);
                }
                break;
            case 2:
                JoinPacketReceive(peer, eventTick, bitReader);
                break;
        }
    }
}
