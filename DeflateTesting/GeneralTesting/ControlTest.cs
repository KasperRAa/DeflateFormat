using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeflateTesting.GeneralTesting
{
    [TestClass]
    public class ControlTest
    {

        [TestMethod]
        #region Test Data
        [DataRow(
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore " +
            "magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo" +
            " consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla paria" +
            "tur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
            )]
        [DataRow("0")]
        [DataRow("#############################################")]
        [DataRow("1234567890987654321|213243546576879809908978675645342312")]
        [DataRow(
            "0123456789qwertyuiopåasdfghjklæøzxcvbnm,.-'¨´+!\"#¤%&/()=?`QWERTYUIOPÅ^ASDFGHJKLÆØ*ZXCVBNM;:_@£$€{[]}|€~\\><§½"
            )]
        [DataRow(
            "0123456789qwertyuiopåasdfghjklæøzxcvbnm,.-'¨´+!\"#¤%&/()=?`QWERTYUIOPÅ^ASDFGHJKLÆØ*ZXCVBNM;:_@£$€{[]}|€~\\><§½" +
            "½§<>\\~?|}][{?$£@_:;MNBVCXZ*ØÆLKJHGFDSA^ÅPOIUYTREWQ`?=)(/&%¤#\"!+´¨'-.,mnbvcxzøælkjhgfdsaåpoiuytrewq9876543210"
            )]
        [DataRow(
            "00112233445566778899qqwweerrttyyuuiiooppååaassddffgghhjjkkllææøøzzxxccvvbbnnmm,,..--''¨¨´´++!!\"\"##¤¤%%&&//((" +
            "))==??``QQWWEERRTTYYUUIIOOPPÅÅ^^AASSDDFFGGHHJJKKLLÆÆØØ**ZZXXCCVVBBNNMM;;::__@@££$$??{{[[]]}}||??~~\\\\>><<§§½½" +
            "½½§§<<>>\\\\~~??||}}]][[{{??$$££@@__::;;MMNNBBVVCCXXZZ**ØØÆÆLLKKJJHHGGFFDDSSAA^^ÅÅPPOOIIUUYYTTRREEWWQQ``??==))" +
            "((//&&%%¤¤##\"\"!!++´´¨¨''--..,,mmnnbbvvccxxzzøøæællkkjjhhggffddssaaååppooiiuuyyttrreewwqq99887766554433221100"
            )]
        [DataRow(
            "000111222333444555666777888999qqqwwweeerrrtttyyyuuuiiiooopppåååaaasssdddfffggghhhjjjkkklllæææøøøzzzxxxcccvvvbb" +
            "bnnnmmm,,,...---'''¨¨¨´´´+++!!!\"\"\"###¤¤¤%%%&&&///((()))===???```QQQWWWEEERRRTTTYYYUUUIIIOOOPPPÅÅÅ^^^AAASSSD" +
            "DDFFFGGGHHHJJJKKKLLLÆÆÆØØØ***ZZZXXXCCCVVVBBBNNNMMM;;;:::___@@@£££$$$???{{{[[[]]]}}}|||???~~~\\\\\\>>><<<§§§½½½" +
            "½½½§§§<<<>>>\\\\\\~~~???|||}}}]]][[[{{{???$$$£££@@@___:::;;;MMMNNNBBBVVVCCCXXXZZZ***ØØØÆÆÆLLLKKKJJJHHHGGGFFFDD" +
            "DSSSAAA^^^ÅÅÅPPPOOOIIIUUUYYYTTTRRREEEWWWQQQ```???===)))(((///&&&%%%¤¤¤###\"\"\"!!!+++´´´¨¨¨'''---...,,,mmmnnnb" +
            "bbvvvcccxxxzzzøøøææælllkkkjjjhhhgggfffdddsssaaaåååpppoooiiiuuuyyytttrrreeewwwqqq999888777666555444333222111000"
            )]
        [DataRow("Fucking finally. It has been taking me a while, but now I can start working on my own implementation of the Deflate format.")]
        [DataRow("")]
        #endregion
        public void TestInbuiltDeflate(string str)
        {
            byte[] s_messageBytes = Encoding.UTF8.GetBytes(str);
            byte[] compressedBytes = InbuiltDeflate.Compress(s_messageBytes);
            byte[] bytes = InbuiltDeflate.Decompress(compressedBytes);
            Assert.AreEqual(str, Encoding.UTF8.GetString(bytes));
        }

    }
}
