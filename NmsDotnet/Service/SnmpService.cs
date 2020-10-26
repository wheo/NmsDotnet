using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using SnmpSharpNet;
using System.Diagnostics;
using System.Net.Sockets;
using NmsDotNet.vo;
using System.Windows.Controls;
using log4net;

namespace NmsDotNet.Service
{
    public class SnmpService
    {
        public static bool _shouldStop = false;

        public static string _DR5000UnitName_oid = "1.3.6.1.4.1.27338.5.2.2.0";
        public static string _CM5000UnitName_oid = "1.3.6.1.4.1.27338.4.2.2.0";

        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SnmpService()
        {
            //Constructor
        }

        public static void Set()
        {
            // Prepare target
            UdpTarget target = new UdpTarget((IPAddress)new IpAddress("192.168.2.167"));
            // Create a SET PDU
            Pdu pdu = new Pdu(PduType.Set);
            /*
            // Set sysLocation.0 to a new string
            pdu.VbList.Add(new Oid("1.3.6.1.2.1.1.6.0"), new OctetString("Some other value"));
            // Set a value to integer
            pdu.VbList.Add(new Oid("1.3.6.1.2.1.67.1.1.1.1.5.0"), new Integer32(500));
            */
            // Set a value to unsigned integer
            pdu.VbList.Add(new Oid("1.3.6.1.4.1.27338.5.3.2.3.2.1.0"), new Integer32(5000)); // primary service id
            pdu.VbList.Add(new Oid("1.3.6.1.4.1.27338.5.3.2.3.3.1.0"), new Integer32(5005)); // video output pid
            // Set Agent security parameters
            AgentParameters aparam = new AgentParameters(SnmpVersion.Ver2, new OctetString("private"));
            // Response packet
            SnmpV2Packet response;
            try
            {
                // Send request and wait for response
                response = target.Request(pdu, aparam) as SnmpV2Packet;
            }
            catch (Exception ex)
            {
                // If exception happens, it will be returned here
                Console.WriteLine(String.Format("Request failed with exception: {0}", ex.Message));
                target.Close();
                return;
            }
            // Make sure we received a response
            if (response == null)
            {
                Console.WriteLine("Error in sending SNMP request.");
            }
            else
            {
                // Check if we received an SNMP error from the agent
                if (response.Pdu.ErrorStatus != 0)
                {
                    Debug.WriteLine(String.Format("SNMP agent returned ErrorStatus {0} on index {1}",
                        response.Pdu.ErrorStatus, response.Pdu.ErrorIndex));
                }
                else
                {
                    // Everything is ok. Agent will return the new value for the OID we changed
                    Debug.WriteLine(String.Format("Agent response {0}: {1}",
                        response.Pdu[0].Oid.ToString(), response.Pdu[0].Value.ToString()));
                }
            }
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

        public static bool Get(string Ip, out string unitName)
        {
            unitName = null;
            // SNMP community name
            OctetString community = new OctetString("public");
            // Define agent parameters class
            AgentParameters param = new AgentParameters(community);
            // Set SNMP version to 1 (or 2)
            param.Version = SnmpVersion.Ver2;

            // Construct the agent address object
            // IPAddress class is easy to use here because it will try to resolve constructor parameter if it doesn't parse to an IP address
            IpAddress agent = new IpAddress(Ip);
            // Construct target
            UdpTarget target = new UdpTarget((IPAddress)agent, 161, 10, 1);
            // Pdu class used for all requests
            Pdu pdu = new Pdu(PduType.Get);
            /*
            pdu.VbList.Add("1.3.6.1.2.1.1.1.0"); //sysDescr
            pdu.VbList.Add("1.3.6.1.2.1.1.2.0"); //sysObjectID
            pdu.VbList.Add("1.3.6.1.2.1.1.3.0"); //sysUpTime
            pdu.VbList.Add("1.3.6.1.2.1.1.4.0"); //sysContact
            pdu.VbList.Add("1.3.6.1.2.1.1.5.0"); //sysName
            */
            // _oid : 장비 이름
            pdu.VbList.Add(_DR5000UnitName_oid);
            pdu.VbList.Add(_CM5000UnitName_oid);

            try
            {
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
                        for (int i = 0; i < result.Pdu.VbCount; i++)
                        {
                            Debug.WriteLine("[{3}] sysDescr({0}) ({1}): {2}",
                                result.Pdu.VbList[i].Oid.ToString(),
                                SnmpConstants.GetTypeName(result.Pdu.VbList[0].Value.Type),
                                result.Pdu.VbList[i].Value.ToString(), Ip);

                            if (result.Pdu.VbList[i].Value.ToString().Equals("SNMP No-Such-Object"))
                            {
                                continue;
                            }
                            else
                            {
                                unitName = result.Pdu.VbList[i].Value.ToString();
                            }

                            //logger.Info(String.Format($"[{Ip}] sysDescr({result.Pdu.VbList[i].Oid.ToString()}) ({ SnmpConstants.GetTypeName(result.Pdu.VbList[0].Value.Type)}): { result.Pdu.VbList[i].Value.ToString()}"));
                        }
                        target.Close();
                        return true;
                    }
                }
                else
                {
                    target.Close();
                    Debug.WriteLine("No response received from SNMP agent.");
                    return false;
                }
            }
            catch (Exception)
            {
                target.Close();
                logger.Error(String.Format("Ip : {0}", Ip));
                //Debug.WriteLine(e.ToString());
            }
            return false;
        }

        public void TrapSend()
        {
        }

        public static void GetBulk()
        {
            // SNMP community name
            OctetString community = new OctetString("public");
            // Define agent parameters class
            AgentParameters param = new AgentParameters(community);
            // Set SNMP version to 2 (GET-BULK only works with SNMP ver 2 and 3)
            param.Version = SnmpVersion.Ver2;
            // Construct the agent address object
            // IpAddress class is easy to use here because
            //  it will try to resolve constructor parameter if it doesn't
            //  parse to an IP address
            IpAddress agent = new IpAddress("192.168.2.60");
            // Construct target
            UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 1);
            // Define Oid that is the root of the MIB
            //  tree you wish to retrieve
            Oid rootOid = new Oid(".1.3.6.1.4.1.27338.4"); // ifDescr
                                                           // This Oid represents last Oid returned by
                                                           //  the SNMP agent
            Oid lastOid = (Oid)rootOid.Clone();
            // Pdu class used for all requests
            Pdu pdu = new Pdu(PduType.GetBulk);
            // In this example, set NonRepeaters value to 0
            pdu.NonRepeaters = 0;
            // MaxRepetitions tells the agent how many Oid/Value pairs to return
            // in the response.
            pdu.MaxRepetitions = 5;
            // Loop through results
            while (lastOid != null)
            {
                // When Pdu class is first constructed, RequestId is set to 0
                // and during encoding id will be set to the random value
                // for subsequent requests, id will be set to a value that
                // needs to be incremented to have unique request ids for each
                // packet
                if (pdu.RequestId != 0)
                {
                    pdu.RequestId += 1;
                }
                // Clear Oids from the Pdu class.
                pdu.VbList.Clear();
                // Initialize request PDU with the last retrieved Oid
                pdu.VbList.Add(lastOid);
                // Make SNMP request
                SnmpV2Packet result = (SnmpV2Packet)target.Request(pdu, param);
                // You should catch exceptions in the Request if using in real application.
                // If result is null then agent didn't reply or we couldn't parse the reply.
                if (result != null)
                {
                    // ErrorStatus other then 0 is an error returned by
                    // the Agent - see SnmpConstants for error definitions
                    if (result.Pdu.ErrorStatus != 0)
                    {
                        // agent reported an error with the request
                        Console.WriteLine("Error in SNMP reply. Error {0} index {1}",
                            result.Pdu.ErrorStatus,
                            result.Pdu.ErrorIndex);
                        lastOid = null;
                        break;
                    }
                    else
                    {
                        // Walk through returned variable bindings
                        foreach (Vb v in result.Pdu.VbList)
                        {
                            // Check that retrieved Oid is "child" of the root OID
                            if (rootOid.IsRootOf(v.Oid))
                            {
                                Console.WriteLine("{0} ({1}): {2}",
                                    v.Oid.ToString(),
                                    SnmpConstants.GetTypeName(v.Value.Type),
                                    v.Value.ToString());

                                if (v.Value.Type == SnmpConstants.SMI_ENDOFMIBVIEW)
                                    lastOid = null;
                                else
                                    lastOid = v.Oid;
                            }
                            else
                            {
                                // we have reached the end of the requested
                                // MIB tree. Set lastOid to null and exit loop
                                lastOid = null;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No response received from SNMP agent.");
                }
            }
            target.Close();
        }

        public static void GetNext()
        {
            // SNMP community name
            OctetString community = new OctetString("public");
            // Define agent parameters class
            AgentParameters param = new AgentParameters(community);
            // Set SNMP version to 1
            param.Version = SnmpVersion.Ver1;
            // Construct the agent address object
            // IpAddress class is easy to use here because
            //  it will try to resolve constructor parameter if it doesn't
            //  parse to an IP address
            IpAddress agent = new IpAddress("192.168.2.60");
            // Construct target
            UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 1);
            // Define Oid that is the root of the MIB
            //  tree you wish to retrieve
            Oid rootOid = new Oid(".1.3.6.1.4.1.27338.4"); // ifDescr
                                                           // This Oid represents last Oid returned by
                                                           //  the SNMP agent
            Oid lastOid = (Oid)rootOid.Clone();
            // Pdu class used for all requests
            Pdu pdu = new Pdu(PduType.GetNext);
            // Loop through results
            while (lastOid != null)
            {
                // When Pdu class is first constructed, RequestId is set to a random value
                // that needs to be incremented on subsequent requests made using the
                // same instance of the Pdu class.
                if (pdu.RequestId != 0)
                {
                    pdu.RequestId += 1;
                }
                // Clear Oids from the Pdu class.
                pdu.VbList.Clear();
                // Initialize request PDU with the last retrieved Oid
                pdu.VbList.Add(lastOid);
                // Make SNMP request
                SnmpV1Packet result = (SnmpV1Packet)target.Request(pdu, param);
                // You should catch exceptions in the Request if using in real application.
                // If result is null then agent didn't reply or we couldn't parse the reply.
                if (result != null)
                {
                    // ErrorStatus other then 0 is an error returned by
                    // the Agent - see SnmpConstants for error definitions
                    if (result.Pdu.ErrorStatus != 0)
                    {
                        // agent reported an error with the request
                        Console.WriteLine("Error in SNMP reply. Error {0} index {1}",
                            result.Pdu.ErrorStatus,
                            result.Pdu.ErrorIndex);
                        lastOid = null;
                        break;
                    }
                    else
                    {
                        // Walk through returned variable bindings
                        foreach (Vb v in result.Pdu.VbList)
                        {
                            // Check that retrieved Oid is "child" of the root OID
                            if (rootOid.IsRootOf(v.Oid))
                            {
                                Console.WriteLine("{0} ({1}): {2}",
                                    v.Oid.ToString(),
                                    SnmpConstants.GetTypeName(v.Value.Type),
                                    v.Value.ToString());
                                lastOid = v.Oid;
                                /*
                                NmsDotnet.Database.vo.Snmp snmp = new NmsDotnet.Database.vo.Snmp()
                                {
                                    Id = v.Oid.ToString()
                                    , IP = agent.ToString()
                                    , Community = community.ToString()
                                    , Syntax = SnmpConstants.GetTypeName(v.Value.Type)
                                    , Value = v.Value.ToString()
                                    , type = "get"
                                };
                                NmsDotnet.Database.vo.Snmp.GetInstance().RegisterSnmpInfo(snmp);
                                //LogItem.GetInstance().LoggingDatabase(snmp);
                                */
                            }
                            else
                            {
                                // we have reached the end of the requested
                                // MIB tree. Set lastOid to null and exit loop
                                lastOid = null;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No response received from SNMP agent.");
                }
            }
            target.Close();
        }

        public static async Task<DataGrid> TrapListener()
        {
            return null;
        }
    }
}