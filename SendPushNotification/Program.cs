using System;

namespace SendPushNotification
{
    class Program
    {
        static  void Main(string[] args)
        {
           string a =  SendNotification(new System.Collections.Generic.List<string>() { "" },"","");
        }

        public string SendNotification(System.Collections.Generic.List<string> clientToken, string title, string body)
        {
            var registrationTokens = clientToken;
            var message = new FirebaseAdmin.Messaging.MulticastMessage()
            {
                Tokens = registrationTokens,
                Data = new System.Collections.Generic.Dictionary<string, string>()
                {
                    {"title", title},
                    {"body", body},
                },
            };
            var response = await FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance.SendMulticastAsync(message).ConfigureAwait(true);
            return "";
        }
    }
}
