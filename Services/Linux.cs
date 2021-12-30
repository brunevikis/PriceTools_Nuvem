﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compass.Services
{
    public static class Linux
    {


        public static bool Run(string workfolder, string command, string name = "xxx", bool wait = true, bool ignoreQueue = false, string user = "DecompTools")
        {

            var ret = true;

            LinuxQueue40.QueueController controller = new LinuxQueue40.QueueController();

            LinuxQueue40.QueueFolders.RegisterFolder(@"Z:\cpas_ctl_common\");



            var comm = new LinuxQueue40.CommItem()
            {
                Command = command,
                CommandName = name + "_" + DateTime.Now.ToString("yyyyMMddHHmmssss"),
                Cluster = new LinuxQueue40.Cluster() { Alias = "Auto", Host = "" },
                IgnoreQueue = ignoreQueue,
                WorkingDirectory = workfolder,
                User = user
            };

            controller.Enqueue(comm);
            if (wait)
            {
                ret = controller.WaitCompletition(comm, timeout: 360000) == true &&
                    comm.ExitCode == 0;
            }

            controller = null;

            return ret;


        }
    }
}
