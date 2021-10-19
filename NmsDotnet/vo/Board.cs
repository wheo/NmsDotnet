using log4net;
using Newtonsoft.Json;
using NmsDotnet.config;
using NmsDotnet.Database.vo;
using NmsDotnet.Utils;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmsDotnet.vo
{
    internal class Board
    {
        [JsonProperty("server")]
        public List<Server> server { get; set; }

        [JsonProperty("group")]
        public List<Group> group { get; set; }

        [JsonProperty("active_log")]
        public List<LogItem> log { get; set; }

        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Board()
        {
        }

        public void GetBoard()
        {
            try
            {
                string uri = string.Format($"{HostManager.getInstance().uri}/api/v1/board");

                string response = Http.Get(uri, null);
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                //DataTable dt = (DataTable)JsonConvert.DeserializeObject<DataTable>(response, settings);
                Board b = JsonConvert.DeserializeObject<Board>(response);
                server = b.server;
                group = b.group;
                log = b.log;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        public static Board instance;

        public static Board Getinstance()
        {
            if (instance == null)
            {
                instance = new Board();
            }

            return instance;
        }
    }
}