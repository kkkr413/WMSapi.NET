using System.Data.SqlTypes;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace WMSapi.Service
{


  
    public class ZPLservice 
    {

        private int PORT = 9100;
        private int TIMEOUT = 3000;
        public bool PrintZPL(string ipAddress, string zplString )
        {
            try
            {
                using var client = new TcpClient();
                client.SendTimeout = TIMEOUT; 

                if (client.ConnectAsync(ipAddress, PORT).Wait(client.SendTimeout))
                {
                    using var stream = client.GetStream();
                    byte[] data = Encoding.ASCII.GetBytes(zplString);
                    stream.Write(data, 0, data.Length);

                    return true;
                }
             
               
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while sending ZPL to the printer: " + ex.Message);
                return false;
            }
            return false;
        }

        public string PAL_zpl(string ipAddress , string data , string data2 , string quantity)
        {


            string zplString =
                                 "^XA" +
                                 "^MMT" +
                                 "^PW336" +
                                 "^LL575" +
                                 "^LS0" +
                                $"^FT24,67^A0N,26,43^FH\\^CI28^FD{data} " +
                                 "^FS^CI27" +
                                $"^FT28,121^A0N,29,48^FH\\^CI28^FD{data2} " +
                                 "^FS^CI27" +
                                 "^FT92,340^BQN,2,7" +
                                $"^FH\\^FDLA,{data}" +
                                 "^FS" +
                                 "^FO24,77^GB291,0,2^FS" +
                                $"^PQ{quantity}" +
                                 ",0,1,Y" +
                                 "^XZ";

            var printer = new ZPLservice();
            bool res =  printer.PrintZPL(ipAddress, zplString);

            return (res == true ? "인쇄요청에 성공 했습니다" : "인쇄요청에 실패 했습니다. TCP연결을 확인 하십시오.");


        }
    }
}
