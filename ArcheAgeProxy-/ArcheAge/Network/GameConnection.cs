﻿using LocalCommons.Native.Logging;
using LocalCommons.Native.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace ArcheAgeProxy.ArcheAge.Network
{
    /// <summary>
    /// Connection That Used Only For Interact With Game Servers.
    /// </summary>
    public class GameConnection : IConnection
    {
        private GameServer m_CurrentInfo;

        public GameServer CurrentInfo
        {
            get { return m_CurrentInfo; }
            set { m_CurrentInfo = value; }
        }

        public GameConnection(Socket socket) : base(socket) 
        {
            Logger.Trace("Game Server {0} - Connected", this);
            DisconnectedEvent += GameConnection_DisconnectedEvent;
            m_LittleEndian = true;
        }

        void GameConnection_DisconnectedEvent(object sender, EventArgs e)
        {
            Logger.Trace("Game Server {0} : Disconnected", m_CurrentInfo != null ? m_CurrentInfo.Id.ToString() : this.ToString());
            Dispose();
            GameServerController.DisconnecteGameServer(m_CurrentInfo != null ? m_CurrentInfo.Id : this.CurrentInfo.Id);
            m_CurrentInfo = null;
            //Game server will be corresponding status offline
        }

        public override void HandleReceived(byte[] data)
        {
            PacketReader reader = new PacketReader(data, 0);
            short opcode = reader.ReadLEInt16();
            PacketHandler<GameConnection> handler = PacketList.GHandlers[opcode];
            if (handler != null) {
                handler.OnReceive(this, reader);
            }
            else
                Logger.Trace("Received Undefined GameServer Packet 0x{0:X2}", opcode);
            reader = null;
        }
    }
}
