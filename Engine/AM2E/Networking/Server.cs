using LiteNetLib;
using System.Collections.Generic;
using System;
using AM2E.Levels;

namespace AM2E.Networking;

public class Server
{
    private readonly Dictionary<int, NetPeer> players = new Dictionary<int, NetPeer>();
    private readonly Dictionary<int, NetPeer> unjoined = new Dictionary<int, NetPeer>();
    private readonly EventBasedNetListener listener = new EventBasedNetListener();
    private readonly NetManager manager;
    internal static readonly Dictionary<string, INetSynced> NetObjects = new();
    private int tick;
    private readonly BitPackedData bitPacker = new BitPackedData();
    private readonly List<NetReliableData> queuedReliableMessages = new();
    private readonly Dictionary<string, NetworkedEntityData> NetworkedEntities = new();
    public Server(int port)
    {
        manager = new NetManager(listener);
        manager.Start(port);
        
        Console.WriteLine("Server Initialised");

        listener.ConnectionRequestEvent += request =>
        {
            // Automatically accept connections for testing.
            request.Accept();
        };

        listener.PeerConnectedEvent += peer =>
        {
            unjoined.Add(peer.Id, peer);
            peer.Tag = new PeerInput();
            Console.WriteLine(peer.Mtu);
            Console.WriteLine("Peer joined");
        };
        
        listener.NetworkReceiveEvent += _listener_NetworkReceiveEvent;
        listener.PeerDisconnectedEvent += _listener_PeerDisconnectedEvent;
    }

    private void _listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        players.Remove(peer.Id);
        unjoined.Remove(peer.Id);

        peer.Tag = null;
        Console.WriteLine("Disconnected");
    }

    private void _listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        var bitReader = new BitPackedData(reader.GetRemainingBytes());
        var eventTick = bitReader.ReadBits(10);
        var type = bitReader.ReadBits(8);
        
        // Only one packet type for now - switch for future expansion.
        switch (type)
        {
            case 1:
                InputPacketReceive(peer, eventTick, bitReader);
                break;
        }
    }

    private void InputPacketReceive(NetPeer peer, int inputTick, BitPackedData data)
    {
        // TODO: Clean this shit up
        // Player has received join packet so remove from unjoined
        var peerInput = peer.Tag as PeerInput;
        if (!players.ContainsKey(peer.Id))
        {
            // Relevant Code
            unjoined.Remove(peer.Id);
            players.Add(peer.Id, peer);
            // Send all previous reliable messages to new player
            foreach (var reliableData in queuedReliableMessages)
            {
                if (!reliableData.SentTicks.ContainsKey(peer))
                    reliableData.SentTicks.Add(peer, tick);
            }
        }
        
        peerInput!.Delay = NetworkGeneral.SeqDiff(inputTick, tick);
        
        if (peerInput.Delay < 0)
            return;
        
        var acknowledgedTick = data.ReadBits(10);
        
        peerInput.AcknowledgedTick = acknowledgedTick;
        
        // Check Reliable Events
        if (players.ContainsKey(peer.Id))
        {
            foreach (var reliableData in queuedReliableMessages)
            {
                if (!reliableData.SentTicks.ContainsKey(peer)) 
                    continue;

                if (NetworkGeneral.SeqDiff(reliableData.SentTicks[peer], acknowledgedTick) > 0) 
                    continue;
                
                reliableData.SentTicks.Remove(peer);
                Console.WriteLine("Reliable message acknowledged");
            }
        }
        
        // Packet Structure
        // 4 bit count
        // variable bit inputs
        var count = data.ReadBits(4);
        
        for (var i = 0; i < count; i++)
        {
            var index = NetworkGeneral.Mod((inputTick - i), NetworkGeneral.MaxGameSequence);
            var distance = NetworkGeneral.SeqDiff(index, tick);
            
            if (distance < 0)
                break;
            
            var netCommand = new NetCommand();
            netCommand.Deserialize(data);
            peerInput.InputBuffer[index] = netCommand;
        }
    }

    internal void Update()
    {
        tick = (tick + 1) % NetworkGeneral.MaxGameSequence;
        manager.PollEvents();
        UpdateControllers();
        // Send State Packet.
        //SendJoin();
        SendState();
    }

    private void UpdateControllers()
    {
        foreach (var peer in players.Values) 
        {
            var input = (PeerInput)peer.Tag;
            var netCommand = input.InputBuffer[tick];
            
            if (netCommand != null)
            {
                input.controller.NetCommand = netCommand;
                
                if (input.lastInput != -1)
                {
                    input.InputBuffer[input.lastInput] = null;
                }
                
                input.lastInput = tick;
            }
            else if (input.lastInput != -1)
            {
                input.controller.NetCommand = input.InputBuffer[input.lastInput];
                Console.WriteLine("Read Last");
            }
            else
            {
                input.controller.NetCommand = new NetCommand();

                Console.WriteLine("Made new");
            }
        }
    }
    private void SendJoin()
    {
        // For proper implementation might want to merge this with state packet.
        foreach (var peer in unjoined.Values)
        {
            bitPacker.Reset();
            bitPacker.WriteBits(tick, 10);
            // Message Type.
            bitPacker.WriteBits(2, 8);
            bitPacker.WriteBits(peer.Id, 8);
            peer.Send(bitPacker.CopyData(), DeliveryMethod.Unreliable);
        }

    }
    private void SendState()
    {
        foreach (var peer in players.Values) 
        {
            bitPacker.Reset();
        
            bitPacker.WriteBits(tick, 10);
            // Message type.
            bitPacker.WriteBits(1, 8);
            var peerInput = (PeerInput)peer.Tag;
            // 128 is added here then subtracted on client end for range -128 to 127.
            bitPacker.WriteBits(peerInput.Delay + 128, 8);
            var count = 0;
            
            // Reliable message sending
            foreach (var data in NetworkedEntities.Values)
            {
                if (data.UnacknowledgedCreate.ContainsKey(peer) || data.UnacknowledgedDestroy.ContainsKey(peer))
                {
                    count++;
                }
            }
            
            bitPacker.WriteBits(count, 8);
            
            foreach (var data in NetworkedEntities.Values)
            {
                if (data.UnacknowledgedCreate.ContainsKey(peer))
                {
                    data.SerializeCreate(bitPacker);
                }
                else if (data.UnacknowledgedDestroy.ContainsKey(peer))
                {
                    data.SerializeDestroy(bitPacker);
                }
            }
            
            // NetObject State Syncing
            bitPacker.WriteBits(NetworkedEntities.Count, 8);
            foreach (var netData in NetworkedEntities.Values)
            {
                var gle = netData.Instance as GenericLevelElement;
                bitPacker.WriteID(gle!.ID);
                netData.Instance.Serialize(bitPacker);
            }
            
            // Player Input Syncing
            bitPacker.WriteBits(players.Count, 8);
            foreach (var player in players.Values)
            {
                var input = (PeerInput)player.Tag;
                bitPacker.WriteBits(player.Id, 8);
                
                if (input.InputBuffer[tick] != null)
                {
                    input.InputBuffer[tick].Serialize(bitPacker);
                }
                else if (input.lastInput != -1)
                {
                    input.InputBuffer[input.lastInput].Serialize(bitPacker);
                }
                else
                {
                    var netCommand = new NetCommand();
                    netCommand.Serialize(bitPacker);
                }
            }
            
            var sendData = bitPacker.CopyData();
            peer.Send(sendData, DeliveryMethod.Unreliable);
        }
    }

    internal void RegisterElement(GenericLevelElement gle)
    {
        if (gle is INetControllable or null)
            return;

        if (gle is not INetSynced)
            return;

        var type = gle.GetType().ToString();
        // Remove 'GameContent.' prefix.
        type = type.Remove(0, 12);
        var netEntityData = new NetworkedEntityData
        {
            ID = gle.ID,
            X = gle.X,
            Y = gle.Y,
            Layer = gle.Layer?.Name,
            Type = type,
            Instance = gle as INetSynced
        };
        foreach (var netPeer in players.Values)
        {
            netEntityData.UnacknowledgedCreate.Add(netPeer, tick);
        }

        NetworkedEntities.Add(gle.ID, netEntityData);
        /*
        var objCreation = new ObjectCreationEvent(type, id, layer, x, y);
        queuedReliableMessages.Add(objCreation);
        
        foreach (var netPeer in players.Values)
        {
            objCreation.SentTicks.Add(netPeer, tick);
        }
        
        NetObjects.Add(id, netSyncedGle);
        */
    }
    /*
    internal void RegisterControllableElement(INetControllable netControllableGle, int x, int y, string layer, string id)
    {
        var type = netControllableGle.GetType().ToString();
        type = type.Remove(0, 12);
        var objCreation = new ControllableCreationEvent(type, id, layer, x, y, netControllableGle.master);
        var peer = players[netControllableGle.master];
        var peerInput = peer.Tag as PeerInput;
        netControllableGle.controller = peerInput!.controller;
        queuedReliableMessages.Add(objCreation);
        
        foreach (var netPeer in players.Values)
        {
            objCreation.SentTicks.Add(netPeer, tick);
        }
        
        if (!NetObjects.ContainsKey(id))
        {
            NetObjects.Add(id, netControllableGle as INetSynced);
        }
    }
    */

    internal void DeleteObject(string id, GenericLevelElement gle)
    {
        if (gle is not INetSynced)
            return;

        NetworkedEntities[id].UnacknowledgedCreate.Clear();
        
        foreach (var netPeer in players.Values)
        {
            NetworkedEntities[id].UnacknowledgedDestroy.Add(netPeer, tick);
        }
    }

    public void Stop()
    {
        manager.Stop();
    }
}