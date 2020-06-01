using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using SnmpSharpNet;
using System.Diagnostics;
using System.Net.Sockets;

namespace NmsDotNet.lib
{
    class Snmp
    {
		protected Socket _socket;
		protected byte[] _inbuffer;
		protected IPEndPoint _peerIP;
		public static bool _shouldStop = false;

		public Snmp()
		{
			//Constructor
		}

		public static void GetTest()
		{
            // SNMP community name
            OctetString community = new OctetString("public");
            // Define agent parameters class
            AgentParameters param = new AgentParameters(community);
            // Set SNMP version to 1 (or 2)
            param.Version = SnmpVersion.Ver2;

            // Construct the agent address object
            // IPAddress class is easy to use here because it will try to resolve constructor parameter if it doesn't parse to an IP address
            IpAddress agent = new IpAddress("192.168.2.129");
            // Construct target
            UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 1);
            // Pdu class used for all requests
            Pdu pdu = new Pdu(PduType.Get);
            pdu.VbList.Add("1.3.6.1.2.1.1.1.0"); //sysDescr            

            // Make SNMP request
            SnmpV2Packet result = (SnmpV2Packet)target.Request(pdu, param);
            // If request is null then agent didn't reply or we couldn't parse the reply.
            if (result != null)
            {
                // ErrorStatus other then 0 is an error returned by the Agent - see SnmpConstants for error definitions
                if (result.Pdu.ErrorStatus != 0)
                {
                    // agent reported an error with request
                    Debug.WriteLine("Error in SNMP reply. Error {0} index {1}",
                        result.Pdu.ErrorStatus,
                        result.Pdu.ErrorIndex);
                }
                else
                {
                    // Reply variables are returned in the same order as they were added to the VbList
                    Debug.WriteLine("sysDescr({0}) ({1}): {2}",
                        result.Pdu.VbList[0].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[0].Value.Type),
                        result.Pdu.VbList[0].Value.ToString());
                    Debug.WriteLine("sysObjectID({0}) ({1}): {2}",
                        result.Pdu.VbList[1].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[1].Value.Type),
                        result.Pdu.VbList[1].Value.ToString());
                    Debug.WriteLine("sysUpTime({0}) ({1}): {2}",
                        result.Pdu.VbList[2].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[2].Value.Type),
                        result.Pdu.VbList[2].Value.ToString());
                    Debug.WriteLine("sysContact({0}) ({1}): {2}",
                        result.Pdu.VbList[3].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[3].Value.Type),
                        result.Pdu.VbList[3].Value.ToString());
                    Debug.WriteLine("sysName({0}) ({1}): {2}",
                        result.Pdu.VbList[4].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[4].Value.Type),
                        result.Pdu.VbList[4].Value.ToString());
                }
            }
            else
            {
                Debug.WriteLine("No response received from SNMP agent.");
            }
            target.Close();
        }

		public static void Get()
        {
            // SNMP community name
            OctetString community = new OctetString("public");
            // Define agent parameters class
            AgentParameters param = new AgentParameters(community);
            // Set SNMP version to 1 (or 2)
            param.Version = SnmpVersion.Ver2;

            // Construct the agent address object
            // IPAddress class is easy to use here because it will try to resolve constructor parameter if it doesn't parse to an IP address
            IpAddress agent = new IpAddress("192.168.2.129");
            // Construct target
            UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 1);
            // Pdu class used for all requests
            Pdu pdu = new Pdu(PduType.Get);
            pdu.VbList.Add("1.3.6.1.2.1.1.1.0"); //sysDescr
            pdu.VbList.Add("1.3.6.1.2.1.1.2.0"); //sysObjectID
            pdu.VbList.Add("1.3.6.1.2.1.1.3.0"); //sysUpTime
            pdu.VbList.Add("1.3.6.1.2.1.1.4.0"); //sysContact
            pdu.VbList.Add("1.3.6.1.2.1.1.5.0"); //sysName

            // Make SNMP request
            SnmpV2Packet result = (SnmpV2Packet)target.Request(pdu, param);
            // If request is null then agent didn't reply or we couldn't parse the reply.
            if ( result != null)
            {
                // ErrorStatus other then 0 is an error returned by the Agent - see SnmpConstants for error definitions
                if ( result.Pdu.ErrorStatus != 0)
                {
                    // agent reported an error with request
                    Debug.WriteLine("Error in SNMP reply. Error {0} index {1}",
                        result.Pdu.ErrorStatus,
                        result.Pdu.ErrorIndex);
                } else {
                    // Reply variables are returned in the same order as they were added to the VbList
                    Debug.WriteLine("sysDescr({0}) ({1}): {2}",
                        result.Pdu.VbList[0].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[0].Value.Type),
                        result.Pdu.VbList[0].Value.ToString());
                    Debug.WriteLine("sysObjectID({0}) ({1}): {2}",
                        result.Pdu.VbList[1].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[1].Value.Type),
                        result.Pdu.VbList[1].Value.ToString());
                    Debug.WriteLine("sysUpTime({0}) ({1}): {2}",
                        result.Pdu.VbList[2].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[2].Value.Type),
                        result.Pdu.VbList[2].Value.ToString());
                    Debug.WriteLine("sysContact({0}) ({1}): {2}",
                        result.Pdu.VbList[3].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[3].Value.Type),
                        result.Pdu.VbList[3].Value.ToString());
                    Debug.WriteLine("sysName({0}) ({1}): {2}",
                        result.Pdu.VbList[4].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[4].Value.Type),
                        result.Pdu.VbList[4].Value.ToString());
                }
            }
            else
            {
                Debug.WriteLine("No response received from SNMP agent.");
            }
            target.Close();
        }

        public void Set()
        {

        }

        public void Trap()
        {

        }

		public static async Task<bool> TrapListener()
		{
			// Construct a socket and bind it to the trap manager port 162
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 162);
			EndPoint ep = (EndPoint)ipep;
			socket.Bind(ep);
			// Disable timeout processing. Just block until packet is received
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 0);
			
			int inlen = -1;
			while (!_shouldStop)
			{
				byte[] indata = new byte[16 * 1024];
				// 16KB receive buffer int inlen = 0;
				IPEndPoint peer = new IPEndPoint(IPAddress.Any, 0);
				EndPoint inep = (EndPoint)peer;
				try
				{
					inlen = socket.ReceiveFrom(indata, ref inep);
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Exception {0}", ex.Message);
					inlen = -1;
				}
				if (inlen > 0)
				{
					// Check protocol version int
					int ver = SnmpPacket.GetProtocolVersion(indata, inlen);
					if (ver == (int)SnmpVersion.Ver1)
					{
						// Parse SNMP Version 1 TRAP packet
						SnmpV1TrapPacket pkt = new SnmpV1TrapPacket();
						pkt.decode(indata, inlen);
						Debug.WriteLine("** SNMP Version 1 TRAP received from {0}:", inep.ToString());
						Debug.WriteLine("*** Trap generic: {0}", pkt.Pdu.Generic);
						Debug.WriteLine("*** Trap specific: {0}", pkt.Pdu.Specific);
						Debug.WriteLine("*** Agent address: {0}", pkt.Pdu.AgentAddress.ToString());
						Debug.WriteLine("*** Timestamp: {0}", pkt.Pdu.TimeStamp.ToString());
						Debug.WriteLine("*** VarBind count: {0}", pkt.Pdu.VbList.Count);
						Debug.WriteLine("*** VarBind content:");
						foreach (Vb v in pkt.Pdu.VbList)
						{
							Debug.WriteLine("**** {0} {1}: {2}", v.Oid.ToString(), SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString());
						}
						Debug.WriteLine("** End of SNMP Version 1 TRAP data.");
					}
					else
					{
						// Parse SNMP Version 2 TRAP packet
						SnmpV2Packet pkt = new SnmpV2Packet();
						pkt.decode(indata, inlen);
						Debug.WriteLine("** SNMP Version 2 TRAP received from {0}:", inep.ToString());
						if ((SnmpSharpNet.PduType)pkt.Pdu.Type != PduType.V2Trap)
						{
							Debug.WriteLine("*** NOT an SNMPv2 trap ****");
						}
						else
						{
							Debug.WriteLine("*** Community: {0}", pkt.Community.ToString());
							Debug.WriteLine("*** VarBind count: {0}", pkt.Pdu.VbList.Count);
							Debug.WriteLine("*** VarBind content:");
							foreach (Vb v in pkt.Pdu.VbList)
							{
								Debug.WriteLine("**** {0} {1}: {2}",
								   v.Oid.ToString(), SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString());
							}
							Debug.WriteLine("** End of SNMP Version 2 TRAP data.");
						}
					}
				}
				else
				{
					if (inlen == 0)
						Debug.WriteLine("Zero length packet received.");
				}
			}
			return true;
		}	
	}
}
