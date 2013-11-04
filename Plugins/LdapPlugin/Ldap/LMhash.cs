//Copyright (c) 2009 John Leitch john.leitch5@gmail.com
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace pGina.Plugin.Ldap
{
    class LMhash
    {
        private static readonly byte[] plainText = { 0x4B, 0x47, 0x53, 0x21, 0x40, 0x23, 0x24, 0x25 };

        public static byte[] ComputeHalf(byte[] Half)
        {
            // Sidestep 'Specified key is a known weak key' exception and
            // speed things up
            if (Half.Length == 0)
                return new byte[] { 0xAA, 0xD3, 0xB4, 0x35, 0xB5, 0x14, 0x04, 0xEE };
            else if (Half.Length > 7)
                throw new NotSupportedException("Password halves greater than 7 " +
                    "characters are not supported");

            Array.Resize(ref Half, 7);

            string[] binary = new string[7]
            {
                Convert.ToString(Half[0], 2),
                Convert.ToString(Half[1], 2),
                Convert.ToString(Half[2], 2),
                Convert.ToString(Half[3], 2),
                Convert.ToString(Half[4], 2),
                Convert.ToString(Half[5], 2),
                Convert.ToString(Half[6], 2),
            };

            string binaryString =
                new string('0', 8 - binary[0].Length) + binary[0] +
                new string('0', 8 - binary[1].Length) + binary[1] +
                new string('0', 8 - binary[2].Length) + binary[2] +
                new string('0', 8 - binary[3].Length) + binary[3] +
                new string('0', 8 - binary[4].Length) + binary[4] +
                new string('0', 8 - binary[5].Length) + binary[5] +
                new string('0', 8 - binary[6].Length) + binary[6];

            DESCryptoServiceProvider des = new DESCryptoServiceProvider()
            {
                IV = new byte[8],
                Key = new byte[8]
                {
                    (byte)((binaryString[0] == '1' ? 128 : 0) +
                    (binaryString[1] == '1' ? 64 : 0) +
                    (binaryString[2] == '1' ? 32 : 0) +
                    (binaryString[3] == '1' ? 16 : 0) +
                    (binaryString[4] == '1' ? 8 : 0) +
                    (binaryString[5] == '1' ? 4 : 0) +
                    (binaryString[6] == '1' ? 2 : 0)),
                    (byte)((binaryString[7] == '1' ? 128 : 0) +
                    (binaryString[8] == '1' ? 64 : 0) +
                    (binaryString[9] == '1' ? 32 : 0) +
                    (binaryString[10] == '1' ? 16 : 0) +
                    (binaryString[11] == '1' ? 8 : 0) +
                    (binaryString[12] == '1' ? 4 : 0) +
                    (binaryString[13] == '1' ? 2 : 0)),
                    (byte)((binaryString[14] == '1' ? 128 : 0) +
                    (binaryString[15] == '1' ? 64 : 0) +
                    (binaryString[16] == '1' ? 32 : 0) +
                    (binaryString[17] == '1' ? 16 : 0) +
                    (binaryString[18] == '1' ? 8 : 0) +
                    (binaryString[19] == '1' ? 4 : 0) +
                    (binaryString[20] == '1' ? 2 : 0)),
                    (byte)((binaryString[21] == '1' ? 128 : 0) +
                    (binaryString[22] == '1' ? 64 : 0) +
                    (binaryString[23] == '1' ? 32 : 0) +
                    (binaryString[24] == '1' ? 16 : 0) +
                    (binaryString[25] == '1' ? 8 : 0) +
                    (binaryString[26] == '1' ? 4 : 0) +
                    (binaryString[27] == '1' ? 2 : 0)),
                    (byte)((binaryString[28] == '1' ? 128 : 0) +
                    (binaryString[29] == '1' ? 64 : 0) +
                    (binaryString[30] == '1' ? 32 : 0) +
                    (binaryString[31] == '1' ? 16 : 0) +
                    (binaryString[32] == '1' ? 8 : 0) +
                    (binaryString[33] == '1' ? 4 : 0) +
                    (binaryString[34] == '1' ? 2 : 0)),
                    (byte)((binaryString[35] == '1' ? 128 : 0) +
                    (binaryString[36] == '1' ? 64 : 0) +
                    (binaryString[37] == '1' ? 32 : 0) +
                    (binaryString[38] == '1' ? 16 : 0) +
                    (binaryString[39] == '1' ? 8 : 0) +
                    (binaryString[40] == '1' ? 4 : 0) +
                    (binaryString[41] == '1' ? 2 : 0)),
                    (byte)((binaryString[42] == '1' ? 128 : 0) +
                    (binaryString[43] == '1' ? 64 : 0) +
                    (binaryString[44] == '1' ? 32 : 0) +
                    (binaryString[45] == '1' ? 16 : 0) +
                    (binaryString[46] == '1' ? 8 : 0) +
                    (binaryString[47] == '1' ? 4 : 0) +
                    (binaryString[48] == '1' ? 2 : 0)),
                    (byte)((binaryString[49] == '1' ? 128 : 0) +
                    (binaryString[50] == '1' ? 64 : 0) +
                    (binaryString[51] == '1' ? 32 : 0) +
                    (binaryString[52] == '1' ? 16 : 0) +
                    (binaryString[53] == '1' ? 8 : 0) +
                    (binaryString[54] == '1' ? 4 : 0) +
                    (binaryString[55] == '1' ? 2 : 0)),
                }
            };

            ICryptoTransform transform = des.CreateEncryptor();

            byte[] b = transform.TransformFinalBlock(plainText, 0, 8);

            return new byte[8] { b[0], b[1], b[2], b[3], b[4], b[5],
                b[6], b[7] };
        }

        public static byte[] Compute(string Password)
        {
            if (Password.Length > 14)
                throw new NotSupportedException("Passwords greater than 14 " +
                    "characters are not supported");

            byte[] passBytes = Encoding.ASCII.GetBytes(Password.ToUpper());

            byte[][] passHalves = new byte[2][];

            if (passBytes.Length > 7)
            {
                int len = passBytes.Length - 7;

                passHalves[0] = new byte[7];
                passHalves[1] = new byte[len];

                Array.Copy(passBytes, passHalves[0], 7);
                Array.Copy(passBytes, 7, passHalves[1], 0, len);
            }
            else
            {
                passHalves[0] = passBytes;
                passHalves[1] = new byte[0];
            }

            for (int x = 0; x < 2; x++)
                passHalves[x] = ComputeHalf(passHalves[x]);

            byte[] hash = new byte[16];

            Array.Copy(passHalves[0], hash, 8);
            Array.Copy(passHalves[1], 0, hash, 8, 8);

            return hash;
        }
    }
}
