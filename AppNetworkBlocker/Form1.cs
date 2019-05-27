using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using WinDivertSharp;
using System.Net.NetworkInformation;

namespace AppNetworkBlocker
{
    public partial class Form1 : Form
    {
        [DllImport("iphlpapi.dll", SetLastError = true)]
        static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, TcpTableClass tblClass, uint reserved = 0);
        [DllImport("iphlpapi.dll", SetLastError = true)]
        static extern uint GetExtendedUdpTable(IntPtr pUdpTable, ref int dwOutBufLen, bool sort, int ipVersion, UdpTableClass tblClass, uint reserved = 0);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        // Enum to define the set of values used to indicate the type of table returned by  
        // calls made to the function 'GetExtendedTcpTable'. 
        public enum TcpTableClass
        {
            TCP_TABLE_BASIC_LISTENER,
            TCP_TABLE_BASIC_CONNECTIONS,
            TCP_TABLE_BASIC_ALL,
            TCP_TABLE_OWNER_PID_LISTENER,
            TCP_TABLE_OWNER_PID_CONNECTIONS,
            TCP_TABLE_OWNER_PID_ALL,
            TCP_TABLE_OWNER_MODULE_LISTENER,
            TCP_TABLE_OWNER_MODULE_CONNECTIONS,
            TCP_TABLE_OWNER_MODULE_ALL
        }

        // Enum to define the set of values used to indicate the type of table returned by calls 
        // made to the function GetExtendedUdpTable. 
        public enum UdpTableClass
        {
            UDP_TABLE_BASIC,
            UDP_TABLE_OWNER_PID,
            UDP_TABLE_OWNER_MODULE
        }

        // Enum for different possible states of TCP connection 
        public enum MibTcpState
        {
            CLOSED = 1,
            LISTENING = 2,
            SYN_SENT = 3,
            SYN_RCVD = 4,
            ESTABLISHED = 5,
            FIN_WAIT1 = 6,
            FIN_WAIT2 = 7,
            CLOSE_WAIT = 8,
            CLOSING = 9,
            LAST_ACK = 10,
            TIME_WAIT = 11,
            DELETE_TCB = 12,
            NONE = 0
        }

        /// <summary> 
        /// This class provides access an IPv4 UDP connection addresses and ports and its 
        /// associated Process IDs and names. 
        /// </summary> 
        [StructLayout(LayoutKind.Sequential)]
        public class UdpProcessRecord
        {
            [DisplayName("Local Address")]
            public IPAddress LocalAddress { get; set; }
            [DisplayName("Local Port")]
            public uint LocalPort { get; set; }
            [DisplayName("Process ID")]
            public int ProcessId { get; set; }
            [DisplayName("Process Name")]
            public string ProcessName { get; set; }

            public UdpProcessRecord(IPAddress localAddress, uint localPort, int pId)
            {
                LocalAddress = localAddress;
                LocalPort = localPort;
                ProcessId = pId;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class TcpProcessRecord
        {
            [DisplayName("Local Address")]
            public IPAddress LocalAddress { get; set; }
            [DisplayName("Local Port")]
            public ushort LocalPort { get; set; }
            [DisplayName("Remote Address")]
            public IPAddress RemoteAddress { get; set; }
            [DisplayName("Remote Port")]
            public ushort RemotePort { get; set; }
            [DisplayName("State")]
            public MibTcpState State { get; set; }
            [DisplayName("Process ID")]
            public int ProcessId { get; set; }
            [DisplayName("Process Name")]
            public string ProcessName { get; set; }

            public TcpProcessRecord(IPAddress localIp, IPAddress remoteIp, ushort localPort,
                ushort remotePort, int pId, MibTcpState state)
            {
                LocalAddress = localIp;
                RemoteAddress = remoteIp;
                LocalPort = localPort;
                RemotePort = remotePort;
                State = state;
                ProcessId = pId;
            }
        }


        /// <summary> 
        /// This function reads and parses the active TCP socket connections available 
        /// and stores them in a list. 
        /// </summary> 
        /// <returns> 
        /// It returns the current set of TCP socket connections which are active. 
        /// </returns> 
        /// <exception cref="OutOfMemoryException"> 
        /// This exception may be thrown by the function Marshal.AllocHGlobal when there 
        /// is insufficient memory to satisfy the request. 
        /// </exception> 
        private static List<TcpProcessRecord> GetAllTcpConnections()
        {
            int bufferSize = 0;
            List<TcpProcessRecord> tcpTableRecords = new List<TcpProcessRecord>();

            // Getting the size of TCP table, that is returned in 'bufferSize' variable. 
            uint result = GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, AF_INET,
                TcpTableClass.TCP_TABLE_OWNER_PID_ALL);

            // Allocating memory from the unmanaged memory of the process by using the 
            // specified number of bytes in 'bufferSize' variable. 
            IntPtr tcpTableRecordsPtr = Marshal.AllocHGlobal(bufferSize);

            try
            {
                // The size of the table returned in 'bufferSize' variable in previous 
                // call must be used in this subsequent call to 'GetExtendedTcpTable' 
                // function in order to successfully retrieve the table. 
                result = GetExtendedTcpTable(tcpTableRecordsPtr, ref bufferSize, true,
                    AF_INET, TcpTableClass.TCP_TABLE_OWNER_PID_ALL);

                // Non-zero value represent the function 'GetExtendedTcpTable' failed, 
                // hence empty list is returned to the caller function. 
                if (result != 0)
                    return new List<TcpProcessRecord>();

                // Marshals data from an unmanaged block of memory to a newly allocated 
                // managed object 'tcpRecordsTable' of type 'MIB_TCPTABLE_OWNER_PID' 
                // to get number of entries of the specified TCP table structure. 
                MIB_TCPTABLE_OWNER_PID tcpRecordsTable = (MIB_TCPTABLE_OWNER_PID)
                                        Marshal.PtrToStructure(tcpTableRecordsPtr,
                                        typeof(MIB_TCPTABLE_OWNER_PID));
                IntPtr tableRowPtr = (IntPtr)((long)tcpTableRecordsPtr +
                                        Marshal.SizeOf(tcpRecordsTable.dwNumEntries));

                // Reading and parsing the TCP records one by one from the table and 
                // storing them in a list of 'TcpProcessRecord' structure type objects. 
                for (int row = 0; row < tcpRecordsTable.dwNumEntries; row++)
                {
                    MIB_TCPROW_OWNER_PID tcpRow = (MIB_TCPROW_OWNER_PID)Marshal.
                        PtrToStructure(tableRowPtr, typeof(MIB_TCPROW_OWNER_PID));
                    tcpTableRecords.Add(new TcpProcessRecord(
                                          new IPAddress(tcpRow.localAddr),
                                          new IPAddress(tcpRow.remoteAddr),
                                          BitConverter.ToUInt16(new byte[2] {
                                              tcpRow.localPort[1],
                                              tcpRow.localPort[0] }, 0),
                                          BitConverter.ToUInt16(new byte[2] {
                                              tcpRow.remotePort[1],
                                              tcpRow.remotePort[0] }, 0),
                                          tcpRow.owningPid, tcpRow.state));
                    tableRowPtr = (IntPtr)((long)tableRowPtr + Marshal.SizeOf(tcpRow));
                }
            }
            catch (OutOfMemoryException outOfMemoryException)
            {
                MessageBox.Show(outOfMemoryException.Message, "Out Of Memory",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            finally
            {
                Marshal.FreeHGlobal(tcpTableRecordsPtr);
            }
            return tcpTableRecords != null ? tcpTableRecords.Distinct()
                .ToList<TcpProcessRecord>() : new List<TcpProcessRecord>();
        }


        /// <summary> 
        /// This function reads and parses the active UDP socket connections available 
        /// and stores them in a list. 
        /// </summary> 
        /// <returns> 
        /// It returns the current set of UDP socket connections which are active. 
        /// </returns> 
        /// <exception cref="OutOfMemoryException"> 
        /// This exception may be thrown by the function Marshal.AllocHGlobal when there 
        /// is insufficient memory to satisfy the request. 
        /// </exception> 
        private static List<UdpProcessRecord> GetAllUdpConnections()
        {
            int bufferSize = 0;
            List<UdpProcessRecord> udpTableRecords = new List<UdpProcessRecord>();

            // Getting the size of UDP table, that is returned in 'bufferSize' variable. 
            uint result = GetExtendedUdpTable(IntPtr.Zero, ref bufferSize, true,
                AF_INET, UdpTableClass.UDP_TABLE_OWNER_PID);

            // Allocating memory from the unmanaged memory of the process by using the 
            // specified number of bytes in 'bufferSize' variable. 
            IntPtr udpTableRecordPtr = Marshal.AllocHGlobal(bufferSize);

            try
            {
                // The size of the table returned in 'bufferSize' variable in previous 
                // call must be used in this subsequent call to 'GetExtendedUdpTable' 
                // function in order to successfully retrieve the table. 
                result = GetExtendedUdpTable(udpTableRecordPtr, ref bufferSize, true,
                    AF_INET, UdpTableClass.UDP_TABLE_OWNER_PID);

                // Non-zero value represent the function 'GetExtendedUdpTable' failed, 
                // hence empty list is returned to the caller function. 
                if (result != 0)
                    return new List<UdpProcessRecord>();

                // Marshals data from an unmanaged block of memory to a newly allocated 
                // managed object 'udpRecordsTable' of type 'MIB_UDPTABLE_OWNER_PID' 
                // to get number of entries of the specified TCP table structure. 
                MIB_UDPTABLE_OWNER_PID udpRecordsTable = (MIB_UDPTABLE_OWNER_PID)
                    Marshal.PtrToStructure(udpTableRecordPtr, typeof(MIB_UDPTABLE_OWNER_PID));
                IntPtr tableRowPtr = (IntPtr)((long)udpTableRecordPtr +
                    Marshal.SizeOf(udpRecordsTable.dwNumEntries));

                // Reading and parsing the UDP records one by one from the table and 
                // storing them in a list of 'UdpProcessRecord' structure type objects. 
                for (int i = 0; i < udpRecordsTable.dwNumEntries; i++)
                {
                    MIB_UDPROW_OWNER_PID udpRow = (MIB_UDPROW_OWNER_PID)
                        Marshal.PtrToStructure(tableRowPtr, typeof(MIB_UDPROW_OWNER_PID));
                    udpTableRecords.Add(new UdpProcessRecord(new IPAddress(udpRow.localAddr),
                        BitConverter.ToUInt16(new byte[2] { udpRow.localPort[1],
                            udpRow.localPort[0] }, 0), udpRow.owningPid));
                    tableRowPtr = (IntPtr)((long)tableRowPtr + Marshal.SizeOf(udpRow));
                }
            }
            catch (OutOfMemoryException outOfMemoryException)
            {
                MessageBox.Show(outOfMemoryException.Message, "Out Of Memory",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            finally
            {
                Marshal.FreeHGlobal(udpTableRecordPtr);
            }
            return udpTableRecords != null ? udpTableRecords.Distinct()
                .ToList<UdpProcessRecord>() : new List<UdpProcessRecord>();
        }


        /// <summary> 
        /// The structure contains information that describes an IPv4 TCP connection with 
        /// IPv4 addresses, ports used by the TCP connection, and the specific process ID 
        /// (PID) associated with connection. 
        /// </summary> 
        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPROW_OWNER_PID
        {
            public MibTcpState state;
            public uint localAddr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] localPort;
            public uint remoteAddr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] remotePort;
            public int owningPid;
        }

        /// <summary> 
        /// The structure contains a table of process IDs (PIDs) and the IPv4 TCP links that 
        /// are context bound to these PIDs. 
        /// </summary> 
        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPTABLE_OWNER_PID
        {
            public uint dwNumEntries;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct,
                SizeConst = 1)]
            public MIB_TCPROW_OWNER_PID[] table;
        }

        /// <summary> 
        /// The structure contains an entry from the User Datagram Protocol (UDP) listener 
        /// table for IPv4 on the local computer. The entry also includes the process ID 
        /// (PID) that issued the call to the bind function for the UDP endpoint. 
        /// </summary> 
        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_UDPROW_OWNER_PID
        {
            public uint localAddr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] localPort;
            public int owningPid;
        }

        /// <summary> 
        /// The structure contains the User Datagram Protocol (UDP) listener table for IPv4 
        /// on the local computer. The table also includes the process ID (PID) that issued 
        /// the call to the bind function for each UDP endpoint. 
        /// </summary> 
        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_UDPTABLE_OWNER_PID
        {
            public uint dwNumEntries;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct,
                SizeConst = 1)]
            public UdpProcessRecord[] table;
        }

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int AF_INET = 2;
        public Process[] ProcessList;
        public int selectedPID = -1;
        public KeyEventArgs HotKey;
        public Thread driverThread = null;
        public List<Port> watchedPorts = new List<Port>();
        public Ping driverTerminator = new Ping();
        public bool packetsBlocked = false;
        public bool terminateDriverRequested = false;
        public bool startDriver = false;

        public enum PortType {
            TCP,
            UDP
        }

        public struct Port {
            public PortType Type;
            public UInt16 Number;
        }

        public Form1()
        {
            InitializeComponent();
            RefreshProcessList();
            new Thread(automaticInterval).Start();
        }

        /// <summary>
        /// Method used by a dedicated thread to call functions periodically
        /// and to check control conditions.
        /// </summary>
        public void automaticInterval()
        {
            while (true)
            {
                Scheduler.Refresh();
                if (Scheduler.isReady("watchForChangedPort", 5000))
                {
                    watchForChangedPort();
                }
                if (terminateDriverRequested)
                {
                    terminateDriverRequested = false;
                    terminateDriver();
                }
                if (driverThread == null && startDriver)
                {
                    startDriver = false;
                    prepareDriverStart();
                }
                Thread.Sleep(20); // 20 ms
            }
        }

        /// <summary>
        /// Start/Restart the driver if new ports have been opened/closed
        /// by the application.
        /// </summary>
        public void watchForChangedPort() {
            if (processSelected())
            {
                Debug.WriteLine("Check for changed ports");
                int PID = selectedPID;
                bool equal = true;
                List<Port> ports = getPortsOfProcess(PID);
                if (ports.Count != watchedPorts.Count)
                {
                    equal = false;
                    Debug.WriteLine("port counts doesnt equal");
                    Debug.WriteLine(ports.ToString() + "|" + watchedPorts.ToString());
                }
                else
                {
                    for (int i = 0; i < ports.Count; i++)
                    {
                        if (ports[i].Number != watchedPorts[i].Number)
                        {
                            Debug.WriteLine(ports[i].Number + " not equal " + watchedPorts[i].Number);
                            equal = false;
                            break;
                        }
                    }
                }
                if (!equal)
                {
                    startDriver = true;
                }
            }
        }

        /// <summary>
        /// Prepare the filter to the driver based on the currently
        /// used application ports.
        /// </summary>
        public void prepareDriverStart() {
            string parameter = "";
            if (!processSelected()) {
                return;
            }
            watchedPorts = getPortsOfProcess(selectedPID);
            foreach (Port port in watchedPorts)
            {
                if (parameter != "")
                {
                    parameter += " or ";
                }
                switch (port.Type)
                {
                    case PortType.TCP:
                        parameter += "tcp.DstPort = ";
                        break;
                    case PortType.UDP:
                        parameter += "udp.DstPort = ";
                        break;
                }
                parameter += port.Number;
            }
            driverThread = new Thread(watchTraffic);
            driverThread.Start(parameter);
        }

        /// <summary>
        /// Driver thread. If the hotkey is activated, it will drop the incoming packets.
        /// </summary>
        public unsafe void watchTraffic(object param) {
            string filter = (string)param;
            if (filter == "") {
                Debug.WriteLine("empty filter");
                goto Cleanup;
            }
            filter = "icmp or " + filter;
            Debug.WriteLine(filter);
            IntPtr driver = WinDivert.WinDivertOpen(filter, WinDivertLayer.Network, 0, WinDivertOpenFlags.None);
            if (driver == new IntPtr(-1))
            {
                Debug.WriteLine("Driver open failed");
                goto Cleanup;
            }
            IPAddress terminateAddress = new IPAddress(new byte[]{127,0,0,2});
            WinDivertBuffer packet = new WinDivertBuffer(0xFFFF);
            uint packetLen = 0;
            WinDivertAddress address = new WinDivertAddress();
            WinDivertParseResult parsed = new WinDivertParseResult();
            while (true)
            {
                WinDivert.WinDivertRecv(driver, packet, ref address, ref packetLen);
                parsed = WinDivert.WinDivertHelperParsePacket(packet, packetLen);
                if (parsed.IPv4Header->DstAddr.Equals(terminateAddress)) {
                    Debug.WriteLine("ICMP signalled termination");
                    break;
                }
                if (!packetsBlocked) {
                    WinDivert.WinDivertSend(driver, packet, packetLen, ref address);
                }
            }
            WinDivert.WinDivertClose(driver);

            Cleanup:
            driverThread = null;
            Debug.WriteLine("Driver terminated");
        }

        /// <summary>
        /// Stop the driver thread by sending az ICMP packet on the network.
        /// </summary>
        public void terminateDriver() {
            if (driverThread != null) {
                driverTerminator.Send("127.0.0.2", 1);
            }
        }

        /// <summary>
        /// Windows Message Queue Handler
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312) {
                if (processSelected()) {
                    togglePacketBlock();
                }
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// Block/Unblock application's internet access
        /// </summary>
        public void togglePacketBlock()
        {
            if (packetsBlocked)
            {
                packetsBlocked = false;
                blockStatus.Visible = false;
            }
            else
            {
                packetsBlocked = true;
                blockStatus.Visible = true;
            }
        }

        /// <summary>
        /// Get the ports associated with the application
        /// </summary>
        /// <param name="pid">Application Process ID</param>
        /// <returns>List of ports</returns>
        public List<Port> getPortsOfProcess(int pid) {
            List<Port> ports = new List<Port>();
            // UDP ports
            foreach (UdpProcessRecord record in GetAllUdpConnections()) {
                if (record.ProcessId == pid) {
                    Port port = new Port(){
                        Type = PortType.UDP,
                        Number = Convert.ToUInt16(record.LocalPort)
                    };
                    ports.Add(port);
                }
            }
            // TCP ports
            foreach (TcpProcessRecord record in GetAllTcpConnections())
            {
                if (record.ProcessId == pid)
                {
                    Port port = new Port()
                    {
                        Type = PortType.TCP,
                        Number = Convert.ToUInt16(record.LocalPort)
                    };
                    ports.Add(port);
                }
            }
            return ports;
        }

        /// <summary>
        /// Get the list of active processes and update the dropdown menu
        /// </summary>
        public void RefreshProcessList() {
            ProcessList = Process.GetProcesses().OrderBy(p => p.ProcessName).ToArray();
            processes.Items.Clear();
            foreach (Process p in ProcessList)
            {
                processes.Items.Add(p.ProcessName + " (" + p.Id + ")");
            }
            reselectPID();
        }

        /// <summary>
        /// This method makes sure that the right list item is
        /// selected after selectedPID is changed directly or
        /// RefreshProcessList was called
        /// </summary>
        public void reselectPID() {
            bool found = false;
            for (int i = 0; i < ProcessList.Length; i++)
            {
                if (ProcessList[i].Id == selectedPID)
                {
                    found = true;
                    processes.SelectedIndex = i;
                    selectedPID = ProcessList[i].Id;
                    startDriver = true;
                    break;
                }
            }
            if (!found) {
                selectedPID = -1;
            }
        }

        /// <summary>
        /// Checks if a process is selected from the list
        /// </summary>
        /// <returns></returns>
        public bool processSelected() {
            return selectedPID >= 0;
        }

        /// <summary>
        /// Register the user entered hotkey to Windows
        /// </summary>
        public void HotKeyChanged() {
            if (HotKey == null) {
                return;
            }

            Form1.UnregisterHotKey(this.Handle, this.GetType().GetHashCode());

            int KeyModifiers = 0x0;

            if (hotkey_ctrl.Checked) {
                KeyModifiers |= 0x2;
            }
            if (hotkey_shift.Checked) {
                KeyModifiers |= 0x4;
            }
            if (hotkey_alt.Checked) {
                KeyModifiers |= 0x1;
            }

            Boolean success = Form1.RegisterHotKey(this.Handle, this.GetType().GetHashCode(), KeyModifiers, HotKey.KeyValue);
            if (success == true)
            {
                hotkey_register_status.Text = "Registered (" + 
                    (hotkey_ctrl.Checked ? "CTRL + " : "") +
                    (hotkey_shift.Checked ? "SHIFT + " : "") +
                    (hotkey_alt.Checked ? "ALT + " : "") +
                    HotKey.KeyCode.ToString() + ")";
                hotkey_register_status.ForeColor = Color.Green;
            }
            else
            {
                hotkey_register_status.Text = "Not registered";
                hotkey_register_status.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// Hotkey changed event handler
        /// </summary>
        private void hotkey_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            if (e.KeyCode == Keys.Escape) {
                this.ActiveControl = null;
                return;
            }
            hotkey_key.Text = e.KeyCode.ToString();
            HotKey = e;
            HotKeyChanged();
        }

        /// <summary>
        /// Restart driver if a new process is selected
        /// </summary>
        private void processes_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = processes.SelectedIndex;
            if (index >= 0) {
                int PID = ProcessList[index].Id;
                selectedPID = PID;
                terminateDriverRequested = true;
                startDriver = true;
            }
        }

        private void hotkey_ctrl_checked(object sender, EventArgs e)
        {
            HotKeyChanged();
        }

        private void hotkey_shift_checked(object sender, EventArgs e)
        {
            HotKeyChanged();
        }

        private void hotkey_alt_checked(object sender, EventArgs e)
        {
            HotKeyChanged();
        }

        private void refresh_click(object sender, EventArgs e)
        {
            RefreshProcessList();
        }
    }
}
