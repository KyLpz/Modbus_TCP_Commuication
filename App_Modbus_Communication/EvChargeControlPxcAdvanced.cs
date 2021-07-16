using NModbus;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace App_Modbus_Communication
{
    /// <summary>
    /// Communiction for:  PHOENIX CONTACT EV Charge Control EM-CP-PP-ETH
    /// </summary>
    public class EvChargeControlPxcAdvanced
    {
        public EvChargeControlPxcAdvanced(IPAddress ipAddress, byte slaveID, int port)
        {
            SlaveID = slaveID;
            IpAddress = ipAddress;
            Port = port;
        }
        public byte SlaveID { get; set; }
        public int Port { get; set; }
        public IPAddress IpAddress { get; set; }
        private bool PingAddress()
        {
            Ping pingSender = new Ping();
            string data = "SverrissagaSverrissagaSverrissag";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 1000;
            PingReply reply = pingSender.Send(IpAddress, timeout, buffer);

            if (reply.Status == IPStatus.Success)
            {
                return true;
            }
            else
            {
                System.Diagnostics.Trace.WriteLine($"Ping to IP-Addres {IpAddress} failed, status: {reply.Status}");
                return false;
            }

        }
        private IModbusMaster SetModbusParameter(TcpClient masterTcpClient)
        {
            var factory = new ModbusFactory();
            var modbusMaster = factory.CreateMaster(masterTcpClient);
            modbusMaster.Transport.ReadTimeout = 200;
            modbusMaster.Transport.Retries = 2;
            modbusMaster.Transport.WriteTimeout = 200;
            return modbusMaster;
        }
        /// <summary>
        /// Get the intaniousStatus of the charge controller
        /// </summary>
        /// <returns> 
        /// string: EvStatus: A...F
        /// int: PWM charge current 6....32 
        /// </returns>
        public async Task<Tuple<string, int>> GetEvStatusAsync()
        {
            if (PingAddress())
            {
                try
                {
                    using (TcpClient masterTcpClient = new TcpClient(IpAddress.ToString(), Port))
                    {

                        var modbusMaster = SetModbusParameter(masterTcpClient);
                        var evStatus = await modbusMaster.ReadInputRegistersAsync(SlaveID, 100, 1);
                        var chargeCurrent = await modbusMaster.ReadHoldingRegistersAsync(SlaveID, 300, 1);
                        return new Tuple<string, int>(Convert.ToChar(evStatus[0]).ToString(), chargeCurrent[0]);

                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($" GetEvStatusAsync failed, Message: {ex.Message} ");
                    return null;
                }
            }
            else
            {
                throw new WebException("Device is not reachable.", WebExceptionStatus.ConnectFailure);
            }

        }
        public async Task SetChargeCurrentAsync(int chargeCurrent)
        {
            if (PingAddress())
            {
                try
                {
                    using (TcpClient masterTcpClient = new TcpClient(IpAddress.ToString(), Port))
                    {

                        var modbusMaster = SetModbusParameter(masterTcpClient);
                        ushort[] writeValue = new ushort[] { 6 };
                        if (chargeCurrent <= 32 && chargeCurrent >= 6)
                        {
                            writeValue[0] = (ushort)chargeCurrent;
                        }
                        else if (chargeCurrent > 32)
                        {
                            writeValue[0] = 32;
                        }
                        await modbusMaster.WriteMultipleRegistersAsync(SlaveID, 300, writeValue);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($" SetChargeCurrentAsync failed, Message: {ex.Message} ");
                }
            }
            else
            {
                throw new WebException("Device is not reachable.", WebExceptionStatus.ConnectFailure);
            }

        }
        public async Task SetEnableChargingProcess(bool isEnabled)
        {
            if (PingAddress())
            {
                try
                {
                    using (TcpClient masterTcpClient = new TcpClient(IpAddress.ToString(), Port))
                    {

                        var modbusMaster = SetModbusParameter(masterTcpClient);
                        await modbusMaster.WriteSingleCoilAsync(SlaveID, 400, isEnabled);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($" SetChargeCurrentAsync failed, Message: {ex.Message} ");
                }
            }
            else
            {
                throw new WebException("Device is not reachable.", WebExceptionStatus.ConnectFailure );
            }

        }
    }
}