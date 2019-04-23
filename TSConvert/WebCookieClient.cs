using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace TSConvert {
    class WebCookieClient : WebClient {
        //static void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e) {
        //    if (e.Error == null) {
        //        using (Stream stream = e.Result) {
        //            using (StreamReader reader = new StreamReader(stream)) {
        //                Console.WriteLine(reader.ReadToEnd());
        //            }
        //        }

        //        WebClientWithCookies client = sender as WebClientWithCookies;

        //        if (client != null) {
        //            _cookies = client.ResponseHeaders["Set-Cookie"];
        //            Console.WriteLine(_cookies);
        //        }
        //    } else {
        //        Console.WriteLine(e.Error.Message);
        //    }
        //}

        //public WebCookieClient() : base() {

        //}

        public CookieContainer _container = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address) {
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;

            if (request != null) {
                //request.Method = "Post";
                request.CookieContainer = _container;
            }

            return request;
        }
    }
}
