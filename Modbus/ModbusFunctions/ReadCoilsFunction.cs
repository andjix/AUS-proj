using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc/>
        public override byte[] PackRequest()
        {
            //TO DO: IMPLEMENT
            ModbusReadCommandParameters p = CommandParameters as ModbusReadCommandParameters;
            byte[] packet = new byte[12];

            // Modbus TCP header
            packet[0] = (byte)(p.TransactionId >> 8);
            packet[1] = (byte)(p.TransactionId);
            packet[2] = 0; // Protocol ID high
            packet[3] = 0; // Protocol ID low
            packet[4] = 0; // Length high
            packet[5] = 6; // Length low (6 bytes follow)
            packet[6] = p.UnitId;
            // PDU
            packet[7] = p.FunctionCode;
            packet[8] = (byte)(p.StartAddress >> 8);
            packet[9] = (byte)(p.StartAddress);
            packet[10] = (byte)(p.Quantity >> 8);
            packet[11] = (byte)(p.Quantity);

            return packet;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            //TO DO: IMPLEMENT

            ModbusReadCommandParameters p = CommandParameters as ModbusReadCommandParameters;
            var result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] == p.FunctionCode + 0x80)
            {
                HandeException(response[8]);
            }

            for (int i = 0; i < p.Quantity; i++)
            {
                int byteIndex = 9 + (i / 8);
                int bitIndex = i % 8;
                ushort value = (ushort)((response[byteIndex] >> bitIndex) & 0x01);
                ushort address = (ushort)(p.StartAddress + i);
                result.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, address), value);
            }

            return result;
        }
    }
}