﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmsDotNet.config
{
    internal class JsonConfig
    {
        public String ip { get; set; }
        public int port { get; set; }
        public String id { get; set; }
        public String pw { get; set; }
        public String DatabaseName { get; set; }

        public String configFileName = "config.json";

        public static JsonConfig instance;

        public static JsonConfig getInstance()
        {
            if (instance == null)
            {
                instance = new JsonConfig();
            }
            return instance;
        }
    }
}