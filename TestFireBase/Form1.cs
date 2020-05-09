using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft;
using Newtonsoft.Json;

namespace TestFireBase
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            sendRequest("AAAAd9PZttQ:APA91bExL6s4e0llTRz2v8y3CYz-s91urad9UWxvBJgjJikj7jKTNj2u4vKR6Y6dPUMtw3ZBLv690BbhANpVsQaslqPRuZFO5g6Po3ngyki6v5QWMnFI2QRlrgQif54yxQ_8XY15RwTp"
                , "514655368916"
                , "cXH0Z35JNRE:APA91bHI53pIS4cQSL0wUKzc9bYX-s_qFvf1VRLx2VA58HcQEaCH9YZLlLAPkMSuXSA2Z1Azx0ZZ4YB7BQyUkfggBzd8acIdW-XptvsDqGVe6BZ7b6S2rXgGkhejaszEgFmYkwLYLoye"
                , "Probando", "Probando");
        }
        public void sendRequest(string serverkey, string senderId, string Device, string t, string b)
        {
            WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
            tRequest.Method = "post";
            //serverKey - Key from Firebase cloud messaging server  
            tRequest.Headers.Add(string.Format("Authorization: key={0}", serverkey));
            //Sender Id - From firebase project setting  
            tRequest.Headers.Add(string.Format("Sender: id={0}", senderId));
            tRequest.ContentType = "application/json";
            var payload = new
            {
                to = Device,
                priority = "high",
                content_available = true,
                notification = new
                {
                    body = b,
                    title = t,
                    badge = 1
                },
                data = new
                {
                    key1 = "value1",
                    key2 = "value2"
                }

            };

            string postbody = JsonConvert.SerializeObject(payload).ToString();
            Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
            tRequest.ContentLength = byteArray.Length;
            using (Stream dataStream = tRequest.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
                using (WebResponse tResponse = tRequest.GetResponse())
                {
                    using (Stream dataStreamResponse = tResponse.GetResponseStream())
                    {
                        if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {
                                String sResponseFromServer = tReader.ReadToEnd();
                                //result.Response = sResponseFromServer;
                            }
                    }
                }
            }

        }
    }
}
